#r "./build/tools/FAKE/tools/FakeLib.dll"

let sd = __SOURCE_DIRECTORY__
let projectName = "Serilog.Sinks.RollingFileAlternate"

open Fake

let buildDir = sd @@ "build"
let packageOutFolder = buildDir @@ "package" @@ "out"
let toolsPath = buildDir @@ "tools"

open EnvironmentHelper
let isRelease = getBuildParamOrDefault "IS_RELEASE" "false" |> System.Boolean.Parse

open ReleaseNotesHelper
open BuildServerHelper

let getReleaseNotesFromFile () =
    ReadFile "docs/RELEASE_NOTES.md"
    |> parseReleaseNotes

let releaseNotes =
    match isRelease with
    | true -> getReleaseNotesFromFile ()
    | false ->
        match isLocalBuild with
        | true  -> ReleaseNotes.New("2.0.0", "2.0.0-pre", ["local developer build"])
        | false ->
            ReleaseNotes.New(
                buildVersion,
                sprintf "2.0.0-pre_%s" buildVersion,
                ["continuous integration build"])

open AssemblyInfoFile

Target "PatchAssemblyInfo" <| fun _ ->

    let attributes =
        [   Attribute.Version releaseNotes.AssemblyVersion
            Attribute.FileVersion releaseNotes.NugetVersion
            Attribute.InformationalVersion releaseNotes.AssemblyVersion ]

    let config = AssemblyInfoFileConfig(false)
    let pathToPatch = sd @@ "assets" @@ "CommonAssemblyInfo.cs"

    CreateCSharpAssemblyInfoWithConfig pathToPatch attributes config

Target "Clean"

Target "Build" <| fun _ ->
    !! "src/**/*.csproj"
    |> MSBuildRelease "" "Rebuild"
    |> Log "Building Sink"

Target "BuildTests" <| fun _ ->
    !! "test/**/*.csproj"
    |> MSBuildRelease "build/out/tests" "Build"
    |> Log "Building Tests"

Target "InstallNunit" <| fun _ ->
        RestorePackageId (
            fun p ->
                { p with
                    OutputPath = toolsPath @@ "nunit-runner"
                    ExcludeVersion = true })
            "NUnit.Runners"

Target "ExecuteTests" <| fun _ ->
    !! "build/out/tests/*.dll"
    |> NUnit (
        fun p ->
            { p with
                ToolPath = toolsPath @@ "nunit-runner/NUnit.Runners/tools"
                Framework = "4.5" })

open NuGetHelper
Target "PackageNuget" <| fun _ ->

    let parm p =
        { p with
            WorkingDir = sd
            Properties = [ "Configuration", "Release" ]
            SymbolPackage = NugetSymbolPackage.Nuspec
            Version = releaseNotes.NugetVersion
            OutputPath = "build" @@ "package " @@ "out" }
    NuGetPack parm (sd @@ "src" @@ "Serilog.Sinks.RollingFileAlternate" @@ "Serilog.Sinks.RollingFileAlternate.csproj")

open System.Text.RegularExpressions

let haveReleasePackage =
    let getPackage pkg =
        let packageRegex =
             "(?<name>([\w|\.]+?))\.(?<number>([\d|\.]+){2,4})\.nupkg"
        let matches = Regex.Match(pkg, packageRegex)
        matches.Groups.["number"].Value

    let builtPackages =
        let packageIncludes =
            !! ( packageOutFolder @@ "*.nupkg" )
            -- ( packageOutFolder @@ "*.symbols.nupkg" )
        packageIncludes
        |> Seq.map (getPackage >> SemVerHelper.parse)

    let releaseNotes = getReleaseNotesFromFile ()
    builtPackages
    |> Seq.exists (fun x -> x = releaseNotes.SemVer)


Target "Publish" <| fun _ ->

    if not (hasBuildParam "NugetApiKey") then
        failwith "Nuget api key needs to be set"


let nunitIsInstalled = 
    fileExists (toolsPath @@ "nunit-runner/Nunit.Runners/tools/nunit-console.exe")

Target "Default" DoNothing

"PatchAssemblyInfo" ==> "Build" |> ignore
"InstallNunit" =?> ("ExecuteTests", not nunitIsInstalled) |> ignore
"BuildTests" ==> "ExecuteTests" |> ignore
"Build" ==> "BuildTests" |> ignore
"ExecuteTests" ==> "PackageNuget" |> ignore
"Build" ==> "PackageNuget" ==> "Default" |> ignore
"PackageNuget" =?> ("Publish", haveReleasePackage) |> ignore

sprintf "Building version: %s" (string releaseNotes.SemVer) |> traceImportant
RestorePackages()
RunTargetOrDefault "Default"

