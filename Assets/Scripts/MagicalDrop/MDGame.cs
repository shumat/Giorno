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

		Player = Instantiate(PlayerTemplate).GetComponent<MDPlayer>();
		PlayArea = Instantiate(PlayAreaTemplate).GetComponent<MDPlayArea>();

		Player.Initialize(this);
		PlayArea.Initialize(this);

		if (!PC.isLocalPlayer)
		{
			PlayArea.transform.position += Vector3.right * 10f;
		}
	}

	/// <summary>
	/// 更新
	/// </summary>
	public override void Process()
	{
		PlayArea.Process();
	}
}
