$ErrorActionPreference = 'Stop'
$webProject = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$webUnity = 'C:\Program Files\Unity\Hub\Editor\6000.3.18f1\Editor\Unity.exe'
$webRun = Join-Path $webProject ('Artifacts\Web\' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $webRun | Out-Null
$env:MOMENTUM_WEB_PATH = Join-Path $webRun 'site'
$webArgs = @('-batchmode','-nographics','-quit','-projectPath',$webProject,'-buildTarget','WebGL','-executeMethod','IncrementalGame.Editor.MomentumLabSceneBuilder.BuildWeb','-logFile',(Join-Path $webRun 'build.log'))
$webInfo = [Diagnostics.ProcessStartInfo]::new()
$webInfo.FileName = $webUnity
$webInfo.Arguments = ($webArgs | ForEach-Object { '"' + $_ + '"' }) -join ' '
$webInfo.UseShellExecute = $false
$webInfo.CreateNoWindow = $true
$webProcess = [Diagnostics.Process]::Start($webInfo)
$webProcess.WaitForExit()
if ($webProcess.ExitCode -ne 0) { throw "Web build failed. See $webRun\build.log" }
Write-Output "Web build: $env:MOMENTUM_WEB_PATH"
