using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Gera o build WebGL em Builds/WebGL, com as cenas habilitadas em Build Settings.
// Pela linha de comando: Unity -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod WebGLBuild.Build
public static class WebGLBuild
{
    public const string Output = "Builds/WebGL";

    [MenuItem("Survivors/Gerar build WebGL")]
    public static void Build()
    {
        var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = Output,
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        });
        var summary = report.summary;
        Debug.Log($"[WebGLBuild] {summary.result}: {summary.totalErrors} erros, {summary.totalSize / (1024 * 1024)} MB, {summary.totalTime}");
        if (Application.isBatchMode && summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
    }
}
