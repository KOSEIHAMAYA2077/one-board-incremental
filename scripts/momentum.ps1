param(
    [ValidateSet('generate','test','build','verify')][string]$Command = 'verify'
)
$ErrorActionPreference = 'Stop'
$MomentumProject = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$MomentumUnity = if ($env:UNITY_EDITOR) { $env:UNITY_EDITOR } else { 'C:\Program Files\Unity\Hub\Editor\6000.3.18f1\Editor\Unity.exe' }
if (-not (Test-Path -LiteralPath $MomentumUnity)) { throw 'Unity 6000.3.18f1 is required.' }
$MomentumRun = Join-Path $MomentumProject ('Artifacts\Momentum\' + (Get-Date -Format 'yyyyMMdd-HHmmss-fffffff'))
New-Item -ItemType Directory -Path $MomentumRun | Out-Null

function Invoke-MomentumUnity([string]$Name, [string[]]$Extra) {
    $arguments = @('-batchmode','-nographics','-projectPath',$MomentumProject,'-logFile',(Join-Path $MomentumRun ($Name+'.log'))) + $Extra
    $quoted = $arguments | ForEach-Object { '"' + $_.Replace('"','\"') + '"' }
    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $MomentumUnity
    $startInfo.Arguments = $quoted -join ' '
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $process = [System.Diagnostics.Process]::Start($startInfo)
    $process.WaitForExit()
    $exitCode = $process.ExitCode
    $process.Dispose()
    if ($exitCode -ne 0) { throw "$Name failed ($exitCode). See $MomentumRun" }
}

if ($Command -eq 'generate' -or -not (Test-Path (Join-Path $MomentumProject 'Assets\_Project\Scenes\MomentumLab.unity'))) {
    Invoke-MomentumUnity 'generate' @('-quit','-executeMethod','IncrementalGame.Editor.MomentumLabSceneBuilder.Generate')
}
if ($Command -in @('test','verify')) {
    Invoke-MomentumUnity 'compile' @('-quit')
    foreach ($platform in @('EditMode','PlayMode')) {
        $resultPath = Join-Path $MomentumRun ($platform+'.xml')
        Invoke-MomentumUnity $platform @('-runTests','-testPlatform',$platform,'-testResults',$resultPath)
        [xml]$document = Get-Content -LiteralPath $resultPath
        if ($document.'test-run'.result -ne 'Passed' -or [int]$document.'test-run'.total -eq 0) { throw "$platform did not pass." }
    }
}
if ($Command -in @('build','verify')) {
    $env:MOMENTUM_BUILD_PATH = Join-Path $MomentumRun 'Windows\OneBoardMomentumLab.exe'
    Invoke-MomentumUnity 'build' @('-quit','-buildTarget','StandaloneWindows64','-executeMethod','IncrementalGame.Editor.MomentumLabSceneBuilder.BuildWindows')
    $zip = Join-Path $MomentumRun 'OneBoardMomentumLab-v0.4.2-portrait-Windows-x64.zip'
    Compress-Archive -Path (Join-Path $MomentumRun 'Windows\*') -DestinationPath $zip
    $hash = (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash.ToLowerInvariant()
    "$hash  $([IO.Path]::GetFileName($zip))" | Set-Content -LiteralPath ($zip+'.sha256') -Encoding ascii
    $commit = git -C $MomentumProject rev-parse HEAD
    @("Version: 0.4.2-portrait", "Commit: $commit", 'Unity: 6000.3.18f1', "Command: $Command", "SHA256: $hash") | Set-Content -LiteralPath (Join-Path $MomentumRun 'summary.txt') -Encoding utf8
}
Write-Output "Artifacts: $MomentumRun"
