@echo off
cls
set NUGETPATH="%~dp0%build\tools\nuget"
for %%X in (nuget.exe) do (set NUGET=%%~$PATH:X)
if not defined NUGET (
    mkdir %NUGETPATH%
    ECHO NuGet not found .. downloading
    PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '.\build\download-nuget.ps1' -OutputPath %NUGETPATH%\nuget.exe"
    ECHO done
    set NUGET="%NUGETPATH%\nuget.exe"
)

if not exist build\tools\FAKE\tools\Fake.exe (
    ECHO FAKE not found .. installing
    %NUGET% "install" "FAKE" -OutputDirectory "build\tools" -ExcludeVersion -Version 3.35.2
)


SET TARGET="Default"

if NOT [%1]==[] (set TARGET="%1")

shift

"build\tools\FAKE\tools\FAKE.exe" "build.fsx" "%TARGET%" %*

