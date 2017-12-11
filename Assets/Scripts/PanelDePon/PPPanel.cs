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
		Dissolve,
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
	public void SetDisturbance(PPPanel leftLink, PPPanel rightLink, PPPanel upLink, PPPanel downLink)
	{
		IsDisturbance = true;
		m_DisturbLinks[(int)PlayAreaBlock.Dir.Left] = leftLink;
		m_DisturbLinks[(int)PlayAreaBlock.Dir.Right] = rightLink;
		m_DisturbLinks[(int)PlayAreaBlock.Dir.Up] = upLink;
		m_DisturbLinks[(int)PlayAreaBlock.Dir.Down] = downLink;
		m_SpriteRenderer.color = Color.gray;
	}

	/// <summary>
	/// 妨害パネル解除
	/// </summary>
	private void UnsetDisturbance()
	{
		IsDisturbance = false;
		m_SpriteRenderer.color = Color.white;

		// 連結情報破棄
		for (int i = 0; i < m_DisturbLinks.Length; i++)
		{
			if (m_DisturbLinks[i] != null)
			{
				m_DisturbLinks[i].m_DisturbLinks[(int)PlayAreaBlock.ConvertReverseDir((PlayAreaBlock.Dir)i)] = null;
				m_DisturbLinks[i] = null;
			}
		}
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
			Vector3 move = dir.normalized * GameManager.TimeStep * PPGame.Config.PanelSwapSpeed;
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
			waitTime -= GameManager.TimeStep;
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

			// 最下段なら連結妨害パネルも落下
			if (IsDisturbance && m_DisturbLinks[(int)PlayAreaBlock.Dir.Down] == null)
			{
				if (m_DisturbLinks[(int)PlayAreaBlock.Dir.Left] != null)
				{
					m_DisturbLinks[(int)PlayAreaBlock.Dir.Left].BeginFall(startBlock.GetLink(PlayAreaBlock.Dir.Left) as PPPanelBlock);
				}
				if (m_DisturbLinks[(int)PlayAreaBlock.Dir.Right] != null)
				{
					m_DisturbLinks[(int)PlayAreaBlock.Dir.Right].BeginFall(startBlock.GetLink(PlayAreaBlock.Dir.Right) as PPPanelBlock);
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
			Vector3 move = -Vector3.up * GameManager.TimeStep * PPGame.Config.PanelFallSpeed;
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
			time += GameManager.TimeStep;
			++count;
			m_SpriteRenderer.color = count % 16 < 8 ? Color.gray : Color.white;
			yield return null;
		}

		m_SpriteRenderer.color = Color.gray;

		float wait = delay;
		while (wait > 0)
		{
			wait -= GameManager.TimeStep;
			yield return null;
		}

		m_SpriteRenderer.enabled = false;
		
		wait = endTime - delay + wait;
		while (wait > 0)
		{
			wait -= GameManager.TimeStep;
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
	/// 妨害パネル分解
	/// </summary>
	public int Dissolve(int dissolveCount = 0)
	{
		if (m_State == State.None && IsDisturbance)
		{
			PPPanel bottomRight = null;
			bottomRight = GetEndDisturb(PlayAreaBlock.Dir.Down);
			bottomRight = bottomRight.GetEndDisturb(PlayAreaBlock.Dir.Right);

			// 全連結妨害パネルを取得
			List<PPPanel> disturbances = new List<PPPanel>();
			List<PPPanelBlock> arounds = new List<PPPanelBlock>();
			List<PPPanel> vertical = null;
			List<PPPanel> horizontal = null;
			// 右下から上端にかけてのパネル
			bottomRight.GetDisturbLinks(out vertical, PlayAreaBlock.Dir.Up);
			vertical.Insert(0, bottomRight);
			int line = 0;
			foreach (PPPanel vertPanel in vertical)
			{
				// 右端から左端にかけてのパネル
				vertPanel.GetDisturbLinks(out horizontal, PlayAreaBlock.Dir.Left);
				horizontal.Insert(0, vertPanel);
				foreach (PPPanel horizPanel in horizontal)
				{
					disturbances.Add(horizPanel);

					// 水平方向の周囲のブロックを保持
					if (line == 0 && horizPanel.Block != null)
						arounds.Add(horizPanel.Block.GetLink(PlayAreaBlock.Dir.Down) as PPPanelBlock);
					if (line == vertical.Count - 1 && horizPanel.Block != null)
						arounds.Add(horizPanel.Block.GetLink(PlayAreaBlock.Dir.Up) as PPPanelBlock);
				}

				// 垂直方向の周囲のブロックを保持
				if (horizontal.Count > 0)
				{
					if (horizontal[0].Block != null)
						arounds.Add(horizontal[0].Block.GetLink(PlayAreaBlock.Dir.Right) as PPPanelBlock);
					if (horizontal[horizontal.Count - 1].Block != null)
						arounds.Add(horizontal[horizontal.Count - 1].Block.GetLink(PlayAreaBlock.Dir.Left) as PPPanelBlock);
				}

				++line;
			}

			// 全連結妨害パネルを分解準備
			int currentCount = 0;
			foreach (PPPanel panel in disturbances)
			{
				if (panel.StandbyDissolve())
				{
					++currentCount;
				}
			}

			// 連結妨害パネルと接している別の妨害パネルも分解
			foreach (PPPanelBlock block in arounds)
			{
				if (block != null && block.AttachedPanel != null && block.AttachedPanel.IsDisturbance)
				{
					currentCount += block.AttachedPanel.Dissolve(dissolveCount + currentCount);
				}
			}

			// 全連結妨害パネルを分解
			float delay = dissolveCount * PPGame.Config.PanelDissolveDelay;
			float endTime = delay + currentCount * PPGame.Config.PanelDissolveDelay;
			foreach (PPPanel panel in disturbances)
			{
				panel.BeginDissolve(panel.m_DisturbLinks[(int)PlayAreaBlock.Dir.Down] == null, delay, endTime);
				delay += PPGame.Config.PanelDissolveDelay;
			}

			return currentCount;
		}

		return 0;
	}

	/// <summary>
	/// 妨害パネル分解準備
	/// </summary>
	/// <returns></returns>
	private bool StandbyDissolve()
	{
		if (m_State == State.None && IsDisturbance)
		{
			m_State = State.Dissolve;
			return true;
		}
		return false;
	}

	/// <summary>
	/// 妨害パネル分解開始
	/// </summary>
	private void BeginDissolve(bool unsetDisturb, float delay, float endTime)
	{
		if (m_State == State.Dissolve && IsDisturbance)
		{
			StartCoroutine(m_PlayingCoroutine = Dissolving(unsetDisturb, delay, endTime));
		}
	}
	
	/// <summary>
	/// 妨害パネル分解
	/// </summary>
	private IEnumerator Dissolving(bool unsetDisturb, float delay, float endTime)
	{
		// 初回は処理しない
		yield return null;

		// 明滅
		float time = 0f;
		int count = 0;
		while (time < 0.5f)
		{
			time += GameManager.TimeStep;
			++count;
			m_SpriteRenderer.color = count % 16 < 8 ? Color.gray : Color.white;
			yield return null;
		}

		m_SpriteRenderer.color = Color.yellow;

		// 待機
		float wait = delay;
		while (wait > 0)
		{
			wait -= GameManager.TimeStep;
			yield return null;
		}

		if (unsetDisturb)
		{
			UnsetDisturbance();
		}
		else
		{
			m_SpriteRenderer.color = Color.gray;
		}

		// 全体の終了を待つ
		wait = endTime - delay + wait;
		while (wait > 0)
		{
			wait -= GameManager.TimeStep;
			yield return null;
		}

		m_State = State.None;
		m_PlayingCoroutine = null;
		yield return null;
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
			// 最下段の水平方向の連結妨害パネルブロックを取得
			List<PPPanel> links;
			List<PPPanel> horizontal = new List<PPPanel>();
			PPPanel mostUnderDisturb = GetEndDisturb(PlayAreaBlock.Dir.Down);
			horizontal.Add(mostUnderDisturb);
			mostUnderDisturb.GetDisturbLinks(out links, PlayAreaBlock.Dir.Left);
			horizontal.AddRange(links);
			mostUnderDisturb.GetDisturbLinks(out links, PlayAreaBlock.Dir.Right);
			horizontal.AddRange(links);

			// 最下段の連結妨害パネルの直下が空ブロックか調べる
			foreach (PPPanel disturb in horizontal)
			{
				if (disturb.Block != null)
				{
					PPPanelBlock underBlock = disturb.Block.GetLink(PlayAreaBlock.Dir.Down) as PPPanelBlock;
					if (underBlock != null && underBlock.AttachedPanel != null)
					{
						return false;
					}
				}
			}
		}

		return true;
	}

	/// <summary>
	/// 指定方向の終端の連結妨害パネル取得
	/// </summary>
	private PPPanel GetEndDisturb(PlayAreaBlock.Dir dir)
	{
		PPPanel dest = null;
		if (IsDisturbance)
		{
			PPPanel t = this;
			while (t != null)
			{
				dest = t;
				t = t.m_DisturbLinks[(int)dir];
			}
		}
		return dest;
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
