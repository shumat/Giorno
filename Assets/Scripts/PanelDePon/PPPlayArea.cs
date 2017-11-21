using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPPlayArea : MonoBehaviour
{
	/// <summary> 最大行数 </summary>
	private int m_Height;
	/// <summary> 最大列数 </summary>
	private int m_Width;

	/// <summary> サイズ </summary>
	private Vector3 m_Size = Vector2.zero;

	/// <summary> ブロックサイズ </summary>
	public float BlockSize { get; private set; }

	/// <summary> ブロックハーフサイズ </summary>
	public float BlockHalfSize { get { return BlockSize * 0.5f; } }

	/// <summary> ゲーム </summary>
	public PPGame Game { get; private set; }

	/// <summary> ブロック親オブジェクト </summary>
	private GameObject m_BlockParent = null;
	/// <summary> ブロック親オブジェクト初期座標 </summary>
	private float m_BlockParentDefaultY = 0f;

	/// <summary> ライン </summary>
	private List<PPPanelBlock[]> m_Lines = new List<PPPanelBlock[]>();

	/// <summary> 全パネル </summary>
	private List<PPPanel> m_Panels = new List<PPPanel>();
	/// <summary> 使用中パネル </summary>
	private List<PPPanel> m_UsingPanels = new List<PPPanel>();
	/// <summary> 未使用パネル </summary>
	private List<PPPanel> m_UnusedPanels = new List<PPPanel>();

	/// <summary> パネル除外リクエスト </summary>
	List<PPPanel> m_PanelRemoveRequests = new List<PPPanel>();

	/// <summary> せり上げ停止時間 </summary>
	private float m_ElevateStopTime = 0f;

	/// <summary> せり上げ待機時間 </summary>
	private float m_ElevateWaitTime = 0f;

	/// <summary> せり上げスピード </summary>
	public float ElevateValue { get; set; }

	/// <summary> せり上げ実行間隔 </summary>
	public float MaxElevateWaitTime { get; set; }

	/// <summary> 連鎖数 </summary>
	public int m_ChainCount = 0;

	public GameObject PanelTemplate; // TODO: AssetBundle

	/// <summary>
	/// 生成
	/// </summary>
	protected void Awake()
	{
		m_BlockParent = transform.Find("Blocks").gameObject;
		m_BlockParentDefaultY = m_BlockParent.transform.position.y;
	}

	/// <summary>
	/// 初期化
	/// </summary>
	public void Initialize(PPGame game)
	{
		Game = game;

		m_Height = PPGame.Config.PlayAreaHeight;
		m_Width = PPGame.Config.PlayAreaWidth;

		BlockSize = PPGame.Config.PanelSize;

		m_Size.x = m_Width * BlockSize;
		m_Size.y = m_Height * BlockSize;

		// 適当に初期化
		for (int i = 0; i < 5; i++)
		{
			AddNewLine();
		}

		m_BlockParent.transform.position += Vector3.up * (BlockSize * 5 - BlockHalfSize);
	}

	/// <summary>
	/// 更新
	/// </summary>
	public void Process()
	{
		// パネル除外
		RemovePanel();

		// パネル消滅
		VanishPanel();

		// パネル落下
		FallPanel();

		// 連鎖数更新
		UpdateChainCount();

		// せり上げ
		Elevate();
	}

	/// <summary>
	/// コルーチン再生
	/// </summary>
	public void StartPlayingCoroutine()
	{
		foreach (PPPanel panel in m_Panels)
		{
			panel.EndPause();
		}
	}
	
	/// <summary>
	/// コルーチン停止
	/// </summary>
	public void StopPlayingCoroutine()
	{
		foreach (PPPanel panel in m_Panels)
		{
			panel.BeginPause();
		}
	}

	/// <summary>
	/// パネル消滅
	/// </summary>
	private void VanishPanel()
	{
		List<PPPanelBlock> blocks;

		if (GetAllMatchPanelBlocks(out blocks))
		{
			// ソート
			blocks.Sort((a, b) =>
			{
				int dif = (int)Mathf.Ceil(b.Position.y - a.Position.y);
				return dif != 0 ? dif : (int)Mathf.Ceil(a.Position.x - b.Position.x);
			});

			// 消滅
			float delay = PPGame.Config.PanelVanishDelay;
			float totalDelay = delay * blocks.Count;
			bool existChainSource = false;
			foreach (PPPanelBlock block in blocks)
			{
				if (block.AttachedPanel != null)
				{
					block.VanishPanel(delay, totalDelay);
					delay += PPGame.Config.PanelVanishDelay;

					// 連鎖ソースチェック
					if (block.AttachedPanel.IsChainSource && !block.AttachedPanel.IsFallWait)
					{
						existChainSource = true;
					}
				}
			}

			// 連鎖カウント
			if (existChainSource)
			{
				++m_ChainCount;
			}
		}

		// 連鎖ソースフラグを折る
		foreach (PPPanelBlock[] line in m_Lines)
		{
			foreach (PPPanelBlock block in line)
			{
				if (block.AttachedPanel != null)
				{
					block.AttachedPanel.IsChainSource = false;
				}
			}
		}
	}

	/// <summary>
	/// 連鎖数更新
	/// </summary>
	private void UpdateChainCount()
	{
		int chainSourceCount = 0;

		foreach (PPPanel panel in m_UsingPanels)
		{
			if (panel.IsChainSource)
			{
				// 連鎖ソース数カウント
				++chainSourceCount;
			}
		}

		// 連鎖ソースが0なら連鎖リセット
		if (chainSourceCount == 0)
		{
			if (m_ChainCount > 0)
			{
				m_ElevateStopTime = PPGame.Config.GetElevateStopTime(m_ChainCount);
				(Game.Player as PPPlayer).ChainEvent(m_ChainCount);
			}

			m_ChainCount = 0;
		}
	}

	/// <summary>
	/// パネル交換
	/// </summary>
	public void SwapPanel(PPPanelBlock block, PlayAreaBlock.Dir dir)
	{
		if (block != null && !block.IsLoced() && block.AttachedPanel != null && !block.AttachedPanel.IsFallWait)
		{
			PPPanelBlock target = block.GetLink(dir) as PPPanelBlock;
			if (target != null && !target.IsLoced() && (target.AttachedPanel == null || !target.AttachedPanel.IsFallWait) && !target.GetIsUpperBlockFallWait())
			{
				// 対象ブロックを落下中のパネルが通過中ならキャンセル
				foreach (PPPanel panel in m_UsingPanels)
				{
					if (panel.IsFalling())
					{
						if ((block.TestInsideY(panel.transform.position.y, BlockSize) && block.TestInsideX(panel.transform.position.x, BlockSize, false)) ||
							(target.TestInsideY(panel.transform.position.y, BlockSize) && target.TestInsideX(panel.transform.position.x, BlockSize, false)))
						{
							return;
						}
					}
				}

				// 交換
				PPPanel t = block.AttachedPanel;
				block.Attach(target.AttachedPanel);
				target.Attach(t);

				// スワップ開始
				if (block.AttachedPanel != null)
				{
					block.AttachedPanel.BeginSwap(block);
				}
				if (target.AttachedPanel != null)
				{
					target.AttachedPanel.BeginSwap(target);
				}

				// 移動先の下が空ブロックなら連鎖無効
				PPPanelBlock tempBlock = target.GetLink(PlayAreaBlock.Dir.Down) as PPPanelBlock;
				if (tempBlock != null && tempBlock.AttachedPanel == null)
				{
					target.AttachedPanel.IgnoreChainSource = true;
				}
				// 空ブロックとの交換なら移動元から上を連鎖無効
				if (block.AttachedPanel == null)
				{
					tempBlock = block;
					while ((tempBlock = tempBlock.GetLink(PlayAreaBlock.Dir.Up) as PPPanelBlock) != null && tempBlock.AttachedPanel != null)
					{
						tempBlock.AttachedPanel.IgnoreChainSource = true;
					}
				}
			}
		}
	}

	/// <summary>
	/// パネル落下
	/// </summary>
	public void FallPanel()
	{
		PPPanelBlock fallTarget = null;

		// 下から2行目から上をチェック
		for (int i = m_Lines.Count - 2; i >= 0; i--)
		{
			foreach (PPPanelBlock block in m_Lines[i])
			{
				if (!block.IsLoced() && block.AttachedPanel != null)
				{
					// 落下地点取得
					if ((fallTarget = block.GetMostUnderEmptyBlock()) != null && fallTarget != block)
					{
						// 落下待機開始
						block.AttachedPanel.BeginFallReady(block, fallTarget);
					}
				}
			}
		}
	}

	/// <summary>
	/// せり上げ
	/// </summary>
	private void Elevate()
	{
		// せり上げ停止中
		m_ElevateStopTime -= Time.deltaTime;
		if (m_ElevateStopTime > 0f)
		{
			return;
		}

		// 毎フレーム上げると気持ち悪いのでカクつかせる
		m_ElevateWaitTime -= Time.deltaTime;
		if (m_ElevateWaitTime > 0f)
		{
			return;
		}
		else
		{
			m_ElevateWaitTime = MaxElevateWaitTime;
		}

		// 消滅中か落下中のパネルがあったらせり上げ不可
		foreach (PPPanel panel in m_UsingPanels)
		{
			if (panel.CurrentState == PPPanel.State.Vanish || panel.CurrentState == PPPanel.State.Fall)
			{
				return;
			}
		}

		// 移動
		m_BlockParent.transform.position += Vector3.up * ElevateValue;

		// 足りない分ラインを追加
		while (m_Lines[m_Lines.Count - 1][0].Position.y > m_BlockParentDefaultY + BlockHalfSize)
		{
			AddNewLine();
		}

		// 座標ループ
		float blocksY = m_BlockParent.transform.localPosition.y - m_BlockParentDefaultY;
		if (blocksY > BlockSize)
		{
			foreach (Transform child in m_BlockParent.transform)
			{
				child.position += Vector3.up * Mathf.Abs(blocksY);
			}
			foreach (PPPanelBlock[] line in m_Lines)
			{
				foreach (PPPanelBlock block in line)
				{
					block.Position += Vector3.up * Mathf.Abs(blocksY);
				}
			}
			m_BlockParent.transform.position += Vector3.down * Mathf.Abs(blocksY);
		}

		// 余分な行を消す
		bool empty;
		for (int i = 0; i < m_Lines.Count - 1; i++)
		{
			empty = true;
			foreach (PPPanelBlock block in m_Lines[i])
			{
				if (block.AttachedPanel != null)
				{
					empty = false;
				}
			}
			if (empty)
			{
				// 接続解除
				foreach (PPPanelBlock block in m_Lines[i + 1])
				{
					block.SetLink(null, PlayAreaBlock.Dir.Up);
				}
				// ライン破棄
				m_Lines.RemoveAt(i--);
			}
			else
			{
				break;
			}
		}
	}

	/// <summary>
	/// せり上げ時間間隔スキップ
	/// </summary>
	public void SkipElevateWait()
	{
		m_ElevateWaitTime = 0;
	}

	/// <summary>
	/// 新規行追加
	/// </summary>
	public void AddNewLine()
	{
		// 現在の最後の行を取得
		PPPanelBlock[] bottomLine = null;
		if (m_Lines.Count > 0)
		{
			bottomLine = m_Lines[m_Lines.Count - 1];
		}

		PPPanelBlock[] line = new PPPanelBlock[m_Width];
		PPPanel panel;
		PPPanelBlock block;
		Vector3 pos;
		List<PPPanel.Type> ignoreTypes = new List<PPPanel.Type>();

		// パネル生成
		for (int i = 0; i < line.Length; i++)
		{
			// ブロック生成
			block = new PPPanelBlock();
			block.BaseTransform = m_BlockParent.transform;

			// 座標登録
			pos = Vector3.zero;
			pos.x = i * BlockSize - m_Size.x / 2f + BlockHalfSize;
			pos.y = m_Lines.Count > 0 ? m_Lines[m_Lines.Count - 1][0].LocalPosition.y - BlockSize : 0;
			block.LocalPosition = pos;

			// 使わなくなったパネルを再利用
			if (m_UnusedPanels.Count > 0)
			{
				panel = m_UnusedPanels[0];
				m_UnusedPanels.RemoveAt(0);
			}
			// パネル生成
			else
			{
				panel = Instantiate(PanelTemplate).GetComponent<PPPanel>();
				panel.gameObject.transform.SetParent(m_BlockParent.transform);
				m_Panels.Add(panel);
			}

			// 無効タイプ設定のため左側だけ接続
			if (i > 0)
			{
				block.SetLink(line[i - 1], PlayAreaBlock.Dir.Left);
			}

			// 無効タイプ設定
			ignoreTypes.Clear();
			if (i > 0 && line[i - 1].GetMatchPanelCountDir(PlayAreaBlock.Dir.Left) >= PPGame.Config.MinVanishMatchCount - 1)
			{
				ignoreTypes.Add(line[i - 1].AttachedPanel.PanelType);
			}
			if (bottomLine != null && bottomLine[i].GetMatchPanelCountDir(PlayAreaBlock.Dir.Up) >= PPGame.Config.MinVanishMatchCount - 1)
		    {
				ignoreTypes.Add(bottomLine[i].AttachedPanel.PanelType);
		    }

			// パネル初期化
			panel.Initialize(this, ignoreTypes);

			block.Attach(panel, true);

			line[i] = block;

			m_UsingPanels.Add(panel);
		}

		// 隣接するブロックを登録
		for (int i = 0; i < line.Length; i++)
		{
			if (i < line.Length - 1)
			{
				line[i].SetLink(line[i + 1], PlayAreaBlock.Dir.Right);
			}
			if (i > 0)
			{
				line[i].SetLink(line[i - 1], PlayAreaBlock.Dir.Left);
			}
			if (bottomLine != null)
			{
				line[i].SetLink(bottomLine[i], PlayAreaBlock.Dir.Up);

				if (bottomLine[i] != null)
				{
					bottomLine[i].SetLink(line[i], PlayAreaBlock.Dir.Down);
					if (bottomLine[i].AttachedPanel != null)
					{
						bottomLine[i].AttachedPanel.Activate();
					}
				}
			}
		}

		// 行を追加
		m_Lines.Add(line);
	}

	/// <summary>
	/// ヒットしたパネル取得
	/// </summary>
	public PPPanel GetHitPanel(Vector2 worldPoint)
	{
		Collider2D collider = Physics2D.OverlapPoint(worldPoint);
		if (collider != null)
		{
			PPPanel panel = collider.gameObject.GetComponent<PPPanel>();
			if (panel != null)
			{
				return panel;
			}
		}

		return null;
	}

	/// <summary>
	/// ヒットしたブロック取得
	/// </summary>
	public PPPanelBlock GetHitBlock(Vector3 screenPosition)
	{
		return FindBlock(GetHitPanel(screenPosition));
	}

	/// <summary>
	/// ブロック検索
	/// </summary>
	private PPPanelBlock FindBlock(PPPanel panel)
	{
		if (panel == null)
		{
			return null;
		}

		foreach (PPPanelBlock[] line in m_Lines)
		{
			foreach (PPPanelBlock block in line)
			{
				if (block.AttachedPanel == panel)
				{
					return block;
				}
			}
		}
		return null;
	}

	/// <summary>
	/// ブロックのグリッド座標を取得
	/// </summary>
	public Vector2i GetBlockGrid(PPPanelBlock block)
	{
		for (int y = 0; y < m_Lines.Count; y++)
		{
			for (int x = 0; x < m_Lines[y].Length; x++)
			{
				if (m_Lines[y][x] == block)
				{
					return new Vector2i(x, y);
				}
			}
		}

		Debug.LogWarning(block.ToString() + " is not found");
		return Vector2i.zero;
	}

	/// <summary>
	/// グリッド座標からブロックを取得
	/// </summary>
	public PPPanelBlock GetBlock(Vector2i grid)
	{
		return m_Lines[grid.y][grid.x];
	}

	/// <summary>
	/// 全消滅ブロック取得
	/// </summary>
	private bool GetAllMatchPanelBlocks(out List<PPPanelBlock> blocks)
	{
		blocks = new List<PPPanelBlock>();

		List<PPPanelBlock> match;
		foreach (PPPanelBlock[] line in m_Lines)
		{
			foreach (PPPanelBlock block in line)
			{
				if (block.AttachedPanel != null && !block.VanishRequested && block.GetMatchPanelBlocks(out match))
				{
					foreach (PPPanelBlock item in match)
					{
						if (!item.VanishRequested)
						{
							item.VanishRequested = true;
							blocks.Add(item);
						}
					}
				}
			}
		}

		return blocks.Count > 0;
	}

	/// <summary>
	/// パネル除外リクエスト
	/// </summary>
	public void RequestRemovePanel(PPPanel panel)
	{
		m_PanelRemoveRequests.Add(panel);
	}

	/// <summary>
	/// パネル除外
	/// </summary>
	public void RemovePanel()
	{
		foreach (PPPanel panel in m_PanelRemoveRequests)
		{
			// パネルリスト更新
			m_UsingPanels.Remove(panel);
			if (!m_UnusedPanels.Contains(panel))
			{
				m_UnusedPanels.Add(panel);
			}

			// デタッチ
			if (panel.Block != null)
			{
				panel.Block.Detach();
			}
		}

		m_PanelRemoveRequests.Clear();
	}

	/// <summary>
	/// 連鎖数
	/// </summary>
	public int ChainCount
	{
		get { return m_ChainCount; }
	}
}
