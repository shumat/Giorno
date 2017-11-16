using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TRGame : GameBase
{
	/// <summary> コンフィグ </summary>
	[SerializeField]
	private TRConfig m_Config = null;

	/// <summary> インスタンス </summary>
	private static TRGame m_Instance = null;

	/// <summary> プレイヤー </summary>
	private TRPlayer m_Player = null;

	public GameObject PlayerTemplate; // TODO: AssetBundle

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

		TRPanelBlock.Size = m_Config.GridSize;
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
		m_Player = Instantiate(PlayerTemplate).GetComponent<TRPlayer>();
		m_Player.Initialize();

		yield return null;

		StartCoroutine(GameLoop());
	}
	/// <summary>
	/// ゲームループ
	/// </summary>
	private IEnumerator GameLoop()
	{
		m_Player.Process();

		yield return null;

		StartCoroutine(GameLoop());
	}

	/// <summary>
	/// プレイヤー
	/// </summary>
	public static TRPlayer Player
	{
		get { return m_Instance.m_Player; }
	}

	/// <summary>
	/// コンフィグ
	/// </summary>
	public static TRConfig Config
	{
		get { return m_Instance.m_Config; }
	}
}
