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
		public byte value;
		public uint frame;
	}

	/// <summary> ローカルフレーム </summary>
	private uint m_LocalFrameCount = 0;
	
	/// <summary> グローバルフレーム </summary>
	private uint m_GlobalFrameCount = 0;

	/// <summary> コマンドリスト </summary>
	private List<CommandData> m_Commands = new List<CommandData>();

	/// <summary> 予約コマンド </summary>
	private CommandData m_PendingCommand;

	/// <summary>
	/// 生成
	/// </summary>
	protected void Awake()
	{
		NetworkGameManager.Instance.AddPlayerController(this);
		name = "Player_" + NetworkGameManager.Instance.PlayerCount;
	}

	/// <summary>
	/// 更新
	/// </summary>
	protected void Update()
	{
		SyncUpdate();
	}

	/// <summary>
	/// 同期更新
	/// </summary>
	private void SyncUpdate()
	{
		// 自身の更新
		if (isLocalPlayer)
		{
			CommandData command = new CommandData();

			// コマンド
			if (Input.GetMouseButton(0))
			{
				command.type = 1;
				command.value = 5;
			}

			// 古いデータを消す
			for (int i = 0; i < m_Commands.Count; i++)
			{
				if (m_Commands[i].frame < m_LocalFrameCount)
				{
					m_Commands.RemoveAt(i--);
				}
			}

			// コマンド送信
			SendCommand(command, ++m_LocalFrameCount);

			// 即実行
			ExecuteCommand(m_LocalFrameCount);

			// ゲーム更新
		}
		// 相手側の更新(同期待ちする)
		else
		{
			//// フレーム更新
			//while (NetworkGameManager.Instance.IsReadyUpdate(m_GlobalFrameCount))
			//{
			//	// 古いデータを消す
			//	for (int i = 0; i < m_Commands.Count; i++)
			//	{
			//		if (m_Commands[i].frame < m_GlobalFrameCount)
			//		{
			//			m_Commands.RemoveAt(i--);
			//		}
			//	}

			//	// コマンド実行
			//	ExecuteCommand(++m_GlobalFrameCount);

			//	// ゲーム更新
			//}
		}
	}

	/// <summary>
	/// 更新可能?
	/// </summary>
	public bool IsReadyUpdate(uint frame)
	{
		if (m_Commands.Count == 0)
		{
			return true;
		}

		for (int i = 0; i < m_Commands.Count; i++)
		{
			if (m_Commands[i].frame >= frame)
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
		// 古いデータを消す
		for (int i = 0; i < m_Commands.Count; i++)
		{
			if (m_Commands[i].frame < frame)
			{
				m_Commands.RemoveAt(i--);
			}
		}

		CommandData command = m_Commands.Find(x => x.frame == frame);
		Debug.Log(name + "[" + frame + "] " + command.type + "(" + command.value + ")");
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
}
