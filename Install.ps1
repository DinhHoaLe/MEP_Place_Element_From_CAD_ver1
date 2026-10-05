param([ValidateSet("2023","2024")][string]$RevitVersion="2023")
$ErrorActionPreference = 'Stop'
$dll = Join-Path $PSScriptRoot "bin\Release\Revit$RevitVersion\net48\CadLayerTools.dll"
if (!(Test-Path -LiteralPath $dll)) { & (Join-Path $PSScriptRoot 'Build.ps1') -RevitVersion $RevitVersion }
$destination = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$RevitVersion"
New-Item -ItemType Directory -Path $destination -Force | Out-Null
[xml]$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'CadLayerTools.addin') -Raw
$manifest.RevitAddIns.AddIn.Assembly = $dll
$manifest.Save((Join-Path $destination 'CadLayerTools.addin'))
Write-Host "Installed. Restart Revit $RevitVersion, then Add-Ins > External Tools > place_element_from_cad."
Write-Host 'Keep this project folder in place: the manifest points to its DLL.'
