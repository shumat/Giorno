using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MDGame : GameBase
{
	/// <summary> コンフィグ </summary>
	[SerializeField]
	private MDConfig m_Config = null;

	/// <summary> インスタンス </summary>
	private static MDGame m_Instance = null;

	/// <summary> プレイヤー </summary>
	private MDPlayer m_Player = null;

	public GameObject m_PlayerTemplate = null; // TODO: AssetBundle

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

		MDDropBlock.Size = m_Config.DropSize;
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
		m_Player = Instantiate(m_PlayerTemplate).GetComponent<MDPlayer>();
		m_Player.name = m_PlayerTemplate.name;
		m_Player.Initialize();

		yield return null;

		StartCoroutine(GameLoop());
	}
	/// <summary>
	/// ゲームループ
	/// </summary>
	private IEnumerator GameLoop()
	{
		if (m_Player.IsPause)
		{
			m_Player.OnPause();
		}
		else
		{
			m_Player.Play();
		}

		yield return null;

		StartCoroutine(GameLoop());
	}

	/// <summary>
	/// プレイヤー
	/// </summary>
	public static MDPlayer Player
	{
		get { return m_Instance.m_Player; }
	}

	/// <summary>
	/// コンフィグ
	/// </summary>
	public static MDConfig Config
	{
		get { return m_Instance.m_Config; }
	}
}
