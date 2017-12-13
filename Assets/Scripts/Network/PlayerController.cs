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
		public sbyte[] values;
		public byte damageLevel;

		public void Clear()
		{
			type = 0;
			values = null;
			damageLevel = 0;
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

		PP_Swap,
		PP_Add,

		TR_Move,
		TR_Rotate,
		TR_Hold,
	}

	/// <summary>
	/// イベント
	/// </summary>
	public struct EventData
	{
		public EventType type;
		public int value;
		public uint frame;
	}

	/// <summary>
	/// イベントタイプ
	/// </summary>
	public enum EventType
	{
		None,
		Damage,
	}

	/// <summary> コマンドリスト </summary>
	private List<CommandData> m_Commands = new List<CommandData>();

	/// <summary> 破棄すべきコマンド数 </summary>
	private int m_NeedsRemoveComandCount = 0;

	/// <summary> ゲーム </summary>
	public GameBase Game { get; private set; }

	/// <summary> シード同期乱数 </summary>
	public System.Random SyncRand { get; private set; }

	/// <summary> ゲームモード </summary>
	[SyncVar]
	private GameBase.GameMode m_GameMode = GameBase.GameMode.None;

	/// <summary> ゲーム終了した？ </summary>
	public bool IsGameOver { get; private set; }

	/// <summary>
	/// ゲームモード
	/// </summary>
	public GameBase.GameMode GameMode
	{
		get { return m_GameMode; }
	}

	/// <summary> ボット  </summary>
	[SyncVar]
	private bool m_IsBot = false;

	/// <summary>
	/// ボット
	/// </summary>
	public bool IsBot
	{
		get { return m_IsBot; }
		set { m_IsBot = value; }
	}

	/// <summary>
	/// 自身が操作可能なボット
	/// </summary>
	public bool IsControlledBot
	{
		get { return NetworkServer.active && IsBot; }
	}

	/// <summary>
	/// 複製プレイヤー
	/// </summary>
	public bool IsClone
	{
		get { return !isLocalPlayer && !IsControlledBot; }
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
		if (Game != null && Game.IsPlaying)
		{
			SyncUpdate();

			// ゲームオーバーチェック
			if (!IsClone && Game.IsOver())
			{
				// ローカルの更新停止
				Game.IsPlaying = false;
				// ゲームオーバー送信
				CmdGameOver();
			}
		}
	}

	/// <summary>
	/// 後の更新
	/// </summary>
	protected void LateUpdate()
	{
		// コマンドを破棄
		RemoveCommand();
	}

	/// <summary>
	/// 同期更新
	/// </summary>
	private void SyncUpdate()
	{
		// ローカルプレイヤーかボット
		if (!IsClone)
		{
			// ステップ実行
			if (Input.GetKey(KeyCode.LeftControl) && !Input.GetKeyDown(KeyCode.Space))
			{
				return;
			}

			// ゲーム更新
			Game.Step();

			// プレイヤー操作
			CommandData command = Game.Player.Step();

			// コマンド送信
			SendCommand(command);

			// 即実行
			ExecuteCommand(0);

			// ゲーム更新
			Game.LateStep();
		}
		// 複製プレイヤー
		else
		{
			// フレーム更新
			int index = 0;
			while (IsValidCommand(index, false))
			{
				// ゲーム更新
				Game.Step();

				// コマンド実行
				ExecuteCommand(index);

				// ゲーム更新
				Game.LateStep();

				++index;
			}
		}
	}

	/// <summary>
	/// コマンド有効?
	/// </summary>
	public bool IsValidCommand(int index, bool checkSync)
	{
		// ローカルプレイヤー
		if (isLocalPlayer || !checkSync)
		{
			return index < m_Commands.Count;
		}
		// 複製プレイヤー
		else
		{
			// ローカルプレイヤー以外のコマンド数をチェック
			PlayerController[] players = NetworkGameManager.Instance.GetPlayers();
			foreach (PlayerController player in players)
			{
				if (!player.isLocalPlayer && index >= player.m_Commands.Count)
				{
					return false;
				}
			}
			return true;
		}
	}

	/// <summary>
	/// コマンド実行
	/// </summary>
	private void ExecuteCommand(int index)
	{
		if (index >= 0 && index < m_Commands.Count)
		{
			Game.Player.ExecuteCommand(m_Commands[index]);
			++m_NeedsRemoveComandCount;
		}
	}

	/// <summary>
	/// コマンドを破棄
	/// </summary>
	public void RemoveCommand()
	{
		for (int i = 0; i < m_NeedsRemoveComandCount; i++)
		{
			m_Commands.RemoveAt(0);
		}
		m_NeedsRemoveComandCount = 0;
	}

	/// <summary>
	/// コマンド送信
	/// </summary>
	[Client]
	private void SendCommand(CommandData command)
	{
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
		if (IsClone)
		{
			m_Commands.Add(command);
		}
	}

	/// <summary>
	/// マッチ成功イベント
	/// </summary>
	public void OnMatchSucceed()
	{
		if (NetworkClient.active && !IsClone)
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
	/// ゲームオーバー送信
	/// </summary>
	[Command(channel=Channels.DefaultReliable)]
	private void CmdGameOver()
	{
		PlayerController[] players = NetworkGameManager.Instance.GetPlayers();
		List<PlayerController> playingPlayer = new List<PlayerController>();
		foreach (PlayerController player in players)
		{
			if (!player.IsGameOver)
			{
				playingPlayer.Add(player);
			}
		}
		IsGameOver = true;
		RpcGameOver((byte)playingPlayer.Count);

		// 2位なら1位も送信
		if (playingPlayer.Count == 2)
		{
			playingPlayer.Find(x => x != this).RpcGameOver(1);
		}
	}

	/// <summary>
	/// ゲームオーバー受信
	/// </summary>
	[ClientRpc(channel=Channels.DefaultReliable)]
	private void RpcGameOver(byte rank)
	{
		IsGameOver = true;
		Game.BeginOver(rank);
	}

	/// <summary>
	/// ゲーム開始
	/// </summary>
	public void CreateGame()
	{
		IsGameOver = false;

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
				obj = Resources.Load<GameObject>("PanelDePon/PPGame");
				Game = Instantiate(obj).GetComponent<PPGame>();
				Game.Initialize(this);
				break;

			case GameBase.GameMode.Tetris:
				obj = Resources.Load<GameObject>("Tetris/TRGame");
				Game = Instantiate(obj).GetComponent<TRGame>();
				Game.Initialize(this);
				break;
		}
	}
}
