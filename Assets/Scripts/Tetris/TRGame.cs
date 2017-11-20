using System.Collections;
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
		TRPanelBlock.Size = Config.GridSize;
	}

	/// <summary>
	/// 初期化
	/// </summary>
	public override void Initialize(PlayerController pc)
	{
		base.Initialize(pc);

		Player = Instantiate(PlayerTemplate).GetComponent<TRPlayer>();
		PlayArea = Instantiate(PlayAreaTemplate).GetComponent<TRPlayArea>();

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
