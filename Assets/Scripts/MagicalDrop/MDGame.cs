using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MDGame : GameBase
{
	/// <summary> コンフィグ </summary>
	private static MDConfig m_Config = null;

	/// <summary>
	/// コンフィグ
	/// </summary>
	public static MDConfig Config
	{
		get
		{
			if (m_Config == null)
			{
				m_Config = Resources.Load<MDConfig>("Configs/MDConfig");
			}
			return m_Config;
		}
	}

	/// <summary> プレイエリア </summary>
	public MDPlayArea PlayArea { get; private set; }

	public GameObject PlayerTemplate = null; // TODO: AssetBundle
	public GameObject PlayAreaTemplate; // TODO: AssetBundle

	/// <summary>
	/// 初期化
	/// </summary>
	public override void Initialize(PlayerController pc)
	{
		base.Initialize(pc);

		Player = Instantiate(PlayerTemplate).GetComponent<MDPlayer>();
		PlayArea = Instantiate(PlayAreaTemplate).GetComponent<MDPlayArea>();
		PlayArea.transform.SetParent(GameObject.Find(Controller.isLocalPlayer ? "PlayArea" : "EnemyPlayArea").transform, false);

		Player.Initialize(this);
		PlayArea.Initialize(this);
	}

	/// <summary>
	/// 更新
	/// </summary>
	public override void Process()
	{
		// コルーチン再生
		PlayArea.StartPlayingCoroutine();

		// エリア更新
		PlayArea.Process();
	}

	/// <summary>
	/// フレーム終了時の更新
	/// </summary>
	public override void LateProcess()
	{
		// コルーチン停止
		PlayArea.StopPlayingCoroutine();
	}
}
