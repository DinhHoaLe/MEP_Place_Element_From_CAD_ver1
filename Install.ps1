param([ValidateSet('2023','2024')][string]$RevitVersion='2023')
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Build.ps1') -RevitVersion $RevitVersion
$destination=Join-Path $env:APPDATA "Autodesk\Revit\Addins\$RevitVersion"
New-Item -ItemType Directory -Force -Path $destination | Out-Null
[xml]$manifest=Get-Content (Join-Path $PSScriptRoot 'CadLayerTools.addin') -Raw
foreach($command in $manifest.RevitAddIns.AddIn){$dll='PlaceFromCad.dll';$command.Assembly=Join-Path $PSScriptRoot "bin\Release\Revit$RevitVersion\net48\$dll"}
$manifest.Save((Join-Path $destination 'CadLayerTools.addin'))
Write-Host "Installed. Restart Revit $RevitVersion. External Tools now contains PLACE ELEMENT FROM CAD."