using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MDPlayArea : MonoBehaviour
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
	public MDGame Game { get; private set; }

	/// <summary> ライン </summary>
	private List<MDDropBlock[]> m_Lines = new List<MDDropBlock[]>();

	/// <summary> 全ドロップ </summary>
	private List<MDDrop> m_Drops = new List<MDDrop>();

	/// <summary> 未使用ドロップ </summary>
	private List<MDDrop> m_UnusedDrops = new List<MDDrop>();

	/// <summary> プルしたドロップ </summary>
	private List<MDDrop> m_PulledDrop = new List<MDDrop>();
	/// <summary> プッシュ中ドロップ </summary>
	private List<MDDrop> m_PushingDrops = new List<MDDrop>();

	/// <summary> 消滅保留中ドロップ </summary>
	private List<MDDrop> m_PendingVanishDrops = new List<MDDrop>();

	/// <summary> ブロック親オブジェクト </summary>
	private GameObject m_BlockParent = null;
	/// <summary> ブロック親オブジェクト初期座標 </summary>
	private float m_BlockParentDefaultY = 0f;

	/// <summary> 連鎖数 </summary>
	public int ChainCount { get; private set; }

	/// <summary> 連鎖受付時間 </summary>
	public float ChainReceiveTime { get; private set; }

	/// <summary> 自動ライン生成待機時間 </summary>
	private float m_AutoLineCreateWaitTime = 0f;

	/// <summary> 更新有効 </summary>
	private bool m_EnableUpdate = true;

	private List<SpriteRenderer> m_BackLines = new List<SpriteRenderer>();
	private List<Color> m_BackLineDefaultColors = new List<Color>();
	
	public GameObject DropTemplate; // TODO: AssetBundle

	/// <summary>
	/// 生成
	/// </summary>
	protected void Awake()
	{
	}

	/// <summary>
	/// 初期化
	/// </summary>
	public void Initialize(MDGame game)
	{
		Game = game;

		m_BlockParent = transform.Find("Blocks").gameObject;
		m_BlockParentDefaultY = m_BlockParent.transform.position.y;

		Height = MDGame.Config.PlayAreaHeight;
		Width = MDGame.Config.PlayAreaWidth;

		BlockSize = MDGame.Config.DropSize;

		m_Size.x = Width * BlockSize;
		m_Size.y = Height * BlockSize;

		m_AutoLineCreateWaitTime = MDGame.Config.AutoLineCreateStartTime;

		// 背景を作成
		GameObject backParent = transform.Find("Back").gameObject;
		GameObject obj, template;
		int backTemplateCount = backParent.transform.childCount;
		for (int i = 0; i < Width; i++)
		{
			template = backParent.transform.GetChild(i % backTemplateCount).gameObject;

			if (i < backTemplateCount)
			{
				obj = template;
			}
			else
			{
				obj = Instantiate(template);
				obj.transform.SetParent(backParent.transform, false);
				obj.transform.localPosition = template.transform.localPosition;
			}

			// 座標登録
			Vector3 pos = obj.transform.localPosition;
			pos.x = i * BlockSize - (m_Size.x - BlockSize) / 2f;
			obj.transform.localPosition = pos;

			// オブジェクト名登録
			obj.name = StringManager.GetLeftMost(backParent.transform.GetChild(0).name) + "_" + i.ToString();

			m_BackLines.Add(obj.GetComponent<SpriteRenderer>());
			m_BackLineDefaultColors.Add(m_BackLines[i].color);
		}

		// 適当に初期化
		for (int i = 0; i < MDGame.Config.FirstDropLine; i++)
		{
			AddNewLine();
		}
	}

	/// <summary>
	/// 更新
	/// </summary>
	public void Step()
	{
		if (!m_EnableUpdate)
		{
			return;
		}

		// 連鎖受付時間更新
		ChainReceiveTime -= GameManager.TimeStep;

		// ライン更新
		UpdateLine();

		// ドロップ上詰め
		CloseDrop();

		// ドロップ消滅
		VanishDrop();

		// 背景ライン更新
		for (int i = 0; i < m_BackLines.Count; i++)
		{
			m_BackLines[i].color = m_BackLines[i].color + (m_BackLineDefaultColors[i] - m_BackLines[i].color) * GameManager.TimeStep * 5f;
		}

		// ゲームオーバー
		if (IsLineOver())
		{
		}
	}

	/// <summary>
	/// コルーチン再生
	/// </summary>
	public void StartPlayingCoroutine()
	{
		foreach (MDDrop drop in m_Drops)
		{
			drop.EndPause();
		}
	}
	
	/// <summary>
	/// コルーチン停止
	/// </summary>
	public void StopPlayingCoroutine()
	{
		foreach (MDDrop drop in m_Drops)
		{
			drop.BeginPause();
		}
	}

	/// <summary>
	/// ライン更新
	/// </summary>
	private void UpdateLine()
	{
		// ライン自動生成
		m_AutoLineCreateWaitTime -= GameManager.TimeStep;
		if (m_AutoLineCreateWaitTime <= 0)
		{
			for (int i = 0; i < MDGame.Config.GetAutoLineCreateCount(0); i++)
			{
				AddNewLine();
			}
			m_AutoLineCreateWaitTime = MDGame.Config.GetLAutoLineCreateInterval(0);
		}

		// ラインが全て表示される位置まで移動
		if (m_Lines[m_Lines.Count - 1][0].Position.y > m_BlockParentDefaultY - BlockHalfSize)
		{
			float scroll = Mathf.Min(GameManager.TimeStep * MDGame.Config.PlayAreaScrollSpeed, Mathf.Abs(m_Lines[m_Lines.Count - 1][0].Position.y - (m_BlockParentDefaultY - BlockHalfSize)));
			m_BlockParent.transform.position += Vector3.down * scroll;

			// 座標ループ
			float blocksY = m_BlockParent.transform.localPosition.y - m_BlockParentDefaultY;
			if (blocksY < -BlockSize)
			{
				foreach (Transform child in m_BlockParent.transform)
				{
					child.position += Vector3.down * Mathf.Abs(blocksY);
				}
				foreach (MDDropBlock[] line in m_Lines)
				{
					foreach (MDDropBlock block in line)
					{
						block.Position += Vector3.down * Mathf.Abs(blocksY);
					}
				}
				m_BlockParent.transform.position += Vector3.up * Mathf.Abs(blocksY);
			}
		}
	}

	/// <summary>
	/// ラインの範囲外までドロップがある?
	/// </summary>
	private bool IsLineOver()
	{
		//for (int row = 0; row < m_Width; row++)
		//{
		//	MDDropBlock block = GetMostUnderFullBlock(row);
		//	if (block.AttachedDrop != null && !block.AttachedDrop.IsValidVanish && block.AttachedDrop.CurrentState != MDDrop.State.Vanish && block.Position.y <= MinBlockPositionY)
		//	{
		//		return true;
		//	}
		//}
		return false;
	}

	/// <summary>
	/// ライン追加
	/// </summary>
	public void AddNewLine()
	{
		// 空のラインを追加
		AddEmptyLine(false);

		MDDrop drop = null;
		foreach (MDDropBlock block in m_Lines[m_Lines.Count - 1])
		{
			// ドロップ再利用
			if (m_UnusedDrops.Count > 0)
			{
				drop = m_UnusedDrops[0];
				m_UnusedDrops.RemoveAt(0);
			}
			// ドロップ生成
			else
			{
				drop = Instantiate(DropTemplate, transform).GetComponent<MDDrop>();
				drop.transform.SetParent(m_BlockParent.transform, false);
				m_Drops.Add(drop);
			}

			// 初期化
			drop.Initialize(this);

			// アタッチ
			block.Attach(drop, true);
		}

		// 2本以上空のラインがあったら1本残して消す
		int emptyLineCount = 0;
		bool empty = true;
		for (int i = 0; i < m_Lines.Count; i++)
		{
			foreach (MDDropBlock block in m_Lines[i])
			{
				if (block.IsAttachOrReserved)
				{
					empty = false;
					break;
				}
			}
			if (!empty)
			{
				break;
			}
			++emptyLineCount;
		}
		if (emptyLineCount >= 2)
		{
			for (int i = 0; i < emptyLineCount - 1; i++)
			{
				foreach (MDDropBlock block in m_Lines[1])
				{
					block.SetLink(null, PlayAreaBlock.Dir.Down);
				}
				m_Lines.RemoveAt(0);
			}
		}
	}

	/// <summary>
	/// 空のラインを追加
	/// </summary>
	private void AddEmptyLine(bool insertHead)
	{
		MDDropBlock[] line = new MDDropBlock[Width];
		Vector3 pos = Vector3.zero;
		for (int i = 0; i < line.Length; i++)
		{
			// ブロック生成
			line[i] = new MDDropBlock();
			line[i].BaseTransform = m_BlockParent.transform;

			// 座標登録
			pos = Vector3.zero;
			pos.x = i * BlockSize - (m_Size.x - BlockSize) / 2f;
			if (insertHead)
			{
				pos.y = m_Lines.Count > 0 ? m_Lines[0][0].LocalPosition.y - BlockSize : 0;
			}
			else
			{
				pos.y = m_Lines.Count > 0 ? m_Lines[m_Lines.Count - 1][0].LocalPosition.y + BlockSize : 0;
			}
			line[i].LocalPosition = pos;
		}

		// 現在の最上段/最下段の行を取得
		PlayAreaBlock[] sideLine = null;
		if (m_Lines.Count > 0)
		{
			sideLine = insertHead ? m_Lines[0] : m_Lines[m_Lines.Count - 1];
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
				line[i].SetLink(sideLine[i], insertHead ? PlayAreaBlock.Dir.Up : PlayAreaBlock.Dir.Down);

				if (sideLine[i] != null)
				{
					sideLine[i].SetLink(line[i], insertHead ? PlayAreaBlock.Dir.Down : PlayAreaBlock.Dir.Up);
				}
			}
		}

		// 行追加
		m_Lines.Insert(insertHead ? 0 : m_Lines.Count, line);
	}

	/// <summary>
	/// ドロップ消滅
	/// </summary>
	public void VanishDrop()
	{
		List<MDDrop> match = new List<MDDrop>();

		// 消滅中かチェンジのドロップがあったら終了
		foreach (MDDropBlock[] line in m_Lines)
		{
			foreach (MDDropBlock block in line)
			{
				if (block.AttachedDrop != null && (block.AttachedDrop.CurrentState == MDDrop.State.Vanish || block.AttachedDrop.CurrentState == MDDrop.State.Change))
				{
					// 連鎖受付時間リセット
					ChainReceiveTime = MDGame.Config.MaxChainReceiveTime;

					return;
				}
			}
		}

		bool usePendingVanishDrops = m_PendingVanishDrops.Count > 0;

		// 保留ドロップを取得
		if (usePendingVanishDrops)
		{
			foreach (MDDrop drop in m_PendingVanishDrops)
			{
				bool isAdjoinFrozen = false;
				drop.Block.GetMatchDrops(ref match, ref isAdjoinFrozen, drop.DropType);

				// 連鎖数カウント
				++ChainCount;

				// 連鎖受付時間リセット
				ChainReceiveTime = MDGame.Config.MaxChainReceiveTime;
			}
			m_PendingVanishDrops.Clear();
		}
		// マッチするドロップを検索
		else
		{
			int count, vanishCount;
			bool existsPushedDrop, forceVanish;
			bool isAdjoinFrozen = false;
			foreach (MDDropBlock[] line in m_Lines)
			{
				foreach (MDDropBlock block in line)
				{
					if (block.AttachedDrop != null && !block.AttachedDrop.IsIgnoreVanish && !match.Contains(block.AttachedDrop))
					{
						// 縦方向に繋がった同タイプドロップ数を取得
						block.GetVerticallMatchDropCount(out count, out vanishCount, out existsPushedDrop, out forceVanish);

						// 消滅扱い
						if ((count >= MDGame.Config.MinVanishMatchCount && ((vanishCount > 0 && count != vanishCount) || existsPushedDrop)) || forceVanish)
						{
							block.GetMatchDrops(ref match, ref isAdjoinFrozen, block.AttachedDrop.DropType);

							if (isAdjoinFrozen)
							{
								// 消滅ドロップを保留
								m_PendingVanishDrops.Add(block.AttachedDrop);
							}
							else
							{
								// 連鎖数カウント
								++ChainCount;

								// 連鎖受付時間リセット
								ChainReceiveTime = MDGame.Config.MaxChainReceiveTime;
							}
						}
					}
				}
			}

			// 氷ドロップチェンジ開始
			if (isAdjoinFrozen)
			{
				foreach (MDDropBlock[] line in m_Lines)
				{
					foreach (MDDropBlock block in line)
					{
						if (block.AttachedDrop != null && block.AttachedDrop.DropType == MDDrop.Type.Frozen)
						{
							block.AttachedDrop.BeginChange(match[0].DropType);
						}
					}
				}

				// 今回は消滅なし
				return;
			}
		}

		// 消滅開始
		foreach (MDDrop drop in match)
		{
			drop.BeginVanish();
		}

		bool needsResetChain = true;

		// 消滅を制限
		foreach (MDDropBlock[] line in m_Lines)
		{
			foreach (MDDropBlock block in line)
			{
				if (block.AttachedDrop != null)
				{
					if (!usePendingVanishDrops)
					{
						block.AttachedDrop.IsValidVanish = false;
						block.AttachedDrop.IsPushed = false;
					}
					if (block.AttachedDrop.CurrentState == MDDrop.State.Vanish)
					{
						needsResetChain = false;
					}
				}

				if (block.ReservedDrop != null)
				{
					needsResetChain = false;
				}
			}
		}

		// 連鎖数リセット
		if (needsResetChain && ChainReceiveTime <= 0f)
		{
			(Game.Player as MDPlayer).OnChainEnd(ChainCount);
			ChainCount = 0;
		}
	}

	/// <summary>
	/// ドロップ除外
	/// </summary>
	public void RemoveDrop(MDDrop drop)
	{
		foreach (MDDropBlock[] line in m_Lines)
		{
			foreach (MDDropBlock block in line)
			{
				if (block.AttachedDrop == drop || block.ReservedDrop == drop)
				{
					block.Detach();
					m_UnusedDrops.Add(drop);
					return;
				}
			}
		}
	}

	/// <summary>
	/// ドロップを詰める
	/// </summary>
	public void CloseDrop()
	{
		MDDropBlock emptyBlock = null;
		MDDrop drop = null;

		// 奥からチェック
		for (int i = m_Lines.Count - 1; i >= 0; i--)
		{
			foreach (MDDropBlock block in m_Lines[i])
			{
				if (block.ReservedDrop != null || (block.AttachedDrop != null && block.AttachedDrop.CurrentState != MDDrop.State.Vanish))
				{
					// 上方向の空ブロック取得
					emptyBlock = block.GetFarthestEmptyBlock(PlayAreaBlock.Dir.Up);

					// 空ブロックを予約
					if (emptyBlock != null)
					{
						// アタッチされてたドロップ
						if (block.AttachedDrop != null)
						{
							drop = block.AttachedDrop;
							block.Detach();
							emptyBlock.Reserve(drop);

							drop.IsValidVanish = true;

							// 上詰め開始
							drop.BeginClose();
						}
						// 予約されてたドロップ
						else
						{
							drop = block.ReservedDrop;
							block.CancelReserve();
							emptyBlock.Reserve(drop);
						}
					}
				}
			}
		}
  	}

	/// <summary>
	/// ドロップをプル
	/// </summary>
	public bool PullDrop(int row)
	{
		if (row < 0 || row >= Width || !IsValidPull(row))
		{
			return false;
		}

		int count = 0;
		// 最下段のドロップがあるブロックを取得
		MDDropBlock block = GetMostUnderFullBlock(row, false);
		if (block != null)
		{
			MDDrop drop = null;
			while (block != null && block.IsAttachOrReserved)
			{
				bool reserveDrop = block.ReservedDrop != null;
				drop = reserveDrop ? block.ReservedDrop : block.AttachedDrop;
				if ((!drop.IsLocked() || reserveDrop) && (m_PulledDrop.Count == 0 || drop.DropType == m_PulledDrop[0].DropType))
				{
					if (reserveDrop)
					{
						drop.CancelState();
						block.CancelReserve();
					}
					else
					{
						block.Detach();
					}

					drop.BeginPull(Game.Player as MDPlayer);

					m_PulledDrop.Add(drop);

					block = block.GetLink(PlayAreaBlock.Dir.Up) as MDDropBlock;
					++count;
				}
				else
				{
					break;
				}
			}
		}

		return count > 0;
	}

	/// <summary>
	/// ドロップをプッシュ
	/// </summary>
	public void PushDrop(int row)
	{
		if (row < 0 || row >= Width || !IsValidPush())
		{
			return;
		}

		// 最下段のドロップがあるブロックの下のブロックを取得
		MDDropBlock block = GetMostUnderFullBlock(row, false);
		block = block == null ? m_Lines[m_Lines.Count - 1][row] : block.GetLink(PlayAreaBlock.Dir.Down) as MDDropBlock;

		for (int i = 0; i < m_PulledDrop.Count; i++)
		{
			// ターゲットブロックが無ければ空のラインを追加
			if (block == null)
			{
				AddEmptyLine(true);
				block = m_Lines[0][row];
			}

			// 消滅可能
			m_PulledDrop[i].IsValidVanish = true;
			// プッシュ登録
			m_PulledDrop[i].IsPushed = true;
			// 即座に消えるプッシュ
			m_PulledDrop[i].IsMatchPushed = m_PulledDrop.Count >= MDGame.Config.MinVanishMatchCount;

			// 予約
			block.Reserve(m_PulledDrop[i]);

			// プッシュ
			m_PushingDrops.Add(m_PulledDrop[i]);
			m_PulledDrop[i].BeginPush(Game.Player.transform.position + Vector3.down * i * BlockSize);

			block = block.GetLink(PlayAreaBlock.Dir.Down) as MDDropBlock;
		}
		m_PulledDrop.Clear();
	}

	/// <summary>
	/// プッシュ終了イベント
	/// </summary>
	public void PushEndEvent(MDDrop drop)
	{
		m_PushingDrops.Remove(drop);
	}

	/// <summary>
	/// プッシュ中ドロップ数を取得
	/// </summary>
	public int GetPushingDropCount()
	{
		return m_PushingDrops.Count;
	}

	/// <summary>
	/// プルしたドロップを取得
	/// </summary>
	public MDDrop GetPulledDrop(int index)
	{
		return m_PulledDrop.Count > 0 ? m_PulledDrop[index] : null;
	}

	/// <summary>
	/// プルしたドロップ数を取得
	/// </summary>
	public int GetPulledDropCount()
	{
		return m_PulledDrop.Count;
	}

	/// <summary>
	/// プッシュ可能?
	/// </summary>
	public bool IsValidPush()
	{
		return m_PulledDrop.Count > 0 && m_PulledDrop.Find(x => x.CurrentState != MDDrop.State.None) == null;
	}

	/// <summary>
	/// プル可能?
	/// </summary>
	public bool IsValidPull(int row)
	{
		return m_PushingDrops.Count == 0 || m_PushingDrops.Find(x => x.Block != null && x.Block.GetLinkCount(PlayAreaBlock.Dir.Left) == row) == null;
	}

	/// <summary>
	/// 最下段のドロップがあるブロックを取得
	/// </summary>
	private MDDropBlock GetMostUnderFullBlock(int row, bool ignoreReserve = true)
	{
		MDDropBlock block = m_Lines[0][row];
		while (block != null && block.AttachedDrop == null && (ignoreReserve || block.ReservedDrop == null))
		{
			block = block.GetLink(PlayAreaBlock.Dir.Up) as MDDropBlock;
		}
		return block;
	}

	/// <summary>
	/// ブロックの座標を取得
	/// </summary>
	public Vector3 GetBlockPosition(int line, int row)
	{
		if (line < m_Lines.Count && line >= 0 && row < m_Lines[line].Length && row >= 0)
		{
			return m_Lines[line][row].Position;
		}
		return Vector3.zero;
	}

	/// <summary>
	/// 座標を列に変換
	/// </summary>
	public int ConvertPositionToRow(float positionX)
	{
		if (m_Lines.Count > 0)
		{
			for (int i = 0; i < m_Lines[0].Length; i++)
			{
				if (positionX < m_Lines[0][i].Position.x + BlockHalfSize)
				{
					return i;
				}
			}
			return m_Lines[0].Length - 1;
		}
		return -1;
	}

	/// <summary>
	/// ライン座標Yの最小値
	/// </summary>
	public float MinBlockPositionY
	{
		get { return m_BlockParentDefaultY - m_Size.y + BlockHalfSize; }
	}

	/// <summary>
	/// 背景ラインタッチ演出
	/// </summary>
	public void SetBackLineFlash(int row)
	{
		for (int i = 0; i < m_BackLines.Count; i++)
		{
			if (i == row)
			{
				m_BackLines[i].color *= 3f;
			}
			else
			{
				m_BackLines[i].color = m_BackLineDefaultColors[i];
			}
		}
  	}
}