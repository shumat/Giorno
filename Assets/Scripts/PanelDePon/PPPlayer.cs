using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPPlayer : PlayerBase
{
	/// <summary> プレイエリア </summary>
	private PPPlayArea m_PlayArea = null;

	/// <summary> タッチしたブロック </summary>
	private PPPanelBlock m_TouchedBlock = null;

	/// <summary> タッチ開始地点 </summary>
	private Vector3 m_TouchStartPos = Vector3.zero;
	/// <summary> タッチ時間 </summary>
	private float m_TouchTime = 0f;

	/// <summary> ゲームレベル </summary>
	private int m_GameLevel = 0;

	/// <summary> キャラクター </summary>
	private Character m_Character = null;

	/// <summary>
	/// 生成
	/// </summary>
	protected void Awake()
	{
		m_PlayArea = FindObjectOfType<PPPlayArea>();
		m_Character = FindObjectOfType<Character>();
	}

	/// <summary>
	/// 初期化
	/// </summary>
	public override void Initialize()
	{
		base.Initialize();

		m_PlayArea.Initialize();
		m_PlayArea.BeginUpdate();

		m_Character.Initialize(GameManager.Instance.PlayerCharacterId);
	}

	/// <summary>
	/// 更新
	/// </summary>
	public override void Play()
	{
		base.Play();

		// せり上げスピード登録
		m_PlayArea.ElevateValue = PPGame.Config.AutoElevateValue;
		m_PlayArea.MaxElevateWaitTime = PPGame.Config.GetAutoElevateInterval(m_GameLevel);

		// タッチした瞬間
		if (InputManager.IsTouchDown())
		{
			m_TouchStartPos = Camera.main.ScreenToWorldPoint(InputManager.GetTouchPosition());
			m_TouchTime = 0;

			// タッチブロック保持
			m_TouchedBlock = m_PlayArea.GetHitBlock(m_TouchStartPos);
		}

		// タッチ中
		if (InputManager.IsTouch())
		{
			// せり上げ
			m_TouchTime += Time.deltaTime;
			if (m_TouchTime > PPGame.Config.ElevateStartTouchTime)
			{
				m_PlayArea.ElevateValue = PPGame.Config.ManualElevateSpeed;
				m_PlayArea.MaxElevateWaitTime = PPGame.Config.ManualElevateInterval;
				m_PlayArea.SkipElevateWait();
			}
			// 入れ替え
			else if (m_TouchedBlock != null)
			{
				Vector2 dif = Camera.main.ScreenToWorldPoint(InputManager.GetTouchPosition()) - m_TouchStartPos;
				if (dif.magnitude > PPGame.Config.PanelSwapStartSwipeDistance)
				{
					PlayAreaBlock.Dir dir = PlayAreaBlock.Dir.Left;
					bool doSwap = true;
					dif.Normalize();
					if (dif.x < -PPGame.Config.PanelSwapDirThreshouldScale)
					{
						dir = PlayAreaBlock.Dir.Left;
					}
					else if (dif.x > PPGame.Config.PanelSwapDirThreshouldScale)
					{
						dir = PlayAreaBlock.Dir.Right;
					}
					else if (dif.y > PPGame.Config.PanelSwapDirThreshouldScale)
					{
						dir = PlayAreaBlock.Dir.Up;
					}
					else if (dif.y < -PPGame.Config.PanelSwapDirThreshouldScale)
					{
						dir = PlayAreaBlock.Dir.Down;
					}
					else
					{
						doSwap = false;
					}

					// 交換
					if (doSwap)
					{
						m_PlayArea.SwapPanel(m_TouchedBlock, dir);
						m_TouchedBlock = null;
					}
				}
			}
		}
	}

	/// <summary>
	/// 連鎖イベント
	/// </summary>
	public void ChainEvent(int chainCount)
	{
		if (chainCount >= 1)
		{
			StartCoroutine(ChainAppeal(chainCount));
		}
	}

	/// <summary>
	/// 連鎖アピール
	/// </summary>
	public IEnumerator ChainAppeal(int chainCount)
	{
		m_Character.Visible = true;
		m_Character.PlayAnim("Chain_0");

		m_Character.ShowChain(true, chainCount + 1);

		yield return null;

		while (!m_Character.IsAnimStoped())
		{
			yield return null;
		}

		m_Character.Visible = false;
		m_Character.ShowChain(false, 0);

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
		int chainCount = m_PlayArea.ChainCount;
		if (chainCount > 0)
		{
			chainCount++;
		}
		GUI.Label(new Rect(10, 100, 300, 100), chainCount.ToString() + " Chain", style);
	}
}
