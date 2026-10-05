$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Build.ps1')
$testOutput = Join-Path $PSScriptRoot 'work\source-tests'
New-Item -ItemType Directory -Force $testOutput | Out-Null
$uiRefs = @('mscorlib.dll','System.dll','System.Core.dll','WindowsBase.dll','PresentationCore.dll','PresentationFramework.dll','System.Xaml.dll') | ForEach-Object { '/reference:' + (Join-Path $refs $_) }
$uiSources = @(Get-ChildItem (Join-Path $PSScriptRoot 'src\UI\*.cs') | ForEach-Object FullName)
$uiSources += @('src\Models\CreatedElementRow.cs','src\Services\CreatedElementExcel.cs','src\Models\UiChoice.cs','src\Models\CadSource.cs','src\Models\ModelSource.cs','src\Models\PairRow.cs','src\Models\PlacementMode.cs','src\Models\EquipmentPoint.cs','src\Models\WizardSettings.cs','src\Models\PlacementData.cs','src\Models\Boundary.cs','src\Models\V2.cs','src\Models\LengthPair.cs','src\Services\EquipmentPresence.cs','tests\SourceWindowTests.cs') | ForEach-Object { Join-Path $PSScriptRoot $_ }
$testExe = Join-Path $testOutput 'SourceWindowTests.exe'
& dotnet $compiler /nologo /target:exe /platform:x64 /nostdlib+ ('/out:' + $testExe) @uiRefs @uiSources @imageResources
if ($LASTEXITCODE -ne 0) { throw 'Source UI test compilation failed' }
& $testExe $testOutput
if ($LASTEXITCODE -ne 0) { throw 'Source UI tests failed' }
