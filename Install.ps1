$ErrorActionPreference = 'Stop'
$dll = Join-Path $PSScriptRoot 'bin\Release\net48\CadLayerTools.dll'
if (!(Test-Path -LiteralPath $dll)) { & (Join-Path $PSScriptRoot 'Build.ps1') }
$destination = Join-Path $env:APPDATA 'Autodesk\Revit\Addins\2024'
New-Item -ItemType Directory -Path $destination -Force | Out-Null
[xml]$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'CadLayerTools.addin') -Raw
$manifest.RevitAddIns.AddIn.Assembly = $dll
$manifest.Save((Join-Path $destination 'CadLayerTools.addin'))
Write-Host 'Installed. Restart Revit 2024, then Add-Ins > External Tools > Export CAD Layers.'
Write-Host 'Keep this project folder in place: the manifest points to its DLL.'
