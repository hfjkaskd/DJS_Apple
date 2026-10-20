using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>批处理构建入口。</summary>
public static class BuildScript
{
	[Serializable]
	private sealed class AndroidBuildSummary
	{
		public string outputPath;
		public string packageName;
		public string version;
		public int versionCode;
		public string result;
		public int errors;
		public int warnings;
		public ulong bytes;
		public double seconds;
	}

	[MenuItem("Tools/Build Android Preview APK")]
	public static void BuildAndroidPreview()
	{
		if (EditorApplication.isPlayingOrWillChangePlaymode)
			throw new InvalidOperationException("Stop Play mode before building the APK.");
		if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
			throw new InvalidOperationException("Select Android first, or launch Unity with -buildTarget Android.");

		PlayerSettings.bundleVersion = "1.0.9";
		PlayerSettings.Android.bundleVersionCode = 109;

		string output = Environment.GetEnvironmentVariable("DJS_APPLE_APK_OUTPUT");
		if (string.IsNullOrEmpty(output))
			output = "Builds/Android/TripleHarvest-" + PlayerSettings.bundleVersion + "-preview.apk";
		output = Path.GetFullPath(output);
		Directory.CreateDirectory(Path.GetDirectoryName(output));

		PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.flyfox.tripleharvest.preview");
		PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
		PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
		PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
		PlayerSettings.Android.useCustomKeystore = false;
		PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
		PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Low);
		PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
		EditorUserBuildSettings.buildAppBundle = false;
		EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
		GameResCatalogBuilder.Build();

		BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
		{
			scenes = new[] { "Assets/Game.unity" },
			locationPathName = output,
			target = BuildTarget.Android,
			options = BuildOptions.CompressWithLz4
		});
		BuildSummary summary = report.summary;
		File.WriteAllText(Path.ChangeExtension(output, ".build.json"), JsonUtility.ToJson(new AndroidBuildSummary
		{
			outputPath = output,
			packageName = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android),
			version = PlayerSettings.bundleVersion,
			versionCode = PlayerSettings.Android.bundleVersionCode,
			result = summary.result.ToString(),
			errors = summary.totalErrors,
			warnings = summary.totalWarnings,
			bytes = summary.totalSize,
			seconds = summary.totalTime.TotalSeconds
		}, true));
		Debug.Log("[BuildScript] Android preview: " + summary.result + " | " + output);
		if (summary.result != BuildResult.Succeeded)
			throw new InvalidOperationException("Android APK build failed; see the build log and summary.");
	}

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
