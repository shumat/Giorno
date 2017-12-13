using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPGame : GameBase
{
	/// <summary> コンフィグ </summary>
	private static PPConfig m_Config = null;

	/// <summary>
	/// コンフィグ
	/// </summary>
	public static PPConfig Config
	{
		get
		{
			if (m_Config == null)
			{
				m_Config = Resources.Load<PPConfig>("Configs/PPConfig");
			}
			return m_Config;
		}
	}

	/// <summary> プレイエリア </summary>
	public PPPlayArea PlayArea { get; private set; }
	
	public GameObject PlayerTemplate; // TODO: AssetBundle
	public GameObject PlayAreaTemplate; // TODO: AssetBundle

	/// <summary>
	/// 初期化
	/// </summary>
	public override void Initialize(PlayerController pc)
	{
		base.Initialize(pc);

		Player = Instantiate(PlayerTemplate).GetComponent<PPPlayer>();
		PlayArea = Instantiate(PlayAreaTemplate).GetComponent<PPPlayArea>();
		PlayArea.transform.SetParent(GameObject.Find(Controller.isLocalPlayer ? "PlayArea" : "EnemyPlayArea").transform, false);

		Player.Initialize(this);
		PlayArea.Initialize(this);
	}

	/// <summary>
	/// 更新
	/// </summary>
	public override void Step()
	{		
		// コルーチン再生
		PlayArea.StartPlayingCoroutine();

		// エリア更新
		PlayArea.Step();
	}

	/// <summary>
	/// フレーム終了時の更新
	/// </summary>
	public override void LateStep()
	{
		// コルーチン停止
		PlayArea.StopPlayingCoroutine();
	}
}
