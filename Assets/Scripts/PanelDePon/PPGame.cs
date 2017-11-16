using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPGame : GameBase
{
	/// <summary> コンフィグ </summary>
	[SerializeField]
	private PPConfig m_Config = null;

	/// <summary> インスタンス </summary>
	private static PPGame m_Instance = null;

	/// <summary> プレイヤー </summary>
	private PPPlayer m_Player = null;

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

		PPPanelBlock.Size = m_Config.PanelSize;
	}

	/// <summary>
	/// 開始
	/// </summary>
	protected void Start()
	{
		StartCoroutine(InitGame());
	}

	/// <summary>
	/// ゲーム初期化
	/// </summary>
	private IEnumerator InitGame()
	{
		GameObject obj = new GameObject();
		m_Player = obj.AddComponent<PPPlayer>();
		m_Player.name = "Player";
		m_Player.Initialize();

		yield return null;

		StartCoroutine(GameLoop());
	}
	/// <summary>
	/// ゲームループ
	/// </summary>
	private IEnumerator GameLoop()
	{
//		if (m_Player.IsPause)
//		{
//			m_Player.OnPause();
//		}
//		else
//		{
			m_Player.Play();
//		}

		yield return null;

		StartCoroutine(GameLoop());
	}

	/// <summary>
	/// プレイヤー
	/// </summary>
	public static PPPlayer Player
	{
		get { return m_Instance.m_Player; }
	}

	/// <summary>
	/// コンフィグ
	/// </summary>
	public static PPConfig Config
	{
		get { return m_Instance.m_Config; }
	}
}
