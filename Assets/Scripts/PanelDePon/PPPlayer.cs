using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPPlayer : PlayerBase
{
	/// <summary> タッチしたブロック </summary>
	private PPPanelBlock m_TouchedBlock = null;

	/// <summary> ゲームレベル </summary>
	public int GameLevel { get; private set; }

	/// <summary> キャラクター </summary>
	//private Character m_Character = null;

	/// <summary>
	/// 生成
	/// </summary>
	protected void Awake()
	{
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
	/// 更新
	/// </summary>
	public override void Play()
	{
		base.Play();

		PPPlayArea playArea = (Game as PPGame).PlayArea;

		if (Input.GetKeyDown(KeyCode.D))
		{
			AddDamage(1);
		}

		// タッチした瞬間
		if (InputManager.IsTouchDown())
		{
			// タッチブロック保持
			m_TouchedBlock = playArea.GetBlock(Camera.main.ScreenToWorldPoint(InputManager.GetTouchPosition()));
		}

		// タッチ中
		if (InputManager.IsTouch())
		{
			// せり上げ
			if (InputManager.IsTouchDouble())
			{
				Elevate();
			}
			// 入れ替え
			else if (m_TouchedBlock != null)
			{
				// 移動先グリッド
				Vector2i target = playArea.ConvertWorldToGrid(Camera.main.ScreenToWorldPoint(InputManager.GetTouchPosition()));

				// 移動元グリッド
				Vector2i current = playArea.GetBlockGrid(m_TouchedBlock);

				if (target != current)
				{
					Vector2i dif = target - current;
					PlayAreaBlock.Dir dir;
					if (dif.x < 0)
					{
						dir = PlayAreaBlock.Dir.Left;
					}
					else if (dif.x > 0)
					{
						dir = PlayAreaBlock.Dir.Right;
					}
					else if (dif.y > 0)
					{
						dir = PlayAreaBlock.Dir.Down;
					}
					else
					{
						dir = PlayAreaBlock.Dir.Up;
					}

					// 交換
					SwapPanel(m_TouchedBlock, dir);
				}
			}
		}
	}

	/// <summary>
	/// パネル入れ替え
	/// </summary>
	private void SwapPanel(PPPanelBlock block, PlayAreaBlock.Dir dir)
	{
		PlayerController.CommandData command = new PlayerController.CommandData();
		command.type =  (byte)PlayerController.CommandType.PP_Swap;
		command.values = new int[2];
		Vector2i grid = (Game as PPGame).PlayArea.GetBlockGrid(block);
		command.values[0] = grid.y * PPGame.Config.PlayAreaWidth + grid.x;
		command.values[1] = (int)dir;
		SetNextCommand(command);
	}

	/// <summary>
	/// せり上げ
	/// </summary>
	private void Elevate()
	{
		PlayerController.CommandData command = new PlayerController.CommandData();
		command.type =  (byte)PlayerController.CommandType.PP_Add;
		SetNextCommand(command);
	}

	/// <summary>
	/// コマンド実行
	/// </summary>
	public override void ExecuteCommand(PlayerController.CommandData command)
	{
		PPPlayArea playArea = (Game as PPGame).PlayArea;

		if (!Game.Controller.isLocalPlayer)
		{
			Debug.Log(command.type);
		}

		switch ((PlayerController.CommandType)command.type)
		{
			case PlayerController.CommandType.PP_Swap:
				if (command.values != null)
				{
					Vector2i grid = new Vector2i(command.values[0] % PPGame.Config.PlayAreaWidth, command.values[0] / PPGame.Config.PlayAreaWidth);
					PPPanelBlock result = playArea.SwapPanel(playArea.GetBlock(grid), (PlayAreaBlock.Dir)command.values[1]);
					if (result != null)
					{
						m_TouchedBlock = result;
					}
				}
				break;

			case PlayerController.CommandType.PP_Add:
				playArea.ElevateValue = PPGame.Config.ManualElevateSpeed;
				playArea.MaxElevateWaitTime = PPGame.Config.ManualElevateInterval;
				playArea.SkipElevateWait();
				break;
		}
	}

	/// <summary>
	/// ダメージ
	/// </summary>
	public override void AddDamage(int level)
	{
		base.AddDamage(level);
		Vector2i size = PPGame.Config.GetDisturbanceSize(level);
		(Game as PPGame).PlayArea.CreateDisturbPanel(size.x, size.y);
	}

	/// <summary>
	/// 連鎖終了イベント
	/// </summary>
	public void OnChainEnd(int chainCount)
	{
		//if (chainCount >= 2)
		//{
		//	StartCoroutine(ChainAppeal(chainCount));
		//}
		int level = PPGame.Config.GetChainDamageLevel(chainCount);
		if (level > 0)
		{
			if (!Game.Controller.isLocalPlayer)
			{
				PlayerController[] players = NetworkGameManager.Instance.GetPlayers();
				foreach (PlayerController player in players)
				{
					if (player != Game.Controller)
					{
						player.Game.Player.AddDamage(PPGame.Config.GetChainDamageLevel(chainCount));
					}
				}
			}
		}
	}

	/// <summary>
	/// 連鎖アピール
	/// </summary>
	public IEnumerator ChainAppeal(int chainCount)
	{
		//m_Character.Visible = true;
		//m_Character.PlayAnim("Chain_0");

		//m_Character.ShowChain(true, chainCount + 1);

		//yield return null;

		//while (!m_Character.IsAnimStoped())
		//{
		//	yield return null;
		//}

		//m_Character.Visible = false;
		//m_Character.ShowChain(false, 0);

		yield return null;
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
		int chainCount = (Game as PPGame).PlayArea.ChainCount;
		if (chainCount > 0)
		{
			chainCount++;
		}
		GUI.Label(new Rect(10, 100, 300, 100), chainCount.ToString() + " Chain", style);
	}
}
