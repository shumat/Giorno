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

	/// <summary>
	/// 生成
	/// </summary>
	protected void Awake()
	{
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
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			SceneManager.LoadScene("Game", LoadSceneMode.Single);
		}
	}

	/// <summary>
	/// メインループ
	/// </summary>
	private IEnumerator MainLoop()
	{
		yield return SceneManager.LoadSceneAsync("DebugModeSelect", LoadSceneMode.Additive);

		DebugModeSelect modeSelect = FindObjectOfType<DebugModeSelect>();
		while (modeSelect.Mode == GameBase.GameMode.None)
		{
			PlayerCharacterId = modeSelect.CharacterId;
			yield return null;
		}
		yield return SceneManager.UnloadSceneAsync("DebugModeSelect");

		yield return SceneManager.LoadSceneAsync(modeSelect.Mode.ToString(), LoadSceneMode.Additive);

		while (!SceneManager.SetActiveScene(SceneManager.GetSceneByName(modeSelect.Mode.ToString())))
		{
			yield return null;
		}
	}

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
