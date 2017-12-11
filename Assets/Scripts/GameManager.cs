using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	/// <summary> インスタンス </summary>
	private static GameManager m_Instance = null;

	/// <summary> プレイヤーキャラクターID </summary>
	public int PlayerCharacterId { get; set; }

	/// <summary> タイムステップ </summary>
	public static float TimeStep { get { return 1f / Application.targetFrameRate; } }

	/// <summary>
	/// 生成
	/// </summary>
	protected void Awake()
	{
		Application.targetFrameRate = 60;

		if (m_Instance != null)
		{
			Destroy(m_Instance);
		}
		m_Instance = this;
	}

	/// <summary>
	/// 開始
	/// </summary>
	protected void Start()
	{
		StartCoroutine(MainLoop());
	}

	/// <summary>
	/// 更新
	/// </summary>
	protected void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && !GameManager.Instance.IsExistsScene("Matching"))
		{
			NetworkGameManager.Instance.Disconnect();
		}
	}

	/// <summary>
	/// メインループ
	/// </summary>
	private IEnumerator MainLoop()
	{
		yield return AddScene("Matching", false);	
	}

	#region Scene Manage

	/// <summary>
	/// シーン操作リクエスト
	/// </summary>
	private struct SceneControllRequest
	{
		public SceneControllType type;
		public string name;
		public bool setActive;
	}

	/// <summary>
	/// シーン操作タイプ
	/// </summary>
	public enum SceneControllType
	{
		Add,
		Unload,
	}
	
	/// <summary> シーン操作リクエスト </summary>
	private List<SceneControllRequest> m_SceneControllRequests = new List<SceneControllRequest>();

	/// <summary>
	/// シーン追加リクエスト
	/// </summary>
	public void RequestAddScene(string sceneName, bool setActive)
	{
		RequestSceneControll(SceneControllType.Add, sceneName, setActive);
	}

	/// <summary>
	/// シーン破棄リクエスト
	/// </summary>
	public void RequestUnloadScene(string sceneName)
	{
		RequestSceneControll(SceneControllType.Unload, sceneName, false);
	}

	/// <summary>
	/// シーン操作リクエスト
	/// </summary>
	private void RequestSceneControll(SceneControllType type, string sceneName, bool setActive)
	{
		SceneControllRequest request = new SceneControllRequest();
		request.type = type;
		request.name = sceneName;
		request.setActive = setActive;
		m_SceneControllRequests.Add(request);
	}

	/// <summary>
	/// リクエストされたシーン操作適用
	/// </summary>
	public Coroutine ApplySceneRequests()
	{
		return StartCoroutine(ApplySceneRequestsProcess());
	}

	/// <summary>
	/// リクエストされたシーン操作適用
	/// </summary>
	private IEnumerator ApplySceneRequestsProcess()
	{
		foreach (var request in m_SceneControllRequests)
		{
			switch (request.type)
			{
				case SceneControllType.Add:
					yield return AddScene(request.name, request.setActive);
					break;

				case SceneControllType.Unload:
					yield return UnloadScene(request.name);
					break;
			}
		}
		m_SceneControllRequests.Clear();
	}

	/// <summary>
	/// シーン追加
	/// </summary>
	public Coroutine AddScene(string sceneName, bool setActive)
	{
		return StartCoroutine(AddSceneProcess(sceneName, setActive));
	}
	
	/// <summary>
	/// シーン追加
	/// </summary>
	private IEnumerator AddSceneProcess(string sceneName, bool setActive)
	{
		// シーン読み込み
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

		// アクティブシーンに設定
		if (setActive)
		{
			while (!SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName)))
			{
				yield return null;
			}
		}
	}

	/// <summary>
	/// シーン破棄
	/// </summary>
	public Coroutine UnloadScene(string sceneName)
	{
		return StartCoroutine(UnloadSceneProcess(sceneName));
	}

	/// <summary>
	/// シーン破棄
	/// </summary>
	private IEnumerator UnloadSceneProcess(string sceneName)
	{
		yield return SceneManager.UnloadSceneAsync(sceneName);
	}

	/// <summary>
	/// シーンが読み込まれているか
	/// </summary>
	public bool IsExistsScene(string sceneName)
	{
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			if (SceneManager.GetSceneAt(i).name == sceneName)
			{
				return true;
			}
		}
		return false;
	}

	#endregion

	/// <summary>
	/// インスタンス
	/// </summary>
	public static GameManager Instance
	{
		get { return m_Instance; }
	}

//	/// <summary>
//	/// コンフィグ
//	/// </summary>
//	public GameConfig Config
//	{
//		get { return m_GameConfig; }
//	}
}
