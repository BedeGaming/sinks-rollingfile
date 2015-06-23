#r "build/tools/FAKE/tools/FakeLib.dll"

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

let releaseNotes =
    match isLocalBuild with
    | true  -> ReleaseNotes.New("2.0.0", "2.0.0-pre", ["local developer build"])
    | false ->
        match isRelease with
        | true ->
            ReadFile "docs/RELEASE_NOTES.md"
            |> parseReleaseNotes
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

Target "Default" DoNothing

"PatchAssemblyInfo" ==> "Build"
"InstallNunit" ==> "ExecuteTests"
"BuildTests" ==> "ExecuteTests"
"Build" ==> "BuildTests"
"ExecuteTests" ==> "PackageNuget"
"Build" ==> "PackageNuget"

RestorePackages()

RunTargetOrDefault "PackageNuget"

