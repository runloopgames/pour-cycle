param([string]$UnityData = 'C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Data')
$ErrorActionPreference = 'Stop'
$project = Split-Path $PSScriptRoot -Parent
$scratch = Join-Path $project 'Temp\Verification'
New-Item -ItemType Directory -Force $scratch | Out-Null
$mono = Join-Path $UnityData 'MonoBleedingEdge\bin\mono.exe'
$compiler = Join-Path $UnityData 'MonoBleedingEdge\lib\mono\4.5\csc.exe'
$cache = Join-Path $UnityData 'Resources\PackageManager\ProjectTemplates\libcache\com.unity.template.3d-8.1.3\ScriptAssemblies'
$response = @('/target:library','/nostdlib+',('/out:"'+$scratch+'\StaticCheck.dll"'))
$response += Get-ChildItem (Join-Path $UnityData 'Managed\UnityEngine') -Filter '*.dll' | ForEach-Object {'/r:"'+$_.FullName+'"'}
$response += Get-ChildItem (Join-Path $UnityData 'NetStandard\ref\2.1.0') -Filter '*.dll' | ForEach-Object {'/r:"'+$_.FullName+'"'}
foreach($name in @('UnityEngine.UI.dll','Unity.TextMeshPro.dll')) {$response += '/r:"'+(Join-Path $cache $name)+'"'}
$response += Get-ChildItem (Join-Path $project 'Assets\PourCycle') -Recurse -Filter '*.cs' | ForEach-Object {'"'+$_.FullName+'"'}
$responsePath = Join-Path $scratch 'compile.rsp'
$response | Set-Content $responsePath
& $mono $compiler /noconfig ('@'+$responsePath)
if($LASTEXITCODE -ne 0){throw 'Static compilation failed'}
$runner = Join-Path $scratch 'Runner.cs'
'class Runner {static void Main(){System.Console.WriteLine("POUR_CYCLE_TESTS_PASS: "+PourCycle.Editor.CoreChecks.Run());}}' | Set-Content $runner
$core = @('LevelConfig','OrderManager','HoldBufferController','PourController','PourGesture','SessionController','OrderPatienceController') | ForEach-Object {Join-Path $project ('Assets\PourCycle\Scripts\'+$_+'.cs')}
$exe = Join-Path $scratch 'CoreChecks.exe'
& $mono $compiler ('/out:'+$exe) $runner (Join-Path $project 'Assets\PourCycle\Editor\CoreChecks.cs') $core
if($LASTEXITCODE -ne 0){throw 'Core compilation failed'}
& $mono $exe
if($LASTEXITCODE -ne 0){throw 'Core checks failed'}
Write-Output 'Static compilation and core checks passed. Unity Play Mode and device testing are separate.'

