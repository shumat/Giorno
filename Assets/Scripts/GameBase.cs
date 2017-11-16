using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBase : MonoBehaviour
{
	/// <summary>
	/// ゲームモード
	/// </summary>
	public enum GameMode
	{
		None,
		MagicalDrop,
		PanelDePon,
		Tetris,
	}
}
