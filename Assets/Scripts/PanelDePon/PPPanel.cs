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
		FallReady,
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

	/// <summary> アクティブ </summary>
	private bool m_Active = false;

	/// <summary> 連鎖ソース? </summary>
	public bool m_IsChainSource = false;

	/// <summary> 連鎖ソース無効 </summary>
	public bool IgnoreChainSource { get; set; }

	/// <summary> 妨害パネル </summary>
	public bool IsDisturbance { get; private set; }

	/// <summary> 妨害パネル接続 </summary>
	protected PPPanel[] m_DisturbLinks = new PPPanel[System.Enum.GetNames(typeof(PlayAreaBlock.Dir)).Length];

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
		m_PlayingCoroutine = null;

		IsDisturbance = false;
		for (int i = 0; i < m_DisturbLinks.Length; i++)
		{
			m_DisturbLinks[i] = null;
		}

		List<Type> validTypes = new List<Type>();
		for (int i = (int)Type.ColorA; i <= (int)Type.ColorF; i++)
		{
			if (!ignoreTypes.Contains((Type)i))
			{
				validTypes.Add((Type)i);
			}
		}
		m_Type = validTypes[area.Game.Controller.SyncRand.Next(validTypes.Count)];

		m_SpriteRenderer.enabled = true;
		m_SpriteRenderer.sprite = Resources.Load("PanelDePon/Panel_0" + ((int)m_Type - 1).ToString(), typeof(Sprite)) as Sprite;
		m_SpriteRenderer.color = Color.gray;
	}

	/// <summary>
	/// 妨害パネル設定
	/// </summary>
	public void SetDisturbance(bool isDisturbance, PPPanel leftLink, PPPanel rightLink, PPPanel upLink, PPPanel downLink)
	{
		IsDisturbance = isDisturbance;
		m_DisturbLinks[(int)PlayAreaBlock.Dir.Left] = leftLink;
		m_DisturbLinks[(int)PlayAreaBlock.Dir.Right] = rightLink;
		m_DisturbLinks[(int)PlayAreaBlock.Dir.Up] = upLink;
		m_DisturbLinks[(int)PlayAreaBlock.Dir.Down] = downLink;
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
	public void BeginFallReady(PPPanelBlock startBlock)
	{
		if (m_State == State.None)
		{
			m_State = State.FallReady;
			StartCoroutine(m_PlayingCoroutine = FallReady(startBlock));
		}
	}

	/// <summary>
	/// 落下待機
	/// </summary>
	public IEnumerator FallReady(PPPanelBlock startBlock)
	{
		// 連鎖ソース
		if (!IgnoreChainSource)
		{
			m_IsChainSource = true;
		}
		IgnoreChainSource = false;

		float waitTime = PPGame.Config.PanelFallWaitTime;
		while (waitTime >= 0f)
		{
			waitTime -= Time.deltaTime;
			yield return null;
		}

		m_State = State.None;
		m_PlayingCoroutine = null;
		BeginFall(startBlock);
	}

	/// <summary>
	/// 落下開始
	/// </summary>
	public void BeginFall(PPPanelBlock startBlock)
	{
		if (m_State == State.None)
		{
			m_State = State.Fall;

			// 連結妨害パネルも落下
			if (IsDisturbance && m_DisturbLinks[(int)PlayAreaBlock.Dir.Down] == null)
			{
				for (int i = 0; i < m_DisturbLinks.Length; i++)
				{
					PlayAreaBlock.Dir dir = (PlayAreaBlock.Dir)i;
					if ((dir == PlayAreaBlock.Dir.Left || dir == PlayAreaBlock.Dir.Right) && m_DisturbLinks[i] != null)
					{
						m_DisturbLinks[i].BeginFall(startBlock.GetLink(dir) as PPPanelBlock);
					}
				}
			}

			StartCoroutine(m_PlayingCoroutine = Falling(startBlock));

			// 1つ上のパネルも落下
			PPPanelBlock upBlock = startBlock.GetLink(PlayAreaBlock.Dir.Up) as PPPanelBlock;
			if (upBlock != null && !upBlock.IsLoced() && upBlock.AttachedPanel != null && upBlock.AttachedPanel.IsValidFall())
			{
				// 連鎖ソース
				if (!upBlock.AttachedPanel.IgnoreChainSource)
				{
					upBlock.AttachedPanel.m_IsChainSource = true;
				}
				upBlock.AttachedPanel.IgnoreChainSource = false;

				upBlock.AttachedPanel.BeginFall(upBlock);
			}
		}
	}

	/// <summary>
	/// 落下中
	/// </summary>
	public IEnumerator Falling(PPPanelBlock startBlock)
	{
		// 開始地点からデタッチ
		startBlock.Detach();
		
		PPPanelBlock curBlock = startBlock;
		PPPanelBlock fallTarget = null;
		float distance = 0;

		while (true)
		{
			// 今いるブロックを算出(上側)
			while (!curBlock.TestInsideY(transform.position.y, m_Area.BlockSize) && curBlock.GetLink(PlayAreaBlock.Dir.Down) != null)
			{
				PPPanelBlock bottom = curBlock.GetLink(PlayAreaBlock.Dir.Down) as PPPanelBlock;
				if (bottom.AttachedPanel == null)
				{
					curBlock = bottom;
				}
				else
				{
					break;
				}
			}

			// 落下地点の再計算
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

		StopFall(fallTarget);
	}

	/// <summary>
	/// 落下中止
	/// </summary>
	private void StopFall(PPPanelBlock fallTarget)
	{
		if (m_State == State.Fall)
		{
			m_State = State.None;

			// 連結妨害パネルも落下中止
			if (IsDisturbance)
			{
				for (int i = 0; i < m_DisturbLinks.Length; i++)
				{
					if (m_DisturbLinks[i] != null)
					{
						m_DisturbLinks[i].StopFall(fallTarget.GetLink((PlayAreaBlock.Dir)i) as PPPanelBlock);
					}
				}
			}

			// 落下地点にアタッチ
			fallTarget.Attach(this, true);

			// 下の落下待機パネルに連鎖ソースを伝搬
			if (m_IsChainSource)
			{
				for (int i = 0; i < 2; i++)
				{
					PPPanelBlock vertBlock = fallTarget.GetLink(i == 0 ? PlayAreaBlock.Dir.Down : PlayAreaBlock.Dir.Up) as PPPanelBlock;
					if (vertBlock != null && vertBlock.AttachedPanel != null && vertBlock.AttachedPanel.CurrentState == State.FallReady)
					{
						vertBlock.AttachedPanel.m_IsChainSource = true;
					}
				}
			}

			StopCoroutine(m_PlayingCoroutine);
			m_PlayingCoroutine = null;
		}
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
		
		wait = endTime - delay + wait;
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
	}

	/// <summary>
	/// 一時停止終了
	/// </summary>
	public void EndPause()
	{
		if (isActiveAndEnabled)
		{
			if (m_PlayingCoroutine != null)
			{
				StartCoroutine(m_PlayingCoroutine);
			}
		}
	}

	/// <summary>
	/// 落下可能?
	/// </summary>
	public bool IsValidFall()
	{
		if (IsDisturbance && Block != null)
		{
			// 最下段の連結妨害パネルまで掘る
			List<PPPanel> disturbLinks = null;
			GetDisturbLinks(out disturbLinks, PlayAreaBlock.Dir.Down);
			PPPanel mostUnderDisturb = disturbLinks.Count  == 0 ? this : disturbLinks[disturbLinks.Count - 1];
			PPPanelBlock mostUnderDisturbBlock = Block.GetLink(PlayAreaBlock.Dir.Down, disturbLinks.Count) as PPPanelBlock;

			// 水平方向の連結妨害パネルブロックを取得
			List<PlayAreaBlock> links;
			List<PlayAreaBlock> horizontal = new List<PlayAreaBlock>();
			mostUnderDisturbBlock.GetLinks(out links, PlayAreaBlock.Dir.Left, mostUnderDisturb.GetDisturbLinkCount(PlayAreaBlock.Dir.Left), true);
			horizontal.AddRange(links);
			mostUnderDisturbBlock.GetLinks(out links, PlayAreaBlock.Dir.Right, mostUnderDisturb.GetDisturbLinkCount(PlayAreaBlock.Dir.Right), false);
			horizontal.AddRange(links);

			// 最下段の連結妨害パネルの直下が空ブロックか調べる
			foreach (PlayAreaBlock block in horizontal)
			{
				PPPanelBlock underBlock = block.GetLink(PlayAreaBlock.Dir.Down) as PPPanelBlock;
				if (underBlock != null && underBlock.AttachedPanel != null)
				{
					return false;
				}
			}
		}

		return true;
	}

	/// <summary>
	/// 落下地点取得
	/// </summary>
	public PPPanelBlock GetFallTarget(PPPanelBlock startBlock = null)
	{
		// 開始地点が空なら現在のブロックを使用
		if (startBlock == null)
		{
			startBlock = Block;
		}

		if (startBlock == null)
		{
			return null;
		}

		PPPanelBlock fallTarget = null;
		int depth = 0;

		// 妨害パネル
		if (IsDisturbance)
		{
			int minDepth;

			// 現在位置から空ブロックまでの距離を取得
			startBlock.GetMostUnderEmptyBlock(ref depth);
			minDepth = depth;

			// 直下にパネルあり
			if (minDepth == 0)
			{
				return startBlock;
			}

			// 最下段の連結妨害パネルまで掘る
			List<PPPanel> disturbLinks = null;
			GetDisturbLinks(out disturbLinks, PlayAreaBlock.Dir.Down);
			disturbLinks = new List<PPPanel>();
			PPPanel mostUnderDisturb = disturbLinks.Count  == 0 ? this : disturbLinks[disturbLinks.Count - 1];
			PPPanelBlock mostUnderDisturbBlock = startBlock.GetLink(PlayAreaBlock.Dir.Down, disturbLinks.Count) as PPPanelBlock;

			// 最下段の連結妨害パネルから空ブロックまでの距離を取得
			for (int i = 0; i < m_DisturbLinks.Length; i++)
			{
				PlayAreaBlock.Dir dir = (PlayAreaBlock.Dir)i;
				if (dir == PlayAreaBlock.Dir.Left || dir == PlayAreaBlock.Dir.Right)
				{
					mostUnderDisturb.GetDisturbLinks(out disturbLinks, dir);
					for (int j = 0; j < disturbLinks.Count; j++)
					{
						depth  = 0;
						(mostUnderDisturbBlock.GetLink(dir, j + 1) as PPPanelBlock).GetMostUnderEmptyBlock(ref depth);
						minDepth = Mathf.Min(minDepth, depth);
					}
				}
			}

			// 最小の空ブロック距離分掘る
			fallTarget = startBlock.GetLink(PlayAreaBlock.Dir.Down, minDepth) as PPPanelBlock;
		}
		// 通常のパネル
		else
		{
			fallTarget = startBlock.GetMostUnderEmptyBlock(ref depth);
		}

		return fallTarget;
	}

	/// <summary>
	/// 指定方向の連結妨害パネルを取得
	/// </summary>
	public void GetDisturbLinks(out List<PPPanel> links, PlayAreaBlock.Dir dir)
	{
		links = new List<PPPanel>();
		PPPanel sidePanel = m_DisturbLinks[(int)dir]; // 隣のパネル
		while (sidePanel != null)
		{
			links.Add(sidePanel);
			sidePanel = sidePanel.m_DisturbLinks[(int)dir]; // 更に隣のパネル
		}
	}
	
	/// <summary>
	/// 指定方向の連結妨害パネル数を取得
	/// </summary>
	private int GetDisturbLinkCount(PlayAreaBlock.Dir dir)
	{
		List<PPPanel> links;
		GetDisturbLinks(out links, dir);
		return links.Count;
	}

	/// <summary>
	/// コルーチン再生中?
	/// </summary>
	public bool IsCoroutinePlaying
	{
		get { return m_PlayingCoroutine != null; }
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
			if (m_State != State.Fall && m_State != State.Vanish && m_State != State.FallReady)
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
}
