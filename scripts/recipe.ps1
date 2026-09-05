param(
    [ValidateSet('generate','test','build','verify')][string]$Command = 'verify'
)
$ErrorActionPreference = 'Stop'
$RecipeProject = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$RecipeUnity = if ($env:UNITY_EDITOR) { $env:UNITY_EDITOR } else { 'C:\Program Files\Unity\Hub\Editor\6000.3.18f1\Editor\Unity.exe' }
if (-not (Test-Path -LiteralPath $RecipeUnity)) { throw 'Unity 6000.3.18f1 is required.' }
$RecipeRun = Join-Path $RecipeProject ('Artifacts\Recipe\' + (Get-Date -Format 'yyyyMMdd-HHmmss-fffffff'))
New-Item -ItemType Directory -Path $RecipeRun | Out-Null

function Invoke-RecipeUnity([string]$Name, [string[]]$Extra) {
    $arguments = @('-batchmode','-nographics','-projectPath',$RecipeProject,'-logFile',(Join-Path $RecipeRun ($Name+'.log'))) + $Extra
    $quoted = $arguments | ForEach-Object { '"' + $_.Replace('"','\"') + '"' }
    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $RecipeUnity
    $startInfo.Arguments = $quoted -join ' '
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $process = [System.Diagnostics.Process]::Start($startInfo)
    $process.WaitForExit()
    $exitCode = $process.ExitCode
    $process.Dispose()
    if ($exitCode -ne 0) { throw "$Name failed ($exitCode). See $RecipeRun" }
}

if ($Command -eq 'generate' -or -not (Test-Path (Join-Path $RecipeProject 'Assets\_Project\Scenes\RecipeLab.unity'))) {
    Invoke-RecipeUnity 'generate' @('-quit','-executeMethod','IncrementalGame.Editor.RecipeLabSceneBuilder.Generate')
}
if ($Command -in @('test','verify')) {
    Invoke-RecipeUnity 'compile' @('-quit')
    foreach ($platform in @('EditMode','PlayMode')) {
        $resultPath = Join-Path $RecipeRun ($platform+'.xml')
        Invoke-RecipeUnity $platform @('-runTests','-testPlatform',$platform,'-testResults',$resultPath)
        [xml]$document = Get-Content -LiteralPath $resultPath
        if ($document.'test-run'.result -ne 'Passed' -or [int]$document.'test-run'.total -eq 0) { throw "$platform did not pass." }
    }
}
if ($Command -in @('build','verify')) {
    $env:RECIPE_BUILD_PATH = Join-Path $RecipeRun 'Windows\OneBoardRecipeLab.exe'
    Invoke-RecipeUnity 'build' @('-quit','-buildTarget','StandaloneWindows64','-executeMethod','IncrementalGame.Editor.RecipeLabSceneBuilder.BuildWindows')
    $zip = Join-Path $RecipeRun 'OneBoardRecipeLab-v0.3.1-recipe-Windows-x64.zip'
    Compress-Archive -Path (Join-Path $RecipeRun 'Windows\*') -DestinationPath $zip
    $hash = (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash.ToLowerInvariant()
    "$hash  $([IO.Path]::GetFileName($zip))" | Set-Content -LiteralPath ($zip+'.sha256') -Encoding ascii
    $commit = git -C $RecipeProject rev-parse HEAD
    @("Version: 0.3.1-recipe", "Commit: $commit", 'Unity: 6000.3.18f1', "Command: $Command", "SHA256: $hash") | Set-Content -LiteralPath (Join-Path $RecipeRun 'summary.txt') -Encoding utf8
}
Write-Output "Artifacts: $RecipeRun"
