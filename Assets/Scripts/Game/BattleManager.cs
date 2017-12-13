using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
	private bool m_IsGameEnd = false;

	/// <summary>
	/// 開始
	/// </summary>
	protected void Start()
	{
		// ゲーム作成
		PlayerController[] players = NetworkGameManager.Instance.GetPlayers();
		foreach (PlayerController player in players)
		{
			player.CreateGame();
		}

		// ゲーム開始
		StartCoroutine(GameStart());
	}

	/// <summary>
	/// 更新
	/// </summary>
	protected void Update()
	{
		// ゲーム終了
		if (!m_IsGameEnd)
		{
			m_IsGameEnd = true;
			PlayerController[] players = NetworkGameManager.Instance.GetPlayers();
			foreach (var player in players)
			{
				if (!player.IsGameOver)
				{
					m_IsGameEnd = false;
					break;
				}
			}
			if (m_IsGameEnd)
			{
				StartCoroutine(GameEnd());
			}
		}
	}

	/// <summary>
	/// ゲーム開始
	/// </summary>
	private IEnumerator GameStart()
	{
		m_IsGameEnd = false;

		yield return null;

		PlayerController[] players = NetworkGameManager.Instance.GetPlayers();

		// 準備開始
		foreach (PlayerController player in players)
		{
			player.Game.BeginReady();
		}
		yield return GameCoroutineStopWait();

		// 同期待機
		NetworkGameManager.Instance.StandbySync();
		while (!NetworkGameManager.Instance.IsCompleateSync())
		{
			yield return null;
		}

		// ゲーム開始
		foreach (PlayerController player in players)
		{
			player.Game.BeginPlay();
		}
	}

	/// <summary>
	/// ゲーム終了
	/// </summary>
	private IEnumerator GameEnd()
	{
		m_IsGameEnd = true;
		
		yield return GameCoroutineStopWait();

		GameManager.Instance.RequestUnloadScene("Battle");
		GameManager.Instance.RequestAddScene("DebugModeSelect", true);
		GameManager.Instance.ApplySceneRequests();
	}

	/// <summary>
	/// ゲームのコルーチン停止待機
	/// </summary>
	private IEnumerator GameCoroutineStopWait()
	{
		PlayerController[] players = NetworkGameManager.Instance.GetPlayers();
		while (true)
		{
			bool stopped = true;
			foreach (PlayerController player in players)
			{
				if (!player.Game.IsCoroutineStoped)
				{
					stopped = false;
					break;
				}
			}
			if (stopped)
			{
				break;
			}

			yield return null;
		}
	}
}
