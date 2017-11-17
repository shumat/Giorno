using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

public class AssetBundleLoader
{
	/// <summary> 読み込み待機中のパス </summary>
	private static List<string> m_PendingLoadPaths = new List<string>();

	/// <summary> アセットバンドル生成リクエスト </summary>
	private static Dictionary<string, AssetBundleCreateRequest> m_Requests = new Dictionary<string, AssetBundleCreateRequest>();

	/// <summary>
	/// アセットバンドル取得
	/// </summary>
	public static AssetBundle Get(string path)
	{
		AssetBundleCreateRequest request = null;

		m_Requests.TryGetValue(path, out request);
		if (request != null)
		{
			return request.assetBundle;
		}

		return null;
	}

	/// <summary>
	/// リクエスト
	/// </summary>
	public static void Add(string path)
	{
		m_PendingLoadPaths.Add(path);
	}

	/// <summary>
	/// 読み込み開始
	/// </summary>
	public static IEnumerator Load()
	{
		string fullPath = "";
		string key = "";

		foreach (string path in m_PendingLoadPaths)
		{
			string[] files = null;

			// ディレクトリ指定
			bool forDirectory = path.Substring(path.Length - 1, 1) == "/";
			if (forDirectory)
			{
				files = Directory.GetFiles(PlatformAssetPath + path, "*.", SearchOption.AllDirectories);
			}
			// ファイル指定
			else
			{
				files = new string[]{ path };
			}

			// 全ファイル読み込み
			foreach (string file in files)
			{
				key = (forDirectory ? file.Substring(PlatformAssetPath.Length) : path).Replace('\\', '/').Replace("./", "");
				if (!m_Requests.ContainsKey(key))
				{
					fullPath = forDirectory ? file : PlatformAssetPath + path;

					// StreamingAssets内に存在しない
					if (!forDirectory && !File.Exists(fullPath))
					{
						Debug.LogError("\"" + fullPath + "\"" + " is not found.");
						continue;
					}

					AssetBundleCreateRequest newRequest = AssetBundle.LoadFromFileAsync(fullPath);

					yield return newRequest;

					m_Requests.Add(key, newRequest);
				}
			}
		}
	}

	/// <summary>
	/// アンロード
	/// </summary>
	public static void Unload(string path, bool unloadAllLoadedObjects)
	{
		AssetBundleCreateRequest request = null;
		m_Requests.TryGetValue(path, out request);
		if (request != null)
		{
			request.assetBundle.Unload(unloadAllLoadedObjects);
		}
		m_Requests.Remove(path);
	}

	/// <summary>
	/// 全てアンロード
	/// </summary>
	public static void UnloadAll()
	{
		foreach (AssetBundleCreateRequest request in m_Requests.Values)
		{
			request.assetBundle.Unload(false);
		}
		m_Requests.Clear();
	}

	/// <summary>
	/// プラットフォームに応じたパス
	/// </summary>
	/// <value>The platform asset path.</value>
	public static string PlatformAssetPath
	{
		get
		{
#if UNITY_SWITCH && !UNITY_EDITOR
			return Application.streamingAssetsPath + "/";
#else
			return "AssetBundles/StandaloneWindows/";
#endif
		}
	}
}
