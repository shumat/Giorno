using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
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
	/// ゲーム開始
	/// </summary>
	private IEnumerator GameStart()
	{
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
