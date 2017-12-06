using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPPlayArea : MonoBehaviour
{
	/// <summary> 最大行数 </summary>
	public int Height { get; private set; }
	/// <summary> 最大列数 </summary>
	public int Width { get; private set; }

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

		Height = PPGame.Config.PlayAreaHeight;
		Width = PPGame.Config.PlayAreaWidth;

		BlockSize = PPGame.Config.PanelSize;

		m_Size.x = Width * BlockSize;
		m_Size.y = Height * BlockSize;

		// 適当に初期化
		for (int i = 0; i < 1; i++)
		{
			AddNewLine(false, false);
		}

		m_BlockParent.transform.position += Vector3.up * (BlockSize * 1 - BlockHalfSize);
	}

	/// <summary>
	/// 更新
	/// </summary>
	public void Process()
	{
		// せり上げスピード登録
		ElevateValue = PPGame.Config.AutoElevateValue;
		MaxElevateWaitTime = PPGame.Config.GetAutoElevateInterval((Game.Player as PPPlayer).GameLevel);

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
		// コルーチン再生中のパネルを取得
		List<PPPanel> playingPanels = new List<PPPanel>();
		foreach (PPPanel panel in m_Panels)
		{
			if (panel.IsCoroutinePlaying)
			{
				playingPanels.Add(panel);
			}
		}

		// 下から処理するためソート
		playingPanels.Sort(delegate(PPPanel a, PPPanel b) { return (int)Mathf.Ceil(a.transform.position.y - b.transform.position.y); });

		// 一時停止終了
		foreach (PPPanel panel in playingPanels)
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
					if (block.AttachedPanel.IsChainSource && block.AttachedPanel.CurrentState != PPPanel.State.FallReady)
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
	public PPPanelBlock SwapPanel(PPPanelBlock block, PlayAreaBlock.Dir dir)
	{
		if (block != null && !block.IsLoced() && block.AttachedPanel != null && !block.AttachedPanel.IsDisturbance)
		{
			PPPanelBlock target = block.GetLink(dir) as PPPanelBlock;
			if (target != null && !target.IsLoced() && !target.GetIsUpperBlockFallWait() && (target.AttachedPanel == null || !target.AttachedPanel.IsDisturbance))
			{
				// 対象ブロックを落下中のパネルが通過中ならキャンセル
				foreach (PPPanel panel in m_UsingPanels)
				{
					if (panel.IsFalling())
					{
						if ((block.TestInsideY(panel.transform.position.y, BlockSize) && block.TestInsideX(panel.transform.position.x, BlockSize, false)) ||
							(target.TestInsideY(panel.transform.position.y, BlockSize) && target.TestInsideX(panel.transform.position.x, BlockSize, false)))
						{
							return null;
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
					while ((tempBlock = tempBlock.GetLink(PlayAreaBlock.Dir.Up) as PPPanelBlock) != null && tempBlock.AttachedPanel != null && !tempBlock.AttachedPanel.IsDisturbance)
					{
						tempBlock.AttachedPanel.IgnoreChainSource = true;
					}
				}

				return target;
			}
		}
		return null;
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
				if (!block.IsLoced() && block.AttachedPanel != null && block.AttachedPanel.IsValidFall())
				{
					// 落下地点取得
					if ((fallTarget = block.GetMostUnderEmptyBlock()) != null && fallTarget != block)
					{
						// 落下待機開始
						block.AttachedPanel.BeginFallReady(block);
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
			AddNewLine(false, false);
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
		DeleteEmptyLine();
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
	public PPPanelBlock[] AddNewLine(bool empty, bool insertHead)
	{
		// 現在の最上段/最下段の行を取得
		PPPanelBlock[] sideLine = null;
		if (m_Lines.Count > 0)
		{
			sideLine = insertHead ? m_Lines[0] : m_Lines[m_Lines.Count - 1];
		}

		PPPanelBlock[] line = new PPPanelBlock[Width];
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
			pos.y = sideLine != null ? sideLine[0].LocalPosition.y + (BlockSize * (insertHead ? 1 : -1)) : 0;
			block.LocalPosition = pos;

			// パネル作成
			if (!empty)
			{
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
				if (!insertHead && sideLine != null && sideLine[i].GetMatchPanelCountDir(PlayAreaBlock.Dir.Up) >= PPGame.Config.MinVanishMatchCount - 1)
				{
					ignoreTypes.Add(sideLine[i].AttachedPanel.PanelType);
				}

				// パネル初期化
				panel.Initialize(this, ignoreTypes);

				if (insertHead)
				{
						panel.Activate();
				}

				block.Attach(panel, true);

				m_UsingPanels.Add(panel);
			}

			line[i] = block;
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
			if (sideLine != null)
			{
				line[i].SetLink(sideLine[i], insertHead ? PlayAreaBlock.Dir.Down : PlayAreaBlock.Dir.Up);

				if (sideLine[i] != null)
				{
					sideLine[i].SetLink(line[i], insertHead ? PlayAreaBlock.Dir.Up : PlayAreaBlock.Dir.Down);
					if (!insertHead && sideLine[i].AttachedPanel != null)
					{
						sideLine[i].AttachedPanel.Activate();
					}
				}
			}
		}

		// 行を追加
		if (insertHead)
		{
			m_Lines.Insert(0, line);
		}
		else
		{
			m_Lines.Add(line);
		}

		return line;
	}
	
	/// <summary>
	/// 余分な空の行を削除
	/// </summary>
	private void DeleteEmptyLine()
	{
		bool empty;
		for (int i = 0; m_Lines.Count > Height; i++)
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
	/// 妨害パネル作成
	/// </summary>
	public void CreateDisturbPanel(int width, int height)
	{
		// 空の行で埋める
		int emptyLineCount = Height - m_Lines.Count;
		for (int i = 0; i < emptyLineCount; i++)
		{
			AddNewLine(true, true);
		}

		// 余分な行を消す
		DeleteEmptyLine();

		// 行追加
		List<PPPanelBlock[]> lines = new List<PPPanelBlock[]>();
		for (int i = 0; i < height; i++)
		{
			lines.Add(AddNewLine(false, true));
		}
		
		// 妨害パネル化
		PPPanel left, right, up, down;
		for (int i = 0; i < lines.Count; i++)
		{
			for (int j = 0; j < lines[i].Length; j++)
			{
				if (lines[i][j].AttachedPanel != null)
				{
					left = j > 0 ? lines[i][j - 1].AttachedPanel : null;
					right = j + 1 < lines[i].Length ? lines[i][j + 1].AttachedPanel : null;
					up = i + 1 < lines.Count ? lines[i + 1][j].AttachedPanel : null;
					down = i > 0 ? lines[i - 1][j].AttachedPanel : null;
					lines[i][j].AttachedPanel.SetDisturbance(true, left, right, up, down);
				}
			}
		}
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
	/// ワールド座標からブロックを取得
	/// </summary>
	public PPPanelBlock GetBlock(Vector3 worldPosition)
	{
		Vector2i grid = ConvertWorldToGrid(worldPosition);
		if (grid.x >= 0 && grid.x < Width && grid.y >= 0 && grid.y < m_Lines.Count)
		{
			return GetBlock(grid);
		}
		return null;
	}

	/// <summary>
	/// パネルからブロックを取得
	/// </summary>
	private PPPanelBlock GetBlock(PPPanel panel)
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
	/// ワールド座標をグリッド座標に変換
	/// </summary>
	public Vector2i ConvertWorldToGrid(Vector3 worldPosition)
	{
		Vector3 areaOrigin = m_Lines[0][0].Position + new Vector3(-BlockHalfSize, BlockHalfSize, 0);
		Vector3 local = worldPosition - areaOrigin;
		return new Vector2i((int)(local.x / BlockSize), (int)(-local.y / BlockSize));
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
				if (block.AttachedPanel != null && !block.AttachedPanel.IsDisturbance && !block.VanishRequested && block.GetMatchPanelBlocks(out match))
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
