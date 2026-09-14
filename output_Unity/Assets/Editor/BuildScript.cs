using UnityEditor;
using UnityEngine;

/// <summary>批处理构建入口。</summary>
public static class BuildScript
{
	[MenuItem("Tools/Build macOS")]
	public static void BuildMac()
	{
		GameResCatalogBuilder.Build();
		BuildPlayerOptions options = new BuildPlayerOptions
		{
			scenes = new[] { "Assets/Game.unity" },
			locationPathName = "Builds/macOS/TripleHarvest.app",
			target = BuildTarget.StandaloneOSX,
			options = BuildOptions.Development
		};
		var report = BuildPipeline.BuildPlayer(options);
		Debug.Log($"[BuildScript] 结果: {report.summary.result}, 错误: {report.summary.totalErrors}");
		if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
		{
			EditorApplication.Exit(1);
		}
	}
}
