using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SmashThePast.Characters.Editor
{
    public static class PrototypeBuild
    {
        private const string OutputPath = "Builds/SmashThePast.app";
        private const string WindowsOutputPath = "Builds/Windows/SmashThePast.exe";

        [MenuItem("Smash the Past/Construir macOS")]
        public static void BuildMacOS()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string outputPath = Path.Combine(projectRoot, OutputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { PrototypeSceneBuilder.ScenePath },
                locationPathName = outputPath,
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"Build macOS fallido: {report.summary.result}");
            Debug.Log($"BUILD_MACOS: {outputPath}, {report.summary.totalSize} bytes.");
        }

        // Diego trabaja en Windows. Sin esto, cada version que le pasamos no le sirve.
        [MenuItem("Smash the Past/Construir Windows")]
        public static void BuildWindows()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string outputPath = Path.Combine(projectRoot, WindowsOutputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { PrototypeSceneBuilder.ScenePath },
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"Build Windows fallido: {report.summary.result}");
            Debug.Log($"BUILD_WINDOWS: {outputPath}, {report.summary.totalSize} bytes.");
        }
    }
}
