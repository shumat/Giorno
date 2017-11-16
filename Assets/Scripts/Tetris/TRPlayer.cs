using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TRPlayer : PlayerBase
{
	/// <summary> プレイエリア </summary>
	private TRPlayArea m_PlayArea = null;
	
	/// <summary> 前回のタッチ座標 </summary>
	private Vector3 m_PrevTouchPosition = Vector3.zero;
	/// <summary> タッチ移動距離 </summary>
	private float m_TouchMoveDistance = 0;

	/// <summary> テトロミノ移動可能? </summary>
	private bool m_PossibleMoveTetromino = false;
	/// <summary> テトロミノ移動先 </summary>
	private Vector3 m_TetrominoTarget = Vector3.zero;


	/// <summary>
	/// 生成
	/// </summary>
	protected void Awake()
	{
		m_PlayArea = FindObjectOfType<TRPlayArea>();
	}

	/// <summary>
	/// 初期化
	/// </summary>
	public override void Initialize()
	{
		base.Initialize();
		m_PlayArea.Initialize();

		m_PlayArea.BeginProcess();
	}

	/// <summary>
	/// 更新
	/// </summary>
	public override void Play()
	{
		base.Play();

		// タッチ開始
		if (InputManager.IsTouchDown())
		{
			m_PrevTouchPosition = InputManager.GetWorldTouchPosition();
			m_TouchMoveDistance = 0;

			m_PossibleMoveTetromino = true;
			m_TetrominoTarget = m_PlayArea.GetTetrominoPosition();
		}
		else
		{
			// 離した瞬間
			if (InputManager.IsTouchUp())
			{
				// 移動距離が短ければ回転
				if (m_TouchMoveDistance < 0.2f)
				{
					m_PlayArea.RotateTetromino(false);
				}
			}
			// タッチ中
			else if (InputManager.IsTouch())
			{
				// テトロミノ移動先を更新
				Vector3 currentTouchPos = InputManager.GetWorldTouchPosition();
				Vector3 delta = currentTouchPos - m_PrevTouchPosition;
				delta.y *= 1.5f;
				m_TetrominoTarget += delta;

				// タッチ移動距離を更新
				m_TouchMoveDistance += Vector3.Distance(currentTouchPos, m_PrevTouchPosition);

				// 今回のタッチ位置を保持
				m_PrevTouchPosition = currentTouchPos;

				// テトロミノ移動
				if (m_PossibleMoveTetromino)
				{
					Vector2i targetGrid = m_PlayArea.ConvertWorldToGridPosition(m_TetrominoTarget);

					/*
					// 有効な範囲に丸める
					if (targetGrid.x < -1)
					{
						m_TetrominoTarget.x = m_PlayArea.ConvertGridToWorldPosition(new Vector2i(-1, targetGrid.y)).x;
					}
					else if (targetGrid.x > m_PlayArea.Width)
					{
						m_TetrominoTarget.x = m_PlayArea.ConvertGridToWorldPosition(new Vector2i(m_PlayArea.Width, targetGrid.y)).x;
					}
					if (targetGrid.y < m_PlayArea.CurrentLine)
					{
						m_TetrominoTarget.y = m_PlayArea.ConvertGridToWorldPosition(new Vector2i(targetGrid.x, m_PlayArea.CurrentLine)).y;
					}
					else if (targetGrid.y > m_PlayArea.Height)
					{
						m_TetrominoTarget.y = m_PlayArea.ConvertGridToWorldPosition(new Vector2i(targetGrid.x, m_PlayArea.Height)).y;
					}
					 */

					// 移動
					m_PlayArea.MoveTetromino(targetGrid);

				}
			}
		}

		if (Input.GetKeyDown(KeyCode.A))
		{
			m_PlayArea.RotateTetromino(true);
		}
		if (Input.GetKeyDown(KeyCode.D))
		{
			m_PlayArea.RotateTetromino(false);
		}

		if (Input.GetMouseButtonDown(1))
		{
			m_PlayArea.RotateTetromino(true);
		}
	}

	/// <summary>
	/// テトロミノ生成イベント
	/// </summary>
	public void NewTetrominoCreateEvent()
	{
		m_PossibleMoveTetromino = false;
	}

	private void OnGUI()
	{
		Vector3 pos = m_TetrominoTarget;
		pos.y = -pos.y;
		pos = Camera.main.WorldToScreenPoint(pos);
		GUI.Label(new Rect(pos.x, pos.y, 100, 100), "target");
	}
}
