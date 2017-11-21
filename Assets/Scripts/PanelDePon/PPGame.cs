﻿using System.Collections;
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

		Player.Initialize(this);
		PlayArea.Initialize(this);

		if (!PC.isLocalPlayer)
		{
			PlayArea.transform.position += Vector3.right * 10f;
		}
	}

	/// <summary>
	/// フレーム開始時の更新
	/// </summary>
	public override void FirstProcess()
	{
		// せり上げスピード登録
		PlayArea.ElevateValue = PPGame.Config.AutoElevateValue;
		PlayArea.MaxElevateWaitTime = PPGame.Config.GetAutoElevateInterval((Player as PPPlayer).GameLevel);

		// コルーチン再生
		PlayArea.StartPlayingCoroutine();
	}

	/// <summary>
	/// 更新
	/// </summary>
	public override void Process()
	{
		// エリア更新
		PlayArea.Process();

		// コルーチン停止
		PlayArea.StopPlayingCoroutine();
	}
}
