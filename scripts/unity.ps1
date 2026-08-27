param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("generate", "compile", "test-edit", "test-play", "build-mac", "build-windows", "package-windows", "verify")]
    [string]$Command
)

$ErrorActionPreference = "Stop"
$RequiredUnityVersion = "6000.3.18f1"
$GameVersion = "0.1.0-prototype0"
$ProjectDirectory = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$ArtifactDirectory = Join-Path $ProjectDirectory "Artifacts"
$LogDirectory = Join-Path $ArtifactDirectory "Logs"
$ResultDirectory = Join-Path $ArtifactDirectory "TestResults"

if ($env:UNITY_EDITOR) {
    $UnityExecutable = $env:UNITY_EDITOR
} else {
    $UnityExecutable = "C:\Program Files\Unity\Hub\Editor\$RequiredUnityVersion\Editor\Unity.exe"
}

if (-not (Test-Path -LiteralPath $UnityExecutable -PathType Leaf)) {
    throw "Unity $RequiredUnityVersion was not found at $UnityExecutable. Install the exact version or set UNITY_EDITOR."
}

New-Item -ItemType Directory -Force -Path $LogDirectory, $ResultDirectory | Out-Null

function Invoke-Unity {
    param(
        [Parameter(Mandatory = $true)][string]$LogName,
        [Parameter(ValueFromRemainingArguments = $true)][string[]]$UnityArguments
    )

    $BaseArguments = @(
        "-batchmode",
        "-nographics",
        "-projectPath", $ProjectDirectory,
        "-logFile", (Join-Path $LogDirectory "$LogName.log")
    )

    & $UnityExecutable @BaseArguments @UnityArguments
    if ($LASTEXITCODE -ne 0) {
        throw "Unity command failed with exit code $LASTEXITCODE. See $LogDirectory\$LogName.log"
    }
}

function New-PrototypeScene {
    Invoke-Unity "generate-scene" "-quit" "-executeMethod" "IncrementalGame.Editor.Prototype0SceneBuilder.GenerateScene"
}

function Confirm-PrototypeScene {
    $ScenePath = Join-Path $ProjectDirectory "Assets\_Project\Scenes\Prototype0.unity"
    if (Test-Path -LiteralPath $ScenePath -PathType Leaf) {
        Invoke-Unity "compile" "-quit"
    } else {
        New-PrototypeScene
    }
}

function Test-EditMode {
    Invoke-Unity "editmode" "-runTests" "-testPlatform" "EditMode" "-testResults" (Join-Path $ResultDirectory "editmode.xml")
}

function Test-PlayMode {
    Invoke-Unity "playmode" "-runTests" "-testPlatform" "PlayMode" "-testResults" (Join-Path $ResultDirectory "playmode.xml")
}

function Build-Windows {
    $env:INCREMENTAL_WINDOWS_BUILD_DIR = Join-Path $ArtifactDirectory "Builds\Windows\OneBoardPrototype0.exe"
    Invoke-Unity "build-windows" "-quit" "-buildTarget" "StandaloneWindows64" "-executeMethod" "IncrementalGame.Editor.Prototype0Build.BuildWindows64"
}

function Build-MacOS {
    $env:INCREMENTAL_MAC_BUILD_DIR = Join-Path $ArtifactDirectory "Builds\macOS\OneBoardPrototype0.app"
    Invoke-Unity "build-macos" "-quit" "-executeMethod" "IncrementalGame.Editor.Prototype0Build.BuildMacOS"
}

function New-WindowsPackage {
    $BuildDirectory = Join-Path $ArtifactDirectory "Builds\Windows"
    $ExecutablePath = Join-Path $BuildDirectory "OneBoardPrototype0.exe"
    if (-not (Test-Path -LiteralPath $ExecutablePath -PathType Leaf)) {
        throw "Windows build is missing. Run build-windows first."
    }

    $PackageDirectory = Join-Path $ArtifactDirectory "Packages"
    New-Item -ItemType Directory -Force -Path $PackageDirectory | Out-Null
    $ArchivePath = Join-Path $PackageDirectory "OneBoardPrototype0-v$GameVersion-Windows-x64.zip"
    Compress-Archive -Path (Join-Path $BuildDirectory "*") -DestinationPath $ArchivePath -Force
    $Hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $ArchivePath).Hash.ToLowerInvariant()
    "$Hash  $([System.IO.Path]::GetFileName($ArchivePath))" | Set-Content -Encoding ascii -NoNewline "$ArchivePath.sha256"

    New-TestSummary -PackageDirectory $PackageDirectory -ArchivePath $ArchivePath -ArchiveHash $Hash -ExecutablePath $ExecutablePath
}

function Get-TestRunSummary {
    param([Parameter(Mandatory = $true)][string]$ResultPath)

    if (-not (Test-Path -LiteralPath $ResultPath -PathType Leaf)) {
        return "not run"
    }

    [xml]$ResultDocument = Get-Content -LiteralPath $ResultPath
    $Run = $ResultDocument.'test-run'
    return "$($Run.result), $($Run.passed)/$($Run.total) passed, $($Run.failed) failed"
}

function New-TestSummary {
    param(
        [Parameter(Mandatory = $true)][string]$PackageDirectory,
        [Parameter(Mandatory = $true)][string]$ArchivePath,
        [Parameter(Mandatory = $true)][string]$ArchiveHash,
        [Parameter(Mandatory = $true)][string]$ExecutablePath
    )

    $CommitHash = (& git -C $ProjectDirectory rev-parse --short=12 HEAD 2>$null)
    if ($LASTEXITCODE -ne 0) {
        $CommitHash = "uncommitted"
    }

    $EditSummary = Get-TestRunSummary (Join-Path $ResultDirectory "editmode.xml")
    $PlaySummary = Get-TestRunSummary (Join-Path $ResultDirectory "playmode.xml")
    $BuildSize = (Get-Item -LiteralPath $ExecutablePath).Length
    $ArchiveName = [System.IO.Path]::GetFileName($ArchivePath)
    $Summary = @(
        "# Prototype 0 Test Summary",
        "",
        "- Generated UTC：$([DateTime]::UtcNow.ToString('yyyy-MM-ddTHH:mm:ssZ'))",
        "- Game Version：$GameVersion",
        "- Commit：$CommitHash",
        "- Unity：$RequiredUnityVersion",
        "- EditMode：$EditSummary",
        "- PlayMode：$PlaySummary",
        "- Windows x64 Build：succeeded, $BuildSize bytes",
        "- Archive：$ArchiveName",
        "- SHA-256：$ArchiveHash"
    )
    $Summary | Set-Content -Encoding utf8 (Join-Path $PackageDirectory "prototype0-test-summary.md")
}

switch ($Command) {
    "generate" { New-PrototypeScene }
    "compile" { Invoke-Unity "compile" "-quit" }
    "test-edit" { Test-EditMode }
    "test-play" { Test-PlayMode }
    "build-mac" { Build-MacOS }
    "build-windows" { Build-Windows }
    "package-windows" { New-WindowsPackage }
    "verify" {
        Confirm-PrototypeScene
        Test-EditMode
        Test-PlayMode
        Build-Windows
        New-WindowsPackage
    }
}
