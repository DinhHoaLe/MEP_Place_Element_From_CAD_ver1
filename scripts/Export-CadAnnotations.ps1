param([Parameter(Mandatory=$true)][string]$DwgPath,[string]$AutoCadConsole='C:\Program Files\Autodesk\AutoCAD 2026\accoreconsole.exe')
$ErrorActionPreference='Stop'
$source=(Resolve-Path -LiteralPath $DwgPath).Path
if([IO.Path]::GetExtension($source) -ne '.dwg'){throw 'Select a DWG file.'}
$projectRoot=Split-Path $PSScriptRoot -Parent
$cache=Join-Path $projectRoot 'work\cad-annotations';New-Item -ItemType Directory -Force -Path $cache | Out-Null
$runDir=Join-Path $cache ([Guid]::NewGuid().ToString('N'));New-Item -ItemType Directory -Path $runDir | Out-Null
$copy=Join-Path $runDir 'source.dwg';Copy-Item -LiteralPath $source -Destination $copy
$output=Join-Path $runDir 'annotations.json'
$lisp=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Export-CadAnnotations.lsp')).Replace('OUTPUT_PATH',$output.Replace('\','/')).Replace('SOURCE_NAME',[IO.Path]::GetFileName($source).Replace('\','\\').Replace('"','\"'))
$lispPath=Join-Path $runDir 'export.lsp';[IO.File]::WriteAllText($lispPath,$lisp,[Text.UTF8Encoding]::new($false))
$script='(setvar "SECURELOAD" 0)'+[Environment]::NewLine+'(load "'+$lispPath.Replace('\','/')+'")'+[Environment]::NewLine+'_QUIT'+[Environment]::NewLine+'_Y'+[Environment]::NewLine
$scriptPath=Join-Path $runDir 'export.scr';[IO.File]::WriteAllText($scriptPath,$script,[Text.UTF8Encoding]::new($false))
& $AutoCadConsole /i $copy /s $scriptPath /l en-US | Out-File (Join-Path $runDir 'console-output.txt')
if(!(Test-Path -LiteralPath $output)){throw 'AutoCAD did not produce annotations. Inspect the local console log.'}
$data=Get-Content $output -Raw | ConvertFrom-Json
$data | Add-Member SourcePath $source
$data | Add-Member SourceHash (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
$json=$data | ConvertTo-Json -Depth 10
$final=Join-Path $cache ([IO.Path]::GetFileName($source)+'.json');[IO.File]::WriteAllText($final,$json,[Text.UTF8Encoding]::new($false))
Write-Host "Saved local CAD annotations: $final ($($data.Blocks.Count) blocks). The source DWG was not edited."