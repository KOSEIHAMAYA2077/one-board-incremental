using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace IncrementalGame.Editor
{
    public static class Prototype0Build
    {
        [MenuItem("Incremental Game/Build/Windows x64")]
        public static void BuildWindows64()
        {
            Build(
                BuildTarget.StandaloneWindows64,
                GetOutputPath("INCREMENTAL_WINDOWS_BUILD_DIR", "Builds/Windows/Prototype0/OneBoardPrototype0.exe"));
        }

        [MenuItem("Incremental Game/Build/macOS")]
        public static void BuildMacOS()
        {
            Build(
                BuildTarget.StandaloneOSX,
                GetOutputPath("INCREMENTAL_MAC_BUILD_DIR", "Builds/macOS/OneBoardPrototype0.app"));
        }

        private static void Build(BuildTarget target, string outputPath)
        {
            if (!File.Exists(Prototype0SceneBuilder.ScenePath))
            {
                Prototype0SceneBuilder.GenerateScene();
            }

            WriteBuildMetadata();
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? "Builds");

            var options = new BuildPlayerOptions
            {
                scenes = new[] { Prototype0SceneBuilder.ScenePath },
                locationPathName = outputPath,
                target = target,
                options = BuildOptions.CleanBuildCache
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Prototype 0 build failed: {report.summary.result}, " +
                    $"errors={report.summary.totalErrors}");
            }

            UnityEngine.Debug.Log(
                $"[Prototype0] Build succeeded: {outputPath} ({report.summary.totalSize} bytes)");
        }

        public static void WriteBuildMetadata()
        {
            const string resourcesDirectory = "Assets/Resources";
            const string assetPath = resourcesDirectory + "/build-info.txt";
            Directory.CreateDirectory(resourcesDirectory);
            File.WriteAllText(assetPath, ReadGitCommit());
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
        }

        private static string ReadGitCommit()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "rev-parse --short=12 HEAD",
                    WorkingDirectory = Directory.GetParent(Application.dataPath)?.FullName ?? ".",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    return "unavailable";
                }

                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(5000);
                return process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output)
                    ? output
                    : "uncommitted";
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogWarning($"[Prototype0] Git commit unavailable: {exception.Message}");
                return "unavailable";
            }
        }

        private static string GetOutputPath(string variableName, string fallback)
        {
            var configured = Environment.GetEnvironmentVariable(variableName);
            return string.IsNullOrWhiteSpace(configured) ? fallback : configured;
        }
    }
}
