param([string]$RevitInstallDir = 'C:\Program Files\Autodesk\Revit 2024')
$ErrorActionPreference = 'Stop'
$sdkLine = @(dotnet --list-sdks)[-1]
if ($sdkLine -notmatch '^([^ ]+) \[(.+)\]') { throw 'Install a .NET SDK first.' }
$compiler = Join-Path $Matches[2] ($Matches[1] + '\Roslyn\bincore\csc.dll')
$refs = 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
if (!(Test-Path -LiteralPath $refs)) { throw 'Install .NET Framework 4.8 Developer Pack.' }
$outDir = Join-Path $PSScriptRoot 'bin\Release\net48'
New-Item -ItemType Directory -Path $outDir -Force | Out-Null
$arguments = @('/nologo', '/target:library', '/platform:x64', '/langversion:latest', '/nostdlib+',
    ('/out:' + (Join-Path $outDir 'CadLayerTools.dll')))
foreach ($name in @('mscorlib.dll', 'System.dll', 'System.Core.dll', 'System.Runtime.Serialization.dll', 'System.Windows.Forms.dll', 'System.Drawing.dll', 'WindowsBase.dll', 'PresentationCore.dll', 'PresentationFramework.dll', 'System.Xaml.dll'))
{ $arguments += '/reference:' + (Join-Path $refs $name) }
foreach ($name in @('RevitAPI.dll', 'RevitAPIUI.dll', 'Newtonsoft.Json.dll'))
{ $arguments += '/reference:' + (Join-Path $RevitInstallDir $name) }
$arguments += @(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'src') -Filter *.cs -Recurse | Sort-Object FullName | ForEach-Object { $_.FullName })
& dotnet $compiler @arguments
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed.' }
Write-Host "Built: $outDir\CadLayerTools.dll"
