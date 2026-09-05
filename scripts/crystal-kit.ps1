param([ValidateSet('generate','build','verify')][string]$Command='verify')
$ErrorActionPreference='Stop'
$KitProject=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$KitUnity='C:\Program Files\Unity\Hub\Editor\6000.3.18f1\Editor\Unity.exe'
$KitRun=Join-Path $KitProject ('Artifacts\CrystalKit\'+(Get-Date -Format 'yyyyMMdd-HHmmss-fffffff'))
New-Item -ItemType Directory -Path $KitRun | Out-Null
function Invoke-KitUnity([string]$Name,[string]$Method) {
    $KitArgs=@('-batchmode','-nographics','-quit','-projectPath',$KitProject,'-executeMethod',$Method,'-logFile',(Join-Path $KitRun ($Name+'.log')))
    $KitInfo=[System.Diagnostics.ProcessStartInfo]::new()
    $KitInfo.FileName=$KitUnity
    $KitInfo.Arguments=($KitArgs | ForEach-Object {'"'+$_.Replace('"','\"')+'"'}) -join ' '
    $KitInfo.UseShellExecute=$false;$KitInfo.CreateNoWindow=$true
    $KitProcess=[System.Diagnostics.Process]::Start($KitInfo);$KitProcess.WaitForExit();$KitExit=$KitProcess.ExitCode;$KitProcess.Dispose()
    if($KitExit -ne 0) { throw "$Name failed ($KitExit). See $KitRun" }
}
if($Command -eq 'generate' -or -not(Test-Path (Join-Path $KitProject 'Assets\Resources\CrystalKitV1\Library.asset'))) {
    Invoke-KitUnity 'generate' 'IncrementalGame.Editor.CrystalAssetBuilder.Generate'
}
if($Command -eq 'verify') { & (Join-Path $PSScriptRoot 'momentum.ps1') verify }
if($Command -ne 'generate') {
    $env:CRYSTAL_BUILD_PATH=Join-Path $KitRun 'Gallery\LucentCrystalGallery.exe'
    Invoke-KitUnity 'gallery-build' 'IncrementalGame.Editor.CrystalAssetBuilder.BuildWindows'
    $env:CRYSTAL_PACKAGE_PATH=Join-Path $KitRun 'LucentCrystalKit-v1.unitypackage'
    Invoke-KitUnity 'asset-package' 'IncrementalGame.Editor.CrystalAssetBuilder.ExportPackage'
    Copy-Item -LiteralPath (Join-Path $KitProject 'docs\LUCENT_CRYSTAL_KIT_V1.md') -Destination (Join-Path $KitRun 'ASSET_GUIDE.md')
    Copy-Item -LiteralPath (Join-Path $KitProject 'ArtExports\CrystalKitV1') -Destination (Join-Path $KitRun 'OBJ') -Recurse
    $KitZip=Join-Path $KitRun 'LucentCrystalKit-v1-Gallery-and-Assets.zip'
    Compress-Archive -Path @((Join-Path $KitRun 'Gallery'),(Join-Path $KitRun 'OBJ'),$env:CRYSTAL_PACKAGE_PATH,(Join-Path $KitRun 'ASSET_GUIDE.md')) -DestinationPath $KitZip
    $KitHash=(Get-FileHash -LiteralPath $KitZip -Algorithm SHA256).Hash.ToLowerInvariant()
    "$KitHash  $([IO.Path]::GetFileName($KitZip))" | Set-Content -LiteralPath ($KitZip+'.sha256') -Encoding ascii
    $KitCommit=git -C $KitProject rev-parse HEAD
    @('Lucent Kit V1 / Gallery 1.0.0-art',"Source: $KitCommit",'Unity: 6000.3.18f1',"SHA256: $KitHash") | Set-Content -LiteralPath (Join-Path $KitRun 'summary.txt') -Encoding utf8
}
Write-Output "Crystal Kit: $KitRun"
