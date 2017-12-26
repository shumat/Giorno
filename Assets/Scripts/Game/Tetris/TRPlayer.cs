using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TRPlayer : PlayerBase
{
	/// <summary> タッチ開始座標 </summary>
	private Vector3 m_TouchStartPosition = Vector3.zero;
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
			m_TouchStartPosition = InputManager.GetWorldTouchPosition();
			m_PrevTouchPosition = m_TouchStartPosition;
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

				// ホールド
				if ((currentTouchPos - m_TouchStartPosition).y > 3f && (Game as TRGame).PlayArea.PossibleHold)
				{
					HoldTetromino();
				}
				// テトロミノ移動
				else if (m_PossibleMoveTetromino)
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
		command.values = new sbyte[2];
		command.values[0] = (sbyte)targetGrid.x;
		command.values[1] = (sbyte)targetGrid.y;
		SetNextCommand(command);
	}

	/// <summary>
	/// テトロミノ回転
	/// </summary>
	private void RotateTetromino(bool negative)
	{
		PlayerController.CommandData command = new PlayerController.CommandData();
		command.type =  (byte)PlayerController.CommandType.TR_Rotate;
		command.values = new sbyte[1];
		command.values[0] = (sbyte)(negative ? 1 : 0);
		SetNextCommand(command);
	}

	/// <summary>
	/// ホールド
	/// </summary>
	private void HoldTetromino()
	{
		PlayerController.CommandData command = new PlayerController.CommandData();
		command.type =  (byte)PlayerController.CommandType.TR_Hold;
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

			case PlayerController.CommandType.TR_Hold:
				(Game as TRGame).PlayArea.Hold();
				break;
		}
		
		// ダメージ
		DamageTable.DamageData damage = GetDamageData((PlayerController.DamageType)command.damageType, command.damageValue);
		if (damage != null)
		{
			(Game as TRGame).PlayArea.RequestAddLine((int)damage.TRLine);
		}
	}

	/// <summary>
	/// ライン消しイベント
	/// </summary>
	public void OnLineVanish(int count)
	{
		if (count > 0)
		{
			SendDamage(PlayerController.DamageType.TR_Vanish, (byte)count);
		}
	}

	/// <summary>
	/// テトロミノ生成イベント
	/// </summary>
	public void OnCreateTetromino()
	{
		m_PossibleMoveTetromino = false;
	}
}
