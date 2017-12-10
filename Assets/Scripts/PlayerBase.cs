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
	public virtual PlayerController.CommandData Process()
	{
		m_NextCommand.Clear();

		if (IsPause)
		{
			OnPause();
		}
		else
		{
			Play();

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
	/// ダメージイベント
	/// </summary>
	public void AddDamage(byte level)
	{
		m_DamageQueue.Add(level);
	}
}
