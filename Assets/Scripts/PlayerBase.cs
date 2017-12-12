using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : MonoBehaviour
{
	/// <summary> ゲーム </summary>
	public GameBase Game { get; protected set; }

	/// <summary> 一時停止中 </summary>
	public bool IsPause { get; protected set; }

	/// <summary> コマンド </summary>
	private PlayerController.CommandData m_NextCommand;

	/// <summary> 適用待ちダメージ </summary>
	private List<byte> m_DamageQueue = new List<byte>();

	/// <summary>
	/// 初期化
	/// </summary>
	public virtual void Initialize(GameBase game)
	{
		Game = game;
	}

	/// <summary>
	/// 更新
	/// </summary>
	public virtual PlayerController.CommandData Step()
	{
		m_NextCommand.Clear();

		if (IsPause)
		{
			OnPause();
		}
		else
		{
			if (!Game.Controller.IsBot)
			{
				Play();
			}
			else
			{
			}

			// ダメージを追加
			if (m_DamageQueue.Count > 0)
			{
				m_NextCommand.damageLevel = m_DamageQueue[0];
				m_DamageQueue.RemoveAt(0);
			}
		}

		return m_NextCommand;
	}

	/// <summary>
	/// 操作更新
	/// </summary>
	public virtual void Play(){}

	/// <summary>
	/// 一時停止中
	/// </summary>
	public virtual void OnPause(){}

	/// <summary>
	/// コマンド登録
	/// </summary>
	public virtual void SetNextCommand(PlayerController.CommandData command)
	{
		m_NextCommand = command;
	}

	/// <summary>
	/// コマンド実行
	/// </summary>
	public virtual void ExecuteCommand(PlayerController.CommandData command){}

	/// <summary>
	/// ダメージ送信
	/// </summary>
	protected void SendDamage(byte level)
	{
		// ローカルプレイヤー
		if (Game.Controller.isLocalPlayer)
		{
			// ボットにダメージ
			PlayerController[] bots = NetworkGameManager.Instance.GetBots(true);
			foreach (PlayerController bot in bots)
			{
				bot.Game.Player.AddDamage(level);
			}
		}
		// 複製プレイヤー
		else
		{
			// ローカルプレイヤーにダメージ
			NetworkGameManager.Instance.LocalPlayer.Game.Player.AddDamage(level);
		}
	}

	/// <summary>
	/// ダメージイベント
	/// </summary>
	private void AddDamage(byte level)
	{
		if (level > 0)
		{
			m_DamageQueue.Add(level);
		}
	}
}
