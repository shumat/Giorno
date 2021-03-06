﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TRGame : GameBase
{
	/// <summary> コンフィグ </summary>
	private static TRConfig m_Config = null;

	/// <summary>
	/// コンフィグ
	/// </summary>
	public static TRConfig Config
	{
		get
		{
			if (m_Config == null)
			{
				m_Config = Resources.Load<TRConfig>("Configs/TRConfig");
			}
			return m_Config;
		}
	}

	/// <summary> プレイエリア </summary>
	public TRPlayArea PlayArea { get; private set; }
	
	public GameObject PlayerTemplate; // TODO: AssetBundle
	public GameObject PlayAreaTemplate; // TODO: AssetBundle

	/// <summary>
	/// 生成
	/// </summary>
	protected void Awake()
	{
	}

	/// <summary>
	/// 初期化
	/// </summary>
	public override void Initialize(PlayerController pc)
	{
		base.Initialize(pc);

		Player = Instantiate(PlayerTemplate).GetComponent<TRPlayer>();
		PlayArea = Instantiate(PlayAreaTemplate).GetComponent<TRPlayArea>();
		PlayArea.transform.SetParent(GameObject.Find(Controller.isLocalPlayer ? "PlayArea" : "EnemyPlayArea").transform, false);

		Player.Initialize(this);
		PlayArea.Initialize(this);
	}

	/// <summary>
	/// 更新
	/// </summary>
	public override void Step()
	{
		PlayArea.Step();
	}

	/// <summary>
	/// ゲームオーバー？
	/// </summary>
	public override bool IsOver()
	{
		return PlayArea.IsOver;
	}
}
