using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
	/// <summary>
	/// コマンド
	/// </summary>
	public struct CommandData
	{
		public byte type;
		public int[] values;
		public uint frame;

		public void Clear()
		{
			type = 0;
			values = null;
			frame = 0;
		}
	}

	/// <summary>
	/// コマンドタイプ
	/// </summary>
	public enum CommandType
	{
		None,
		MD_Push,
		MD_Pull,
		MD_Add,
		TR_Move,
		TR_Rotate,
	}

	/// <summary> フレーム </summary>
	public uint FrameCount { get; private set; }

	/// <summary> コマンドリスト </summary>
	private List<CommandData> m_Commands = new List<CommandData>();

	/// <summary> 予約コマンド </summary>
	private CommandData m_PendingCommand;

	/// <summary> ゲーム </summary>
	public GameBase Game { get; private set; }

	/// <summary> シード同期乱数 </summary>
	public System.Random SyncRand { get; private set; }

	/// <summary> ゲームモード </summary>
	[SyncVar]
	private GameBase.GameMode m_GameMode = GameBase.GameMode.None;

	/// <summary>
	/// ゲームモード
	/// </summary>
	public GameBase.GameMode GameMode
	{
		get { return m_GameMode; }
	}

	/// <summary>
	/// 開始
	/// </summary>
	protected void Start()
	{
		NetworkGameManager.Instance.AddPlayerController(this);
		name = "Player_" + NetworkGameManager.Instance.PlayerCount;
	}

	/// <summary>
	/// 更新
	/// </summary>
	protected void Update()
	{
		if (Game != null)
		{
			SyncUpdate();
		}
	}

	/// <summary>
	/// 同期更新
	/// </summary>
	private void SyncUpdate()
	{
		// 自身の更新
		if (isLocalPlayer)
		{
			// ステップ実行
			if (Input.GetKey(KeyCode.LeftShift) && !Input.GetKeyDown(KeyCode.Space))
			{
				return;
			}

			// ゲーム更新
			Game.FirstProcess();

			// プレイヤー操作
			CommandData command = Game.Player.Process();

			// コマンド送信
			SendCommand(command, FrameCount);

			// 即実行
			ExecuteCommand(FrameCount);

			// ゲーム更新
			Game.Process();

			++FrameCount;
		}
		// 相手側の更新(同期待ちする)
		else
		{
			// フレーム更新
			while (NetworkGameManager.Instance.IsReadyUpdate(FrameCount))
			{
				// ゲーム更新
				Game.FirstProcess();

				// コマンド実行
				ExecuteCommand(FrameCount++);

				// ゲーム更新
				Game.Process();
			}
		}
	}

	/// <summary>
	/// 更新可能?
	/// </summary>
	public bool IsReadyUpdate(uint frame)
	{
		for (int i = 0; i < m_Commands.Count; i++)
		{
			if (m_Commands[i].frame == frame)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// コマンド実行
	/// </summary>
	private void ExecuteCommand(uint frame)
	{
		// フレームのコマンドを実行
		int index = m_Commands.FindIndex(x => x.frame == frame);
		if (index >= 0)
		{
			Game.Player.ExecuteCommand(m_Commands[index]);
		}
	}

	/// <summary>
	/// コマンドを破棄
	/// </summary>
	public void RemoveCommand(uint frame)
	{
		for (int i = 0; i < m_Commands.Count; i++)
		{
			if (m_Commands[i].frame < frame)
			{
				m_Commands.RemoveAt(i--);
			}
			else
			{
				break;
			}
		}
	}

	/// <summary>
	/// コマンド送信
	/// </summary>
	[Client]
	private void SendCommand(CommandData command, uint frame)
	{
		command.frame = frame;

		// 自分の分は自分で追加
		m_Commands.Add(command);

		// 送信
		CmdSendCommand(command);
	}

	/// <summary>
	/// コマンド送信
	/// </summary>
	[Command(channel=Channels.DefaultReliable)]
	private void CmdSendCommand(CommandData command)
	{
		// サーバー

		// 専用サーバー
		if (isServer && !isClient)
		{
			m_Commands.Add(command);
		}

		RpcRecieveCommand(command);
	}

	/// <summary>
	/// コマンド受信
	/// </summary>
	[ClientRpc(channel=Channels.DefaultReliable)]
	private void RpcRecieveCommand(CommandData command)
	{
		// クライアント

		if (!isLocalPlayer)
		{
			m_Commands.Add(command);
		}
	}

	/// <summary>
	/// マッチ成功イベント
	/// </summary>
	public void OnMatchSucceed()
	{
		if (NetworkClient.active && isLocalPlayer)
		{
			CmdRandomSeed((int)(Random.value * int.MaxValue));
		}
	}

	/// <summary>
	/// 乱数シード送信
	/// </summary>
	[Command(channel=Channels.DefaultReliable)]
	private void CmdRandomSeed(int seed)
	{
		RpcRandomSeed(seed);
	}

	/// <summary>
	/// 乱数シード受信
	/// </summary>
	[ClientRpc(channel=Channels.DefaultReliable)]
	private void RpcRandomSeed(int seed)
	{
		SyncRand = new System.Random(seed);
	}

	/// <summary>
	/// 同期待機送信
	/// </summary>
	[Command(channel=Channels.DefaultReliable)]
	public void CmdStandbySync()
	{
		RpcStandbySync();
	}
	
	/// <summary>
	/// 同期待機受信
	/// </summary>
	[ClientRpc(channel=Channels.DefaultReliable)]
	private void RpcStandbySync()
	{
		++NetworkGameManager.Instance.SyncCount;
	}
	
	/// <summary>
	/// ゲームモード送信
	/// </summary>
	[Command(channel=Channels.DefaultReliable)]
	public void CmdSetGameMode(GameBase.GameMode mode)
	{
		m_GameMode = mode;
	}

	/// <summary>
	/// ゲーム開始
	/// </summary>
	public void BeginGame()
	{
		GameObject obj = null;
		Debug.Log(m_GameMode);
		switch (m_GameMode)
		{
			case GameBase.GameMode.MagicalDrop:
				obj = Resources.Load<GameObject>("MagicalDrop/MDGame");
				Game = Instantiate(obj).GetComponent<MDGame>();
				Game.Initialize(this);
				break;

			case GameBase.GameMode.PanelDePon:
				break;

			case GameBase.GameMode.Tetris:
				obj = Resources.Load<GameObject>("Tetris/TRGame");
				Game = Instantiate(obj).GetComponent<TRGame>();
				Game.Initialize(this);
				break;
		}
	}
}
