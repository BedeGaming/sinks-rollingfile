#r "./build/tools/FAKE/tools/FakeLib.dll"

let sd = __SOURCE_DIRECTORY__
let projectName = "Serilog.Sinks.RollingFileAlternate"

open Fake
open System
open System.Collections.Generic
open System.IO

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

// helpers

let inline FileName fullName = Path.GetFileName fullName

let BuildFailed errors =
    raise (BuildException("The project build failed.", errors |> List.ofSeq))

let DeployFailed errors =
    raise (BuildException("The project deployment failed.", errors |> List.ofSeq))

let TestsFailed errors =
    raise (BuildException("The project tests failed.", errors |> List.ofSeq))

let Run workingDirectory fileName args =
    let errors = new List<string>()
    let messages = new List<string>()
    let timout = TimeSpan.MaxValue
        
    let error msg =
        traceError msg
        errors.Add msg
        
    let message msg =
        traceImportant msg
        messages.Add msg
        
    let code = 
        ExecProcessWithLambdas (fun info ->
            info.FileName <- fileName
            info.WorkingDirectory <- workingDirectory
            info.Arguments <- args
        ) timout true error message
    
    ProcessResult.New code messages errors

let dotnet failedF args =
    let result = Run currentDirectory "dotnet" args
    if not result.OK then failedF result.Errors

let BuildProject project =
    dotnet BuildFailed ("pack \"" + project + "\" -c Release")

let RestoreProject backup =
    if endsWith ".bak" backup then
        CopyFile (replace ".bak" "" backup) backup
        DeleteFile backup

Target "Clean" (fun _ ->
    !! "artifacts" ++ "src/*/bin" ++ "test/*/bin"
        |> DeleteDirs
)

Target "RestoreDependencies" (fun _ ->
    dotnet BuildFailed "restore"
)

Target "BuildProjects" (fun _ ->
    !! "src/*/project.json" 
        |> Seq.iter(BuildProject)
)

Target "CopyArtifacts" (fun _ ->    
    !! "src/*/bin/**/*.nupkg" 
        |> Seq.iter(CopyArtifact)
)

Target "RunTests" (fun _ ->
    !! "test/*/project.json" 
        |> Seq.iter(RunTests)
)

FinalTarget "RestoreProjects" (fun _ ->
    !! "src/*/project.json.bak" ++ "src/*/project.lock.json.bak" ++ "test/*/project.json.bak" ++ "test/*/project.lock.json.bak"
        |> Seq.iter(RestoreProject)
)

open System.Text.RegularExpressions

Target "Publish" <| fun _ ->

    if not (hasBuildParam "NugetApiKey") then
        failwith "Nuget api key needs to be set"

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
    let haveMatchingPackage =
        builtPackages
        |> Seq.exists (fun x -> x = releaseNotes.SemVer)

    if not haveMatchingPackage then
        failwith <| sprintf
            "No package found that matches the version in the release notes: [ %s] - \
            Have you not built the release version yet?"
            (string releaseNotes.SemVer)


Target "Build" (fun _ ->)

"Clean"
  ==> "BackupProjects"
  ==> "UpdateVersions"
  ==> "RestoreDependencies"
  ==> "BuildProjects"
  ==> "CopyArtifacts"
  ==> "RunTests"
  ==> "Build"
