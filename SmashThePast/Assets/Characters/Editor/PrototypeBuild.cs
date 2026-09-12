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
    }
}
