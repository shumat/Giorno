using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
	private bool m_GameStarted = false;

	protected void Start()
	{
	}

	protected void Update()
	{
		if (!m_GameStarted)
		{
			PlayerController[] players = NetworkGameManager.Instance.GetPlayers();
			foreach (PlayerController pc in players)
			{
				pc.BeginGame(GameBase.GameMode.Tetris);
			}
			m_GameStarted = true;
		}
	}
}
