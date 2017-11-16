using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugModeSelect : MonoBehaviour
{
	[SerializeField]
	public Character m_Character = null;
	GameBase.GameMode m_GameMode = GameBase.GameMode.None;

	private void Start()
	{
		//m_Character.Visible = true;
	}

	public GameBase.GameMode Mode
	{
		get { return m_GameMode; }
	}

	public void StartPanelDePON()
	{
		m_GameMode = GameBase.GameMode.PanelDePon;
	}

	public void StartMagicalDrop()
	{
		m_GameMode = GameBase.GameMode.MagicalDrop;
	}

	public void StartTetris()
	{
		m_GameMode = GameBase.GameMode.Tetris;
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
