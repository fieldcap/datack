$currentDirectory = $PSScriptRoot

Write-Host "Starting update script in $currentDirectory"

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

Write-Host "Stopping Datack Agent..."

Stop-Service "Datack Agent"

Write-Host "Stopped Datack Agent"

$releasesUri = "https://api.github.com/repos/rogerfar/datack/releases/latest"
$downloadUri = ((Invoke-RestMethod -Method GET -Uri $releasesUri).assets | Where-Object name -like "*Agent.zip").browser_download_url

Write-Host "Downloading $downloadUri"

$pathZip = Join-Path -Path $([System.IO.Path]::GetTempPath()) -ChildPath $(Split-Path -Path $downloadUri -Leaf)

Invoke-WebRequest -Uri $downloadUri -Out $pathZip

$tempExtract = Join-Path -Path $([System.IO.Path]::GetTempPath()) -ChildPath $((New-Guid).Guid)

Write-Host "Extracting to $tempExtract"

Expand-Archive -Path $pathZip -DestinationPath $tempExtract -Force

Write-Host "Backing up appsettings.json"

Copy-Item -Path "$currentDirectory\appsettings.json" -Destination $tempExtract -Force 

Write-Host "Moving new files"

Copy-Item -Path "$tempExtract\*" -Destination $currentDirectory -Force -Recurse

Write-Host "Removing temp files"

Remove-Item -Path $tempExtract -Force -Recurse -ErrorAction SilentlyContinue

Remove-Item $pathZip -Force

Write-Host "Starting Datack..."

Start-Service "Datack Agent"

Write-Host "Started Datack Agent"