using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPPanel : MonoBehaviour
{
	/// <summary>
	/// パネルタイプ
	/// </summary>
	public enum Type
	{
		None,
		ColorA,
		ColorB,
		ColorC,
		ColorD,
		ColorE,
		ColorF,
		Special,
		Obstacle,
	}

	/// <summary>
	/// ステート
	/// </summary>
	public enum State
	{
		None,
		Move,
		Vanish,
		Fall,
	}

	/// <summary> スプライトレンダラ </summary>
	private SpriteRenderer m_SpriteRenderer = null;

	/// <summary> プレイエリア </summary>
	private PPPlayArea m_Area = null;

	/// <summary> 所属ブロック </summary>
	private PPPanelBlock m_Block = null;

	/// <summary> パネルタイプ </summary>
	private Type m_Type = Type.None;

	/// <summary> ステート </summary>
	private State m_State = State.None;

	/// <summary> プレイ中のコルーチン </summary>
	private IEnumerator m_PlayingCoroutine = null;

	/// <summary> 落下待機コルーチン </summary>
	private IEnumerator m_FallWaitCoroutine = null;

	/// <summary> アクティブ </summary>
	private bool m_Active = false;

	/// <summary> 連鎖ソース? </summary>
	public bool m_IsChainSource = false;

	/// <summary> 連鎖ソース無効 </summary>
	public bool IgnoreChainSource { get; set; }

	/// <summary>
	/// 生成
	/// </summary>
	protected void Awake()
	{
		m_SpriteRenderer = GetComponent<SpriteRenderer>();
		m_DebugText = GetComponentInChildren<TextMesh>();
	}

	private TextMesh m_DebugText = null;
	protected void Update()
	{
		m_DebugText.text = "";
		if (m_Block == null)
		{
			m_DebugText.text = "none\n";
		}
		if (m_State != State.None)
		{
			m_DebugText.text += m_State.ToString() + "\n";
		}
		if (IsChainSource)
		{
			m_DebugText.text += "chain\n";
		}
		if (IgnoreChainSource)
		{
			m_DebugText.text += "Ignorechain\n";
		}
	}

	/// <summary>
	/// 初期化
	/// </summary>
	public void Initialize(PPPlayArea area, List<Type> ignoreTypes)
	{
		gameObject.SetActive(true);

		m_Area = area;
		m_Block = null;
		m_State = State.None;
		m_Active = false;
		m_IsChainSource = false;
		IgnoreChainSource = false;

		m_FallWaitCoroutine = null;

		List<Type> validTypes = new List<Type>();
		for (int i = (int)Type.ColorA; i <= (int)Type.ColorF; i++)
		{
			if (!ignoreTypes.Contains((Type)i))
			{
				validTypes.Add((Type)i);
			}
		}
		m_Type = validTypes[area.Game.PC.SyncRand.Next(validTypes.Count)];

		m_SpriteRenderer.enabled = true;
		m_SpriteRenderer.sprite = Resources.Load("PanelDePon/Panel_0" + ((int)m_Type - 1).ToString(), typeof(Sprite)) as Sprite;
		m_SpriteRenderer.color = Color.gray;
	}

	/// <summary>
	/// アクティベート
	/// </summary>
	public void Activate()
	{
		m_Active = true;
		m_SpriteRenderer.color = Color.white;
	}

	/// <summary>
	/// アクティブ
	/// </summary>
	public bool Active
	{
		get { return m_Active; }
	}

	/// <summary>
	/// 交換開始
	/// </summary>
	public void BeginSwap(PPPanelBlock target)
	{
		if (m_State == State.None)
		{
			m_State = State.Move;
			StartCoroutine(m_PlayingCoroutine = Swaping(target));
		}
	}

	/// <summary>
	/// 交換中
	/// </summary>
	public IEnumerator Swaping(PPPanelBlock target)
	{
		while (true)
		{
			Vector3 dir = target.Position - transform.position;
			Vector3 move = dir.normalized * Time.deltaTime * PPGame.Config.PanelSwapSpeed;
			transform.position += move;

			if (dir.magnitude - move.magnitude < 0)
			{
				transform.position = target.Position;
				break;
			}

			yield return null;
		}

		m_State = State.None;
		m_PlayingCoroutine = null;
	}

	/// <summary>
	/// 落下待機開始
	/// </summary>
	public void BeginFallReady(PPPanelBlock startBlock, PPPanelBlock fallTarget)
	{
		if (m_State == State.None && !IsFallWait)
		{
			StartCoroutine(m_FallWaitCoroutine = FallReady(startBlock, fallTarget));
		}
	}

	/// <summary>
	/// 落下待機
	/// </summary>
	public IEnumerator FallReady(PPPanelBlock startBlock, PPPanelBlock fallTarget)
	{
		// 連鎖ソース
		if (!IgnoreChainSource)
		{
			m_IsChainSource = true;
		}
		IgnoreChainSource = false;

		float waitTime = PPGame.Config.PanelFallWaitTime;

		while (waitTime >= 0f && m_State == State.None)
		{
			waitTime -= Time.deltaTime;
			yield return null;
		}

		if (m_State == State.None)
		{
			BeginFall(startBlock, fallTarget);
		}

		m_FallWaitCoroutine = null;
	}

	/// <summary>
	/// 落下開始
	/// </summary>
	public void BeginFall(PPPanelBlock startBlock, PPPanelBlock fallTarget)
	{
		if (m_State == State.None)
		{
			m_State = State.Fall;
			StartCoroutine(m_PlayingCoroutine = Falling(startBlock, fallTarget));

			// 1つ上のパネルも落下
			PPPanelBlock upBlock = startBlock.GetLink(PlayAreaBlock.Dir.Up) as PPPanelBlock;
			if (upBlock != null && !upBlock.IsLoced() && upBlock.AttachedPanel != null)
			{
				// 連鎖ソース
				if (!upBlock.AttachedPanel.IgnoreChainSource)
				{
					upBlock.AttachedPanel.m_IsChainSource = true;
				}
				upBlock.AttachedPanel.IgnoreChainSource = false;

				upBlock.AttachedPanel.BeginFall(upBlock, fallTarget.GetLink(PlayAreaBlock.Dir.Up) as PPPanelBlock);
			}
		}
	}

	/// <summary>
	/// 落下中
	/// </summary>
	public IEnumerator Falling(PPPanelBlock startBlock, PPPanelBlock fallTarget)
	{
		// 開始地点からデタッチ
		startBlock.Detach();

		// 距離算出
		float distance = Mathf.Abs(fallTarget.Position.y - transform.position.y);
		PPPanelBlock curBlock = startBlock;

		while (true)
		{
			// 落下地点の再計算
			while (!curBlock.TestInsideY(transform.position.y, m_Area.BlockSize) && curBlock.GetLink(PlayAreaBlock.Dir.Down) != null)
			{
				curBlock = curBlock.GetLink(PlayAreaBlock.Dir.Down) as PPPanelBlock;
			}

			fallTarget = curBlock.GetMostUnderEmptyBlock();
			distance = Mathf.Abs(fallTarget.Position.y - transform.position.y);

			// 移動
			Vector3 move = -Vector3.up * Time.deltaTime * PPGame.Config.PanelFallSpeed;
			transform.position += move;
			distance -= move.magnitude;

			// 移動完了
			if (distance < 0)
			{
				transform.position = fallTarget.Position;
				break;
			}

			yield return null;
		}

		// 落下地点にアタッチ
		fallTarget.Attach(this);

		// 下の落下待機パネルに連鎖ソースを伝搬
		if (m_IsChainSource)
		{
			for (int i = 0; i < 2; i++)
			{
				PPPanelBlock vertBlock = fallTarget.GetLink(i == 0 ? PlayAreaBlock.Dir.Down : PlayAreaBlock.Dir.Up) as PPPanelBlock;
				if (vertBlock != null && vertBlock.AttachedPanel != null && vertBlock.AttachedPanel.IsFallWait)
				{
					vertBlock.AttachedPanel.m_IsChainSource = true;
				}
			}
		}

		m_State = State.None;
		m_PlayingCoroutine = null;
	}

	/// <summary>
	/// 消滅開始
	/// </summary>
	public void BeginVanish(float delay, float endTime)
	{
		if (m_State == State.None)
		{
			m_State = State.Vanish;
			StartCoroutine(m_PlayingCoroutine = Vanishing(delay, endTime));
		}
	}

	/// <summary>
	/// 消滅
	/// </summary>
	private IEnumerator Vanishing(float delay, float endTime)
	{
		float time = 0f;
		int count = 0;
		while (time < 0.5f)
		{
			time += Time.deltaTime;
			++count;
			m_SpriteRenderer.color = count % 16 < 8 ? Color.gray : Color.white;
			yield return null;
		}

		m_SpriteRenderer.color = Color.gray;

		float wait = delay;
		while (wait > 0)
		{
			wait -= Time.deltaTime;
			yield return null;
		}

		m_SpriteRenderer.enabled = false;
		
		wait = endTime - delay;
		while (wait > 0)
		{
			wait -= Time.deltaTime;
			yield return null;
		}

		m_Area.RequestRemovePanel(this);
		//m_Block.Detach();

		m_State = State.None;
		m_SpriteRenderer.color = Color.white;

		m_PlayingCoroutine = null;
		gameObject.SetActive(false);
	}

	/// <summary>
	/// 一時停止
	/// </summary>
	public void BeginPause()
	{
		if (m_PlayingCoroutine != null)
		{
			StopCoroutine(m_PlayingCoroutine);
		}
		if (m_FallWaitCoroutine != null)
		{
			StopCoroutine(m_FallWaitCoroutine);
		}
	}

	/// <summary>
	/// 一時停止終了
	/// </summary>
	public void EndPause()
	{
		if (m_PlayingCoroutine != null)
		{
			StartCoroutine(m_PlayingCoroutine);
		}
		if (m_FallWaitCoroutine != null)
		{
			StartCoroutine(m_FallWaitCoroutine);
		}
	}

	/// <summary>
	/// ロック中？
	/// </summary>
	public bool IsLocked()
	{
		return m_State != State.None || !Active;
	}

	/// <summary>
	/// 落下中?
	/// </summary>
	public bool IsFalling()
	{
		return m_State == State.Fall;
	}

	/// <summary>
	/// 連鎖ソースフラグ
	/// </summary>
	public bool IsChainSource
	{
		get { return m_IsChainSource; }
		set
		{
			// 落下中か消滅中は変更不可
			if (m_State != State.Fall && m_State != State.Vanish && !IsFallWait)
			{
				m_IsChainSource = value;
			}
		}
	}

	/// <summary>
	/// パネルタイプ
	/// </summary>
	public Type PanelType
	{
		get { return m_Type; }
	}

	/// <summary>
	/// ステート
	/// </summary>
	public State CurrentState
	{
		get { return m_State; }
	}

	/// <summary>
	/// 所属ブロック
	/// </summary>
	public PPPanelBlock Block
	{
		get { return m_Block; }
		set { m_Block = value; }
	}

	/// <summary>
	/// 落下待機中
	/// </summary>
	public bool IsFallWait
	{
		get { return m_FallWaitCoroutine != null; }
	}
}
