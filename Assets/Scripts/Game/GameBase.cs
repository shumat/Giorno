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

	/// <summary> 対戦中 </summary>
	public bool IsPlaying { get; set; }

	/// <summary> 順位 </summary>
	private byte m_Rank = 0;

	/// <summary> 再生中のコルーチン </summary>
	private IEnumerator m_PlayingCoroutine = null;

	/// <summary>
	/// 初期化
	/// </summary>
	public virtual void Initialize(PlayerController controller)
	{
		Controller = controller;
		IsPlaying = false;
	}

	/// <summary>
	/// 開始時
	/// </summary>
	protected void Start()
	{
	}

	/// <summary>
	/// ゲーム開始
	/// </summary>
	public void BeginPlay()
	{
		IsPlaying = true;
	}
	
	/// <summary>
	/// ゲーム開始準備
	/// </summary>
	public void BeginReady()
	{
		if (m_PlayingCoroutine == null)
		{
			StartCoroutine(m_PlayingCoroutine = Ready());
		}
	}

	/// <summary>
	/// ゲーム開始準備
	/// </summary>
	protected virtual IEnumerator Ready()
	{
		m_PlayingCoroutine = null;
		yield return null;
	}

	/// <summary>
	/// ゲームオーバー
	/// </summary>
	public void BeginOver(byte rank)
	{
		if (m_PlayingCoroutine == null)
		{
			StartCoroutine(m_PlayingCoroutine = Over(rank));
		}
	}

	/// <summary>
	/// ゲームオーバー
	/// </summary>
	protected virtual IEnumerator Over(byte rank)
	{
		IsPlaying = false;
		m_Rank = rank;

		yield return new WaitForSeconds(3f);
		
		m_PlayingCoroutine = null;
	}

	/// <summary>
	/// コルーチン停止中？
	/// </summary>
	public bool IsCoroutineStoped
	{
		get { return m_PlayingCoroutine == null; }
	}

	/// <summary>
	/// 更新
	/// </summary>
	public virtual void Step(){}

	/// <summary>
	/// フレーム終了時の更新
	/// </summary>
	public virtual void LateStep(){}

	/// <summary>
	/// ゲームオーバー？
	/// </summary>
	public virtual bool IsOver()
	{
		return false;
	}

	private void OnGUI()
	{
		if (Controller.IsGameOver && Controller.isLocalPlayer)
		{
			string text = m_Rank == 1 ? "Win" : "Lose";
			ScaledGUI.Label(text, TextAnchor.MiddleCenter, Vector2.zero, Color.white, 100);
		}
	}
}
