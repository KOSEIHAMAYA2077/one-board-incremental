param(
    [ValidateSet('generate','test','build','verify')][string]$Command = 'verify'
)
$ErrorActionPreference = 'Stop'
$PlacementProject = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$PlacementUnity = if ($env:UNITY_EDITOR) { $env:UNITY_EDITOR } else { 'C:\Program Files\Unity\Hub\Editor\6000.3.18f1\Editor\Unity.exe' }
if (-not (Test-Path -LiteralPath $PlacementUnity)) { throw 'Unity 6000.3.18f1 is required.' }
$PlacementRun = Join-Path $PlacementProject ('Artifacts\Placement\' + (Get-Date -Format 'yyyyMMdd-HHmmss-fffffff'))
New-Item -ItemType Directory -Path $PlacementRun | Out-Null

function Invoke-PlacementUnity([string]$Name, [string[]]$Extra) {
    $arguments = @('-batchmode','-nographics','-projectPath',$PlacementProject,'-logFile',(Join-Path $PlacementRun ($Name+'.log'))) + $Extra
    $quoted = $arguments | ForEach-Object { '"' + $_.Replace('"','\"') + '"' }
    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $PlacementUnity
    $startInfo.Arguments = $quoted -join ' '
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $process = [System.Diagnostics.Process]::Start($startInfo)
    $process.WaitForExit()
    $exitCode = $process.ExitCode
    $process.Dispose()
    if ($exitCode -ne 0) { throw "$Name failed ($exitCode). See $PlacementRun" }
}

if ($Command -eq 'generate' -or -not (Test-Path (Join-Path $PlacementProject 'Assets\_Project\Scenes\FreePlacement.unity'))) {
    Invoke-PlacementUnity 'generate' @('-quit','-executeMethod','IncrementalGame.Editor.FreePlacementSceneBuilder.Generate')
}
if ($Command -in @('test','verify')) {
    Invoke-PlacementUnity 'compile' @('-quit')
    foreach ($platform in @('EditMode','PlayMode')) {
        $resultPath = Join-Path $PlacementRun ($platform+'.xml')
        Invoke-PlacementUnity $platform @('-runTests','-testPlatform',$platform,'-testResults',$resultPath)
        [xml]$document = Get-Content -LiteralPath $resultPath
        if ($document.'test-run'.result -ne 'Passed' -or [int]$document.'test-run'.total -eq 0) { throw "$platform did not pass." }
    }
}
if ($Command -in @('build','verify')) {
    $env:PLACEMENT_BUILD_PATH = Join-Path $PlacementRun 'Windows\OneBoardRouteLab.exe'
    Invoke-PlacementUnity 'build' @('-quit','-buildTarget','StandaloneWindows64','-executeMethod','IncrementalGame.Editor.FreePlacementSceneBuilder.BuildWindows')
    $zip = Join-Path $PlacementRun 'OneBoardRouteLab-v0.2.0-placement-Windows-x64.zip'
    Compress-Archive -Path (Join-Path $PlacementRun 'Windows\*') -DestinationPath $zip
    $hash = (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash.ToLowerInvariant()
    "$hash  $([IO.Path]::GetFileName($zip))" | Set-Content -LiteralPath ($zip+'.sha256') -Encoding ascii
    $commit = git -C $PlacementProject rev-parse HEAD
    @("Version: 0.2.0-placement", "Commit: $commit", 'Unity: 6000.3.18f1', "Command: $Command", "SHA256: $hash") | Set-Content -LiteralPath (Join-Path $PlacementRun 'summary.txt') -Encoding utf8
}
Write-Output "Artifacts: $PlacementRun"
