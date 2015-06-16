param(
    [String] $majorMinor = "0.0",            # 2.0
    [String] $patch = "0",                   # $env:APPVEYOR_BUILD_VERSION
    [String] $customLogger = "",             # C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll
    [String] $nugetExecutable = "nuget",     # teamcity
    [String] $msbuildExecutable = "msbuild", # teamcity
    [Switch] $notouch
)

function Set-AssemblyVersions($informational, $assembly)
{
    (Get-Content assets/CommonAssemblyInfo.cs) |
        ForEach-Object { $_ -replace """1.0.0.0""", """$assembly""" } |
        ForEach-Object { $_ -replace """1.0.0""", """$informational""" } |
        ForEach-Object { $_ -replace """1.1.1.1""", """$($informational).0""" } |
        Set-Content assets/CommonAssemblyInfo.cs
}

function Install-NuGetPackages($solution)
{
    &($nugetExecutable) restore "$solution"
}

function Invoke-MSBuild($solution, $customLogger)
{
    if ($customLogger)
    {
        &($msbuildExecutable) "$solution" /verbosity:minimal /p:Configuration=Release /logger:"$customLogger"
    }
    else
    {
        &($msbuildExecutable) "$solution" /verbosity:minimal /p:Configuration=Release
    }
}

function Invoke-NuGetPackProj($csproj)
{
    &($nugetExecutable) pack -Prop Configuration=Release -Symbols $csproj
}

function Invoke-NuGetPackSpec($nuspec, $version)
{
    &($nugetExecutable) pack $nuspec -Version $version -OutputDirectory .\ -Prop Configuration=Release -Symbols
}

function Invoke-NuGetPack($version)
{
    ls src/**/*.csproj |
        Where-Object { -not ($_.Name -like "*net40*") } |
        ForEach-Object { Invoke-NuGetPackSpec $_ $version }
}

function Invoke-Build($project, $majorMinor, $patch, $customLogger, $notouch)
{
    $solution = "$project.sln"
    $solution4 = "$project-net40.sln"
    $package="$majorMinor.$patch"

    Write-Output "Building $project $package"

    if (-not $notouch)
    {
        $assembly = "$majorMinor.0.0"

        Write-Output "Assembly version will be set to $assembly"
        Set-AssemblyVersions $package $assembly
    }

    Install-NuGetPackages $solution
    
    Invoke-MSBuild $solution $customLogger

    Invoke-NuGetPack $package
}

$ErrorActionPreference = "Stop"
Invoke-Build "serilog-sinks-rollingfile" $majorMinor $patch $customLogger $notouch

