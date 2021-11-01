$currentDirectory = $PSScriptRoot

$logfile = "$currentDirectory\upgrade_log.txt"
Function Write-Log
{
   Param ([string]$logString)
   Add-content $logfile -Value $logString
   Write-Host $logString
}

Write-Log "Starting update script in $currentDirectory"

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

Write-Log "Stopping Datack..."

Stop-Service Datack

Write-Log "Stopped Datack"

$releasesUri = "https://api.github.com/repos/rogerfar/datack/releases/latest"
$downloadUri = ((Invoke-RestMethod -Method GET -Uri $releasesUri).assets | Where-Object name -like "*Server.zip").browser_download_url

$tempPath = $([System.IO.Path]::GetTempPath())
$tempName = $(Split-Path -Path $downloadUri -Leaf)

Write-Log "Downloading $downloadUri to $tempPath\$tempName"

$pathZip = Join-Path -Path $tempPath -ChildPath $tempName

Invoke-WebRequest -Uri $downloadUri -Out $pathZip

$tempExtract = Join-Path -Path $tempPath -ChildPath $((New-Guid).Guid)

Write-Log "Extracting $pathZip to $tempExtract"

Expand-Archive -Path $pathZip -DestinationPath $tempExtract -Force

Write-Log "Backing up appsettings.json"

Copy-Item -Path "$currentDirectory\appsettings.json" -Destination $tempExtract -Force 

Write-Log "Copying new files"

Copy-Item -Path "$tempExtract\*" -Destination $currentDirectory -Force -Recurse

Write-Log "Removing temp files from $tempExtract"

Remove-Item -Path $tempExtract -Force -Recurse -ErrorAction SilentlyContinue

Write-Log "Removing $pathZip"

Remove-Item $pathZip -Force

Write-Log "Starting Datack..."

Start-Service Datack

Write-Log "Started Datack"