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

	/// <summary> プレイヤーコントローラ </summary>
	public PlayerController Controller { get; protected set; }

	/// <summary> プレイヤー </summary>
	public PlayerBase Player { get; protected set; }
	
	/// <summary>
	/// 初期化
	/// </summary>
	public virtual void Initialize(PlayerController controller)
	{
		Controller = controller;
	}

	/// <summary>
	/// 更新
	/// </summary>
	public virtual void Step(){}

	/// <summary>
	/// フレーム終了時の更新
	/// </summary>
	public virtual void LateStep(){}
}
