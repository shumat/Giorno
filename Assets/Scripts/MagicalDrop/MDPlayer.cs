﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MDPlayer : PlayerBase
{
	/// <summary> タッチ開始地点 </summary>
	private Vector3 m_TouchStartPos = Vector3.zero;
	/// <summary> タッチ時間 </summary>
	private float m_TouchTime = 0f;

	/// <summary> 現在位置 </summary>
	private int m_CurrentRow = 0;

	/// <summary> 行動無効 </summary>
	private bool m_IgnoreAction = false;

	/// <summary> ライン生成待機時間 </summary>
	private float m_LineCreateWaitTime = 0f;

	/// <summary> ゲームレベル </summary>
	//private int m_GameLevel = 0;

	/// <summary> スプライトレンダラ </summary>
	private SpriteRenderer m_SpriteRenderer = null;

	/// <summary> キャラクター </summary>
	//private Character m_Character = null;

	/// <summary>
	/// 生成
	/// </summary>
	protected void Awake()
	{
		m_SpriteRenderer = GetComponent<SpriteRenderer>();
		//m_Character = FindObjectOfType<Character>();
	}

	/// <summary>
	/// 初期化
	/// </summary>
	public override void Initialize(GameBase game)
	{
		base.Initialize(game);
		//m_Character.Initialize(GameManager.Instance.PlayerCharacterId);
	}

	/// <summary>
	/// 開始
	/// </summary>
	protected void Start()
	{
		m_CurrentRow = MDGame.Config.PlayAreaWidth / 2;
		UpdatePosition();
	}

	/// <summary>
	/// 更新
	/// </summary>
	public override void Play()
	{
		base.Play();

		MDPlayArea playArea = (Game as MDGame).PlayArea;

		// タッチした瞬間
		if (InputManager.IsTouchDown())
		{
			m_TouchStartPos = InputManager.GetWorldTouchPosition();
			m_IgnoreAction = false;
			m_TouchTime = 0;

			// 背景ライン演出
			playArea.SetBackLineFlash(playArea.ConvertPositionToRow(m_TouchStartPos.x));
		}
		// タッチ中
		if (InputManager.IsTouch())
		{
			// 列を更新
			m_CurrentRow = playArea.ConvertPositionToRow(m_TouchStartPos.x);
			UpdatePosition();

			// ライン作成
			m_TouchTime += Time.deltaTime;
			m_LineCreateWaitTime -= Time.deltaTime;
			if (m_TouchTime > MDGame.Config.LineCreateStartTouchTime)
			{
				if (m_LineCreateWaitTime <= 0f)
				{
					m_LineCreateWaitTime = MDGame.Config.LineCreateContinueTouchTime;
					AddLine();
				}
			}
			// アクション
			else if (!m_IgnoreAction)
			{
				Vector2 dif = InputManager.GetWorldTouchPosition() - m_TouchStartPos;
				if (!m_IgnoreAction && Mathf.Abs(dif.y) > MDGame.Config.ActionStartSwipeDistance)
				{
					// 上
					if (dif.y > 0 && playArea.IsValidPush())
					{
						// プッシュ
						PushDrop(m_CurrentRow);
						m_IgnoreAction = true;
					}
					// 下
					else if (dif.y <= 0 && playArea.IsValidPull(m_CurrentRow))
					{
						// プル
						PullDrop(m_CurrentRow);
						m_IgnoreAction = true;
					}
				}
			}
		}

		// 適当に色変更
		m_SpriteRenderer.color = playArea.GetPulledDrop(0) == null ? Color.white : playArea.GetPulledDrop(0).SpriteColor;
	}

	/// <summary>
	/// プッシュ
	/// </summary>
	private void PushDrop(int row)
	{
		PlayerController.CommandData command = new PlayerController.CommandData();
		command.type =  (byte)PlayerController.CommandType.MD_Push;
		command.values = new int[1];
		command.values[0] = row;
		SetNextCommand(command);
	}

	/// <summary>
	/// プル
	/// </summary>
	private void PullDrop(int row)
	{
		PlayerController.CommandData command = new PlayerController.CommandData();
		command.type =  (byte)PlayerController.CommandType.MD_Pull;
		command.values = new int[1];
		command.values[0] = row;
		SetNextCommand(command);
	}

	/// <summary>
	/// ライン追加
	/// </summary>
	private void AddLine()
	{
		PlayerController.CommandData command = new PlayerController.CommandData();
		command.type =  (byte)PlayerController.CommandType.MD_Add;
		SetNextCommand(command);
	}

	/// <summary>
	/// コマンド実行
	/// </summary>
	public override void ExecuteCommand(PlayerController.CommandData command)
	{
		switch ((PlayerController.CommandType)command.type)
		{
			case PlayerController.CommandType.MD_Push:
				if (command.values != null)
				{
					m_CurrentRow = command.values[0];
					UpdatePosition();
					(Game as MDGame).PlayArea.PushDrop(m_CurrentRow);
				}
				break;

			case PlayerController.CommandType.MD_Pull:
				if (command.values != null)
				{
					m_CurrentRow = command.values[0];
					UpdatePosition();
					(Game as MDGame).PlayArea.PullDrop(m_CurrentRow);
				}
				break;

			case PlayerController.CommandType.MD_Add:
				(Game as MDGame).PlayArea.AddNewLine();
				break;
		}
	}

	/// <summary>
	/// ポーズ中
	/// </summary>
	public override void OnPause()
	{
	}

	/// <summary>
	/// 連鎖イベント
	/// </summary>
	public void ChainEvent(int chainCount)
	{
		if (chainCount >= 2)
		{
			StartCoroutine(ChainAppeal(chainCount));
		}
	}

	/// <summary>
	/// 連鎖アピール
	/// </summary>
	public IEnumerator ChainAppeal(int chainCount)
	{
		//m_IsPause = true;
		//m_PlayArea.BeginPause();

		//m_Character.Visible = true;
		//m_Character.PlayAnim("Chain_0");

		//m_Character.ShowChain(true, chainCount);

		//yield return null;

		//while (!m_Character.IsAnimStoped())
		//{
		//	yield return null;
		//}

		//m_Character.Visible = false;
		//m_Character.ShowChain(false, 0);

		//		m_IsPause = false;
		//		m_PlayArea.EndPause();

		yield return null;
	}

	/// <summary>
	/// 列を更新
	/// </summary>
	private void UpdatePosition()
	{
		// 適当に移動
		Vector3 pos = transform.position;
		pos.y = (Game as MDGame).PlayArea.MinBlockPositionY;
		pos.x = (Game as MDGame).PlayArea.GetBlockPosition(0, m_CurrentRow).x;
		transform.position = pos;
	}

	/// <summary>
	/// GUI表示
	/// </summary>
	private void OnGUI()
	{
		GUIStyle style = new GUIStyle();
		GUIStyleState styleState = new GUIStyleState();
		styleState.textColor = Color.white;
		style.fontSize = 50;
		style.normal = styleState;
		GUI.Label(new Rect(10, 100, 300, 100),
			(Game as MDGame).PlayArea.ChainCount.ToString() + " Chain\n" +
			"Chain Receive " + (Game as MDGame).PlayArea.ChainReceiveTime.ToString() + "\n" +
			"IsValidPull " + (Game as MDGame).PlayArea.IsValidPull(m_CurrentRow) + "\n" +
			"PulledDropCount " + (Game as MDGame).PlayArea.GetPulledDropCount() + "\n" +
			"PushingDropCount " + (Game as MDGame).PlayArea.GetPushingDropCount() , style);
	}
}
