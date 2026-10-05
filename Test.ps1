param([ValidateSet('2023','2024')][string]$RevitVersion='2023',[string]$RevitInstallDir)
$ErrorActionPreference = 'Stop'
# Build.ps1 also defines compiler/reference paths in this scope.
. (Join-Path $PSScriptRoot 'Build.ps1') -RevitVersion $RevitVersion -RevitInstallDir $RevitInstallDir
$sourceFiles = @(Get-ChildItem (Join-Path $PSScriptRoot 'src') -Filter *.cs -Recurse | ForEach-Object FullName)
$referenceArgs = @()
foreach ($name in @('mscorlib.dll','System.dll','System.Core.dll','System.Drawing.dll','WindowsBase.dll','PresentationCore.dll','PresentationFramework.dll','System.Xaml.dll'))
{ $referenceArgs += '/reference:' + (Join-Path $refs $name) }
foreach ($name in @('RevitAPI.dll','RevitAPIUI.dll'))
{ $referenceArgs += '/reference:' + (Join-Path $RevitInstallDir $name) }
foreach ($test in @('MatcherTests','UniqueTests','MarkerTests','RectTest','RenderWpf'))
{
    $testDir = Join-Path $PSScriptRoot 'work\tests'
    $testExe = Join-Path $testDir ($test + '.exe')
    $compileArgs = @('/nologo','/target:exe','/platform:x64','/nostdlib+',('/out:' + $testExe)) + $referenceArgs + $sourceFiles + (Join-Path $testDir ($test + '.cs'))
    & dotnet $compiler @compileArgs
    if ($LASTEXITCODE -ne 0) { throw "Compilation failed: $test" }
    & $testExe $testDir
    if ($LASTEXITCODE -ne 0) { throw "Test failed: $test" }
}
