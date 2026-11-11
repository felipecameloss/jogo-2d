using System.IO;
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
        PlayerSettings.productName = GameInfo.Title; // título da aba do navegador
        var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = Output,
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        });
        var summary = report.summary;
        if (summary.result == BuildResult.Succeeded) MakeResponsiveFullscreen();
        Debug.Log($"[WebGLBuild] {summary.result}: {summary.totalErrors} erros, {summary.totalSize / (1024 * 1024)} MB, {summary.totalTime}");
        if (Application.isBatchMode && summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
    }

    static void MakeResponsiveFullscreen()
    {
        string indexPath = Path.Combine(Output, "index.html");
        string cssPath = Path.Combine(Output, "TemplateData/style.css");
        if (File.Exists(indexPath))
        {
            string html = File.ReadAllText(indexPath);
            html = html.Replace("fullscreenContainer.style.width = \"960px\";", "fullscreenContainer.style.width = \"100%\";")
                       .Replace("fullscreenContainer.style.height = \"540px\";", "fullscreenContainer.style.height = \"100%\";");
            File.WriteAllText(indexPath, html);
        }
        if (File.Exists(cssPath))
        {
            string css = File.ReadAllText(cssPath);
            css += "\nhtml, body { width: 100%; height: 100%; overflow: hidden; background: #0d0907; }\n" +
                   "#unity-container, #unity-container.unity-desktop { position: fixed !important; inset: 0 !important; left: 0 !important; top: 0 !important; transform: none !important; width: 100vw !important; height: 100vh !important; }\n" +
                   "#unity-fullscreen-container { width: 100% !important; height: 100% !important; }\n" +
                   "#unity-canvas { width: 100% !important; height: 100% !important; }\n" +
                   "#unity-footer { display: none !important; }\n";
            File.WriteAllText(cssPath, css);
        }
    }
}
