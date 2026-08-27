#!/usr/bin/env bash
set -euo pipefail

readonly REQUIRED_UNITY_VERSION="6000.3.18f1"
readonly GAME_VERSION="0.1.0-prototype0"
readonly SCRIPT_DIRECTORY="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly PROJECT_DIRECTORY="$(cd "${SCRIPT_DIRECTORY}/.." && pwd)"
readonly ARTIFACT_DIRECTORY="${PROJECT_DIRECTORY}/Artifacts"
readonly LOG_DIRECTORY="${ARTIFACT_DIRECTORY}/Logs"
readonly RESULT_DIRECTORY="${ARTIFACT_DIRECTORY}/TestResults"

if [[ -n "${UNITY_EDITOR:-}" ]]; then
  UNITY_EXECUTABLE="${UNITY_EDITOR}"
else
  UNITY_EXECUTABLE="/Applications/Unity/Hub/Editor/${REQUIRED_UNITY_VERSION}/Unity.app/Contents/MacOS/Unity"
fi

usage() {
  printf '%s\n' "Usage: scripts/unity.sh <generate|compile|test-edit|test-play|build-mac|build-windows|package-windows|verify>"
}

require_unity() {
  if [[ ! -x "${UNITY_EXECUTABLE}" ]]; then
    printf '%s\n' "Unity ${REQUIRED_UNITY_VERSION} was not found at: ${UNITY_EXECUTABLE}" >&2
    printf '%s\n' "Install the exact Editor version or set UNITY_EDITOR to its executable." >&2
    exit 2
  fi
}

prepare_artifacts() {
  mkdir -p "${LOG_DIRECTORY}" "${RESULT_DIRECTORY}"
}

run_unity() {
  local log_name="$1"
  shift
  "${UNITY_EXECUTABLE}" \
    -batchmode \
    -nographics \
    -projectPath "${PROJECT_DIRECTORY}" \
    -logFile "${LOG_DIRECTORY}/${log_name}.log" \
    "$@"
}

generate_scene() {
  run_unity generate-scene \
    -quit \
    -executeMethod IncrementalGame.Editor.Prototype0SceneBuilder.GenerateScene
}

ensure_scene() {
  if [[ -f "${PROJECT_DIRECTORY}/Assets/_Project/Scenes/Prototype0.unity" ]]; then
    compile_project
  else
    generate_scene
  fi
}

compile_project() {
  run_unity compile -quit
}

test_edit_mode() {
  run_unity editmode \
    -runTests \
    -testPlatform EditMode \
    -testResults "${RESULT_DIRECTORY}/editmode.xml"
}

test_play_mode() {
  run_unity playmode \
    -runTests \
    -testPlatform PlayMode \
    -testResults "${RESULT_DIRECTORY}/playmode.xml"
}

build_macos() {
  INCREMENTAL_MAC_BUILD_DIR="${ARTIFACT_DIRECTORY}/Builds/macOS/OneBoardPrototype0.app" \
    run_unity build-macos \
      -quit \
      -executeMethod IncrementalGame.Editor.Prototype0Build.BuildMacOS
}

build_windows() {
  INCREMENTAL_WINDOWS_BUILD_DIR="${ARTIFACT_DIRECTORY}/Builds/Windows/OneBoardPrototype0.exe" \
    run_unity build-windows \
      -quit \
      -buildTarget StandaloneWindows64 \
      -executeMethod IncrementalGame.Editor.Prototype0Build.BuildWindows64
}

package_windows() {
  local build_directory="${ARTIFACT_DIRECTORY}/Builds/Windows"
  local package_directory="${ARTIFACT_DIRECTORY}/Packages"
  local archive_name="OneBoardPrototype0-v${GAME_VERSION}-Windows-x64.zip"
  local archive_path="${package_directory}/${archive_name}"

  if [[ ! -f "${build_directory}/OneBoardPrototype0.exe" ]]; then
    printf '%s\n' "Windows build is missing. Run build-windows first." >&2
    exit 3
  fi

  mkdir -p "${package_directory}"
  # Do not put macOS resource forks or extended attributes into a Windows archive.
  ditto -c -k --norsrc --noextattr "${build_directory}" "${archive_path}"
  (
    cd "${package_directory}"
    shasum -a 256 "${archive_name}" > "${archive_name}.sha256"
  )

  write_test_summary "${package_directory}" "${archive_name}"
}

test_run_summary() {
  local result_path="$1"

  if [[ ! -f "${result_path}" ]]; then
    printf '%s' "not run"
    return
  fi

  local run_line
  local result
  local total
  local passed
  local failed
  run_line="$(grep -m 1 '<test-run ' "${result_path}")"
  result="$(sed -E 's/.* result="([^"]+)".*/\1/' <<< "${run_line}")"
  total="$(sed -E 's/.* total="([^"]+)".*/\1/' <<< "${run_line}")"
  passed="$(sed -E 's/.* passed="([^"]+)".*/\1/' <<< "${run_line}")"
  failed="$(sed -E 's/.* failed="([^"]+)".*/\1/' <<< "${run_line}")"
  printf '%s' "${result}, ${passed}/${total} passed, ${failed} failed"
}

write_test_summary() {
  local package_directory="$1"
  local archive_name="$2"
  local summary_path="${package_directory}/prototype0-test-summary.md"
  local commit_hash
  local archive_hash
  local build_size

  commit_hash="$(git -C "${PROJECT_DIRECTORY}" rev-parse --short=12 HEAD 2>/dev/null || printf '%s' 'uncommitted')"
  archive_hash="$(awk '{print $1}' "${package_directory}/${archive_name}.sha256")"
  build_size="$(stat -f '%z' "${ARTIFACT_DIRECTORY}/Builds/Windows/OneBoardPrototype0.exe")"

  {
    printf '# Prototype 0 Test Summary\n\n'
    printf -- '- Generated UTC：%s\n' "$(date -u '+%Y-%m-%dT%H:%M:%SZ')"
    printf -- '- Game Version：%s\n' "${GAME_VERSION}"
    printf -- '- Commit：%s\n' "${commit_hash}"
    printf -- '- Unity：%s\n' "${REQUIRED_UNITY_VERSION}"
    printf -- '- EditMode：%s\n' "$(test_run_summary "${RESULT_DIRECTORY}/editmode.xml")"
    printf -- '- PlayMode：%s\n' "$(test_run_summary "${RESULT_DIRECTORY}/playmode.xml")"
    printf -- '- Windows x64 Build：succeeded, %s bytes\n' "${build_size}"
    printf -- '- Archive：%s\n' "${archive_name}"
    printf -- '- SHA-256：%s\n' "${archive_hash}"
  } > "${summary_path}"
}

main() {
  if [[ $# -ne 1 ]]; then
    usage
    exit 2
  fi

  require_unity
  prepare_artifacts

  case "$1" in
    generate) generate_scene ;;
    compile) compile_project ;;
    test-edit) test_edit_mode ;;
    test-play) test_play_mode ;;
    build-mac) build_macos ;;
    build-windows) build_windows ;;
    package-windows) package_windows ;;
    verify)
      ensure_scene
      test_edit_mode
      test_play_mode
      build_windows
      package_windows
      ;;
    *)
      usage
      exit 2
      ;;
  esac
}

main "$@"
