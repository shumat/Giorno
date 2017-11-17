using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugModeSelect : MonoBehaviour
{
	[SerializeField]
	public Character m_Character = null;

	private void Start()
	{
		//m_Character.Visible = true;
	}

	private void StartGame(GameBase.GameMode gameMode)
	{
		GameManager.Instance.RequestUnloadScene("DebugModeSelect");
		GameManager.Instance.RequestAddScene(gameMode.ToString(), true);
		GameManager.Instance.ApplySceneRequests();
	}

	public void StartPanelDePON()
	{
		StartGame(GameBase.GameMode.PanelDePon);
	}

	public void StartMagicalDrop()
	{
		StartGame(GameBase.GameMode.MagicalDrop);
	}

	public void StartTetris()
	{
		StartGame(GameBase.GameMode.Tetris);
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
