using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TRPlayer : PlayerBase
{
	/// <summary> 前回のタッチ座標 </summary>
	private Vector3 m_PrevTouchPosition = Vector3.zero;
	/// <summary> タッチ移動距離 </summary>
	private float m_TouchMoveDistance = 0;

	/// <summary> テトロミノ移動可能? </summary>
	private bool m_PossibleMoveTetromino = false;
	/// <summary> テトロミノ移動先 </summary>
	private Vector3 m_TetrominoTarget = Vector3.zero;

	/// <summary>
	/// 初期化
	/// </summary>
	public override void Initialize(GameBase game)
	{
		base.Initialize(game);
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
			m_TetrominoTarget = (Game as TRGame).PlayArea.GetTetrominoPosition();
		}
		else
		{
			// 離した瞬間
			if (InputManager.IsTouchUp())
			{
				// 移動距離が短ければ回転
				if (m_TouchMoveDistance < 0.2f)
				{
					RotateTetromino(false);
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
					MoveTetromino((Game as TRGame).PlayArea.ConvertWorldToGridPosition(m_TetrominoTarget));
				}
			}
		}
	}

	/// <summary>
	/// テトロミノ移動
	/// </summary>
	private void MoveTetromino(Vector2i targetGrid)
	{
		PlayerController.CommandData command = new PlayerController.CommandData();
		command.type =  (byte)PlayerController.CommandType.TR_Move;
		command.values = new int[2];
		command.values[0] = targetGrid.x;
		command.values[1] = targetGrid.y;
		SetNextCommand(command);
	}

	/// <summary>
	/// テトロミノ回転
	/// </summary>
	private void RotateTetromino(bool negative)
	{
		PlayerController.CommandData command = new PlayerController.CommandData();
		command.type =  (byte)PlayerController.CommandType.TR_Rotate;
		command.values = new int[1];
		command.values[0] = (byte)(negative ? 1 : 0);
		SetNextCommand(command);
	}

	/// <summary>
	/// コマンド実行
	/// </summary>
	public override void ExecuteCommand(PlayerController.CommandData command)
	{
		switch ((PlayerController.CommandType)command.type)
		{
			case PlayerController.CommandType.TR_Move:
				if (command.values != null)
				{
					(Game as TRGame).PlayArea.MoveTetromino(new Vector2i(command.values[0], command.values[1]));
				}
				break;

			case PlayerController.CommandType.TR_Rotate:
				if (command.values != null)
				{
					(Game as TRGame).PlayArea.RotateTetromino(command.values[0] == 1);
				}
				break;
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
