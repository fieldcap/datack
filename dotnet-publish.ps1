$utf8NoBomEncoding = New-Object System.Text.UTF8Encoding $False

$version = (Get-Content "package.json" | ConvertFrom-Json).version

$csProjServer = "$pwd\src\dotnet\Datack.Web.Web\Datack.Web.Web.csproj"
$csProjAgent = "$pwd\src\dotnet\Datack.Agent.Service\Datack.Agent.csproj"
$navbar = "$pwd\src\react\src\pages\settings\SettingsOverview.tsx";

$newCsProjServer = (Get-Content $csProjServer) -replace '<Version>.*?<\/Version>', "<Version>$version</Version>" 
[System.IO.File]::WriteAllLines($csProjServer, $newCsProjServer, $utf8NoBomEncoding)

$newCsProjAgent = (Get-Content $csProjAgent) -replace '<Version>.*?<\/Version>', "<Version>$version</Version>" 
[System.IO.File]::WriteAllLines($csProjAgent, $newCsProjAgent, $utf8NoBomEncoding)

$newNavbar = (Get-Content $navbar) -replace '<span id="version">.*?<\/span>', "<span id=""version"">$version</span>"
[System.IO.File]::WriteAllLines($navbar, $newNavbar, $utf8NoBomEncoding)

cd src
cd react
npm install
npm run build

cd ..
cd dotnet/Datack.Web.Web
dotnet build Datack.Web.Web.csproj
dotnet publish Datack.Web.Web.csproj -c Release -o ..\..\..\out\server

cd ..
cd Datack.Agent.Service
dotnet build Datack.Agent.csproj
dotnet publish Datack.Agent.csproj -c Release -o ..\..\..\out\agent

cd ..
cd ..
cd ..
cd out/server

$location = Get-Location
[string]$Zip = "C:\Program Files\7-Zip\7z.exe"
[array]$arguments = "a", "-tzip", "-y", "$location/../../Datack.Server.zip", "."
& $Zip $arguments

cd ..
cd agent

$location = Get-Location
[string]$Zip = "C:\Program Files\7-Zip\7z.exe"
[array]$arguments = "a", "-tzip", "-y", "$location/../../Datack.Agent.zip", "."
& $Zip $arguments

cd ..
cd ..

Remove-Item -Path out -Recurse -Force

gh-release --assets Datack.Server.zip,Datack.Agent.zip

#Remove-Item Datack.Server.zip
#Remove-Item Datack.Agent.zip