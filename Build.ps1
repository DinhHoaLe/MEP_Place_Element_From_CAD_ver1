param([ValidateSet('2023','2024')][string]$RevitVersion='2023',[string]$RevitInstallDir,[ValidateSet('All','Combined','Place','Duct')][string]$Tool='Place')
$ErrorActionPreference='Stop'
if(!$RevitInstallDir){$RevitInstallDir="C:\Program Files\Autodesk\Revit $RevitVersion"}
$apiYear=2000+[Reflection.AssemblyName]::GetAssemblyName((Join-Path $RevitInstallDir 'RevitAPI.dll')).Version.Major
if($apiYear -ne [int]$RevitVersion){throw "API version $apiYear does not match target $RevitVersion."}
$sdkLine=@(dotnet --list-sdks)[-1]
if($sdkLine -notmatch '^([^ ]+) \[(.+)\]'){throw 'Install a .NET SDK first.'}
$compiler=Join-Path $Matches[2] ($Matches[1]+'\Roslyn\bincore\csc.dll')
$refs='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
if(!(Test-Path -LiteralPath $refs)){throw '.NET Framework 4.8 Developer Pack is required.'}
$outDir=Join-Path $PSScriptRoot "bin\Release\Revit$RevitVersion\net48"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$referenceArgs=@()
foreach($name in @('mscorlib.dll','System.dll','System.Core.dll','System.Runtime.Serialization.dll','System.Windows.Forms.dll','System.Drawing.dll','WindowsBase.dll','PresentationCore.dll','PresentationFramework.dll','System.Xaml.dll')){$referenceArgs+='/reference:'+(Join-Path $refs $name)}
foreach($name in @('RevitAPI.dll','RevitAPIUI.dll','Newtonsoft.Json.dll')){$referenceArgs+='/reference:'+(Join-Path $RevitInstallDir $name)}
$allSources=@(Get-ChildItem (Join-Path $PSScriptRoot 'src') -Recurse -Filter *.cs | Sort-Object FullName)
$sourceRules=Get-Content (Join-Path $PSScriptRoot 'SourceSets.json') -Raw | ConvertFrom-Json
$assemblies=switch($Tool){'All'{@('PlaceFromCad')};'Combined'{@('CadLayerTools')};'Place'{@('PlaceFromCad')};'Duct'{throw 'CREATE DUCT FROM CAD has moved to ../MEP_Create_Duct_From_IFC_ver1. Run Build.ps1 in that project.'}}
foreach($assembly in $assemblies){
$excluded=@($sourceRules.$assembly)
$sources=@($allSources | Where-Object {$relative=$_.FullName.Substring($PSScriptRoot.Length+1).Replace('\','/');$relative -notin $excluded} | ForEach-Object FullName)
$images=@('project-symbol','equipment','routes');if($assembly -eq 'PlaceFromCad'){$images=@('project-symbol','equipment')}elseif($assembly -eq 'CreateDuctFromCad'){$images=@('project-symbol','routes')}
$resources=@($images | ForEach-Object {'/resource:'+(Join-Path $PSScriptRoot ('assets\'+$_+'.png'))+',CadLayerTools.Assets.'+$_+'.png'})
$arguments=@('/nologo','/target:library','/platform:x64','/langversion:latest','/nostdlib+',('/out:'+(Join-Path $outDir ($assembly+'.dll'))))+$referenceArgs+$sources
& dotnet $compiler @arguments @resources
if($LASTEXITCODE -ne 0){throw "Compilation failed: $assembly"}
Write-Host "Built: $outDir\$assembly.dll"
}
# Preserve the local UI-test script's resource inputs.
$imageResources=@(Get-ChildItem (Join-Path $PSScriptRoot 'assets\*.png') | ForEach-Object {'/resource:'+$_.FullName+',CadLayerTools.Assets.'+$_.Name})