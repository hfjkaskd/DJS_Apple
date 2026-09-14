using UnityEngine;

namespace Orange
{
	[CreateAssetMenu(fileName = "VersionCode", menuName = "ScriptableObject/VersionCode", order = 0)]
	public class VersionCodeTemplate : ScriptableObject
	{
		[Header("版本号")]
		public int InnerVersion;

		[Header("字符版本号")]
		public string BundleVersion;

		[Header("测试包")]
		public bool Debug;

		[Header("是否带Log")]
		public bool HasLog;

		[Header("测试服")]
		public bool TestServer;

		[Header("AB测试测试服")]
		public bool ABTestTestServer;

		[Header("打包时间（打包时自动赋值）")]
		public string BuildTime;

		[Header("编辑器下使用真实的bundle加载")]
		public bool UseBundleLoadInEditor;
	}
}
