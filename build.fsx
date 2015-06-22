#r "build/tools/FAKE/tools/FakeLib.dll"

let sd = __SOURCE_DIRECTORY__

open Fake

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


Target "Build" <| fun _ ->
    !! "src/**/*.csproj"
    |> MSBuildRelease "" "Rebuild"
    |> Log "Building Sink"

Target "Default" DoNothing

"PatchAssemblyInfo"
    ==> "Build"
    ==> "Default"

RunTargetOrDefault "Default"
