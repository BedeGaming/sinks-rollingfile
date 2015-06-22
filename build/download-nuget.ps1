Param( [String] $OutputPath = '.\build\tools\nuget.exe' )
$source = "http://nuget.org/nuget.exe"
Write-Output "Download nuget to: $OutputPath"

$wc = New-Object System.Net.WebClient
$wc.DownloadFile($source, $OutputPath)

