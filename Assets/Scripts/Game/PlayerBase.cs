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

	/// <summary>
	/// ダメージデータ
	/// </summary>
	private struct DamageQueue
	{
		public byte value, type;
	}

	/// <summary> 適用待ちダメージ </summary>
	private List<DamageQueue> m_DamageQueue = new List<DamageQueue>();

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
				m_NextCommand.damageType = m_DamageQueue[0].type;
				m_NextCommand.damageValue = m_DamageQueue[0].value;
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
	protected void SendDamage(PlayerController.DamageType damageType, byte value)
	{
		// ローカルプレイヤー
		if (Game.Controller.isLocalPlayer)
		{
			// ボットにダメージ
			PlayerController[] bots = NetworkGameManager.Instance.GetBots(true);
			foreach (PlayerController bot in bots)
			{
				bot.Game.Player.AddDamage(damageType, value);
			}
		}
		// 複製プレイヤー
		else
		{
			// ローカルプレイヤーにダメージ
			PlayerController localPlayer = NetworkGameManager.Instance.LocalPlayer;
			localPlayer.Game.Player.AddDamage(damageType, value);
		}
	}

	/// <summary>
	/// ダメージイベント
	/// </summary>
	private void AddDamage(PlayerController.DamageType damageType, byte value)
	{
		if (damageType != PlayerController.DamageType.None)
		{
			DamageQueue damage = new DamageQueue();
			damage.type = (byte)damageType;
			damage.value = value;
			m_DamageQueue.Add(damage);
		}
	}

	/// <summary>
	/// ダメージデータ取得
	/// </summary>
	protected DamageTable.DamageData GetDamageData(PlayerController.DamageType damageType, byte damageValue)
	{
		switch (damageType)
		{
			case PlayerController.DamageType.MD_Chain:
				return MDGame.Config.GetDamageData(damageValue);

			case PlayerController.DamageType.PP_Chain:
				return PPGame.Config.GetChainDamageData(damageValue);

			case PlayerController.DamageType.PP_Vanish:
				return PPGame.Config.GetVanishDamageData(damageValue);

			case PlayerController.DamageType.TR_Vanish:
				return TRGame.Config.GetDamageData(damageValue);
		}
		return null;
	}
}
