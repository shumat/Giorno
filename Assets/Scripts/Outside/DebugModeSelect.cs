﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugModeSelect : MonoBehaviour
{
	[SerializeField]
	public Character m_Character = null;

	private IEnumerator m_StartGameCoroutine = null;

	private void Start()
	{
		//m_Character.Visible = true;
	}

	private void OnGUI()
	{
		if (m_StartGameCoroutine != null)
		{
			ScaledGUI.Label("Waiting for player", TextAnchor.UpperCenter);
		}
	}

	private IEnumerator StartGame(GameBase.GameMode gameMode)
	{
		// ゲームモード送信
		NetworkGameManager.Instance.LocalPlayer.CmdSetGameMode(gameMode);
		// 乱数シード送信
		NetworkGameManager.Instance.LocalPlayer.CmdRandomSeed((int)(Random.value * int.MaxValue));

		PlayerController[] bots = NetworkGameManager.Instance.GetBots(true);
		foreach (PlayerController bot in bots)
		{
			bot.CmdSetGameMode(gameMode);
			bot.CmdRandomSeed((int)(Random.value * int.MaxValue));
		}

		// 同期待機開始
		NetworkGameManager.Instance.StandbySync();

		// 同期完了まで待機
		while (!NetworkGameManager.Instance.IsCompleateSync())
		{
			yield return null;
		}

		GameManager.Instance.RequestUnloadScene("DebugModeSelect");
		GameManager.Instance.RequestAddScene("Battle", true);
		GameManager.Instance.ApplySceneRequests();
	}

	public void StartPanelDePON()
	{
		if (m_StartGameCoroutine == null)
		{
			StartCoroutine(m_StartGameCoroutine = StartGame(GameBase.GameMode.PanelDePon));
		}
	}

	public void StartMagicalDrop()
	{
		if (m_StartGameCoroutine == null)
		{
			StartCoroutine(m_StartGameCoroutine = StartGame(GameBase.GameMode.MagicalDrop));
		}
	}

	public void StartTetris()
	{
		if (m_StartGameCoroutine == null)
		{
			StartCoroutine(m_StartGameCoroutine = StartGame(GameBase.GameMode.Tetris));
		}
	}

	public void SetCharacter(int id)
	{
		if (m_Character != null)
		{
			m_Character.Initialize(id);
		}
	}

	public int CharacterId
	{
		get { return m_Character.Id; }
	}
}
