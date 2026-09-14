using System.Collections.Generic;
using System.Text;

public class LMGGGOPOHOK
{
	public const string ASSETBUNDLE_REMOTE_NAME = "Remote";

	public const string ASSETBUNDLE_NAME = "Local";

	public const string ASSET_BUNDLE_MANIFEST = "AssetBundleManifest";

	public const string ASSETBUNDLE_DIVIDE_FOLDER_NAME = "V";

	public const string ASSETBUNDLE_SUB_DIVIDE_FOLDER_NAME = "AssetsSubV";

	public const string ASSETBUNDLE_VARIANT = "ab";

	private static StringBuilder sb;

	public static readonly string RemoteAssetListConfigVersionSaveKey;

	public static readonly string BothAssetListConfigKey;

	public static readonly string RemoteAssetListConfigPreDirName;

	public static readonly string RemoteAssetListConfigName;

	public static readonly string BothAssetListConfigName;

	public static readonly string RemoteAssetsListConfigMd5;

	public static readonly string LocalAssetsPackagePath;

	public static readonly string RemoteAssetsPackagePath;

	public static readonly string RemoteAssetsPersistentCachePath;

	public static readonly string RemoteAssetsPersistentSavePath;

	private static string abName;

	private static string abRemoteName;

	private static readonly Dictionary<string, string> key_assetBundleNameDic;

	private static readonly Dictionary<string, string> key_remoteAssetBundleNameDic;

	private static readonly Dictionary<string, string> assetBundleName_keyDic;

	private static readonly Dictionary<string, string> remoteAssetBundleName_keyDic;

	internal static string GetPersistentAssetBundleSavePath(string InKey)
	{
		return null;
	}

	internal static string GetPersistentAssetBundleCachePath(string InKey)
	{
		return null;
	}

	internal static string GetAssetBundleName(string InKey, bool InIsLocal)
	{
		return null;
	}

	internal static string GetKey(string InABName)
	{
		return null;
	}
}
