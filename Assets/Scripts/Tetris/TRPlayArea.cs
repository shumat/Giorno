using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TRPlayArea : MonoBehaviour
{
	/// <summary> 行数 </summary>
	public int Height { get; private set; }
	/// <summary> 列数 </summary>
	public int Width { get; private set; }

	/// <summary> サイズ </summary>
	private Vector3 m_Size = Vector2.zero;

	/// <summary> メイン処理コルーチン </summary>
	private IEnumerator m_MainCoroutine = null;

	/// <summary> ライン </summary>
	private List<TRPanelBlock[]> m_Lines = new List<TRPanelBlock[]>();

	/// <summary> 未使用のパネル </summary>
	private List<TRPanel> m_UnusedPanels = new List<TRPanel>();

	/// <summary> ブロック親オブジェクト </summary>
	private GameObject m_BlockParent = null;

	/// <summary> 操作中のテトロミノ </summary>
	private TRTetromino m_Tetromino = null;

	/// <summary> テトロミノを形成するパーツ </summary>
	private List<TRPanel> m_TetrominoParts = new List<TRPanel>();
	
	/// <summary> 次のテトロミノリスト </summary>
	private List<int> m_NextTetrominoTypes = new List<int>();

	/// <summary> 現在のライン </summary>
	public int CurrentLine { get; private set; }

	/// <summary> 自動下移動時間間隔 </summary>
	private float m_MoveDownInterval = 1f;
	/// <summary> 自動下移動待機時間 </summary>
	private float m_MoveDownWaitTime = 0;

	/// <summary> テトロミノ位置を確定させる猶予時間 </summary>
	private float m_TetrominoAttachPostTime = 0.5f;
	/// <summary> テトロミノ位置を確定させる待機時間 </summary>
	private float m_TetrominoAttachWaitTime = 0;

	public GameObject PanelTemplate; // TODO: AssetBundle

	/// <summary>
	/// 生成
	/// </summary>
	protected void Awake()
	{
		m_BlockParent = transform.Find("Blocks").gameObject;
	}

	/// <summary>
	/// 初期化
	/// </summary>
	public void Initialize()
	{
		// サイズ登録
		Height = TRGame.Config.PlayAreaHeight;
		Width = TRGame.Config.PlayAreaWidth;
		m_Size.x = Width * TRPanelBlock.Size;
		m_Size.y = Height * TRPanelBlock.Size;

		// ブロック生成
		Vector3 pos;
		for (int i = 0; i < Height; i++)
		{
			TRPanelBlock[] line = new TRPanelBlock[Width];
			for (int row = 0; row < Width; row++)
			{
				line[row] = new TRPanelBlock();
				line[row].BaseTransform = m_BlockParent.transform;

				pos = Vector3.zero;
				pos.x = row * TRPanelBlock.Size - (m_Size.x - TRPanelBlock.Size) / 2f;
				pos.y = -(i * TRPanelBlock.Size + TRPanelBlock.HalfSize);
				line[row].LocalPosition = pos;

				// 連結
				if (row > 0)
				{
					line[row].SetLink(line[row - 1], PlayAreaBlock.Dir.Left);
					line[row - 1].SetLink(line[row], PlayAreaBlock.Dir.Right);
				}
				if (m_Lines.Count > 0)
				{
					line[row].SetLink(m_Lines[i - 1][row], PlayAreaBlock.Dir.Up);
					m_Lines[i - 1][row].SetLink(line[row], PlayAreaBlock.Dir.Down);
				}
			}

			m_Lines.Add(line);
		}

		// テトロミノ生成
		m_Tetromino = new TRTetromino();
		CreateNewTetromino();
	}

	/// <summary>
	/// メイン処理開始
	/// </summary>
	public void BeginProcess()
	{
		if (m_MainCoroutine == null)
		{
			m_MainCoroutine = Process();
			StartCoroutine(m_MainCoroutine);
		}
	}

	/// <summary>
	/// メイン処理
	/// </summary>
	private IEnumerator Process()
	{
		// ラインを下げる
		DownLine();

		// パネル消滅
		VanishPanel();

		// パネル落下
		FallPanel();

		if (!IsTetrominoFlying())
		{
			m_TetrominoAttachWaitTime -= Time.deltaTime;
			if (m_TetrominoAttachWaitTime < 0)
			{
				AttachTetromino();

				if (!CreateNewTetromino())
				{
					Debug.Log("Over");
				}
			}
		}

		// デバッグネクスト変更
		if (Input.GetMouseButtonDown(1))
		{
			SetNextTetromino((m_NextTetrominoTypes[0] + 1) % m_Tetromino.NumType);
		}

		// 1フレーム待機
		yield return null;

		m_MainCoroutine = Process();
		StartCoroutine(m_MainCoroutine);
	}

	/// <summary>
	/// ステップ実行
	/// </summary>
	public void StepUpdate()
	{
		if (m_MainCoroutine != null)
		{
			StartCoroutine(m_MainCoroutine);
			StopCoroutine(m_MainCoroutine);
		}
	}

	/// <summary>
	/// 一時停止
	/// </summary>
	public void SetPause(bool pause)
	{
		if (m_MainCoroutine != null)
		{
			if (pause)
			{
				StopCoroutine(m_MainCoroutine);
			}
			else
			{
				StartCoroutine(m_MainCoroutine);
			}
		}
	}

	#region Tetromino

	/// <summary>
	/// テトロミノ新規作成
	/// </summary>
	public bool CreateNewTetromino()
	{
		// 初期化
		m_Tetromino.Initialize(GetNextTetromino());

		// 初期座標
		m_Tetromino.GridPosition = new Vector2i(Width / 2, 1);

		CurrentLine = m_Tetromino.GridPosition.y;

		ResetDownLineWait();
		ResetTetrominoAttachWait();

		TRGame.Player.NewTetrominoCreateEvent();

		// 姿勢更新
		return UpdateTetromino();
	}

	/// <summary>
	/// 次のテトロミノを取得
	/// </summary>
	private int GetNextTetromino()
	{
		for (int i = 0; i < 5 - m_NextTetrominoTypes.Count; i++)
		{
			m_NextTetrominoTypes.Add(Random.Range(0, m_Tetromino.NumType));
		}
		int next = m_NextTetrominoTypes[0];
		m_NextTetrominoTypes.RemoveAt(0);
		return next;
	}
	
	/// <summary>
	/// 次のテトロミノを登録
	/// </summary>
	public void SetNextTetromino(int type)
	{
		if (m_NextTetrominoTypes.Count == 0)
		{
			m_NextTetrominoTypes.Add(type);
		}
		else
		{
			m_NextTetrominoTypes[0] = type;
		}
	}

	/// <summary>
	/// テトロミノ姿勢更新
	/// </summary>
	public bool UpdateTetromino(bool apply = true)
	{
		List<int> reserveBlocks = new List<int>();
		for (int y = 0; y < m_Tetromino.ShapeSize; y++)
		{
			for (int x = 0; x < m_Tetromino.ShapeSize; x++)
			{
				// パネルあり
				if (m_Tetromino.GetShapeGrid(x, y))
				{
					// はまる位置
					Vector2i grid = m_Tetromino.GridPosition - m_Tetromino.GetAxisGrid() + new Vector2i(x, y);

					// 範囲外
					if (grid.x < 0 || grid.x >= Width || grid.y < 0 || grid.y >= Height)
					{
						return false;
					}

					// はまる予定のブロック
					TRPanelBlock block = m_Lines[grid.y][grid.x];

					// 既に埋まってる
					if (block != null && block.AttachedPanel != null)
					{
						return false;
					}

					// ブロック予約
					reserveBlocks.Add(grid.y * Width + grid.x);
				}
			}
		}

		if (apply)
		{
			// 前回までの予約キャンセル
			foreach (TRPanel parts in m_TetrominoParts)
			{
				m_UnusedPanels.Add(parts);
				parts.Block.CancelReserve();
				parts.Deactivate();
			}
			m_TetrominoParts.Clear();

			for (int i = 0; i < reserveBlocks.Count; i++)
			{
				TRPanel panel;

				// パネル再利用
				if (m_UnusedPanels.Count > 0)
				{
					panel = m_UnusedPanels[0];
					m_UnusedPanels.RemoveAt(0);
				}
				// パネル生成
				else
				{
					panel = Instantiate(PanelTemplate).GetComponent<TRPanel>();
					panel.transform.SetParent(m_BlockParent.transform);
				}

				panel.Initialize(this, m_Tetromino.PanelColor);
				m_TetrominoParts.Add(panel);

				// ブロック予約
				m_Lines[reserveBlocks[i] / Width][reserveBlocks[i] % Width].Reserve(panel, true);
			}
		}

		return true;
	}

	/// <summary>
	/// テトロミノ姿勢確定
	/// </summary>
	public void AttachTetromino()
	{
		foreach (TRPanel panel in m_TetrominoParts)
		{
			if (panel.Block != null)
			{
				panel.Block.ApplyhReservedDrop();
			}
		}
		m_TetrominoParts.Clear();
	}

	/// <summary>
	/// テトロミノ確定待機時間リセット
	/// </summary>
	public void ResetTetrominoAttachWait()
	{
		m_TetrominoAttachWaitTime = m_TetrominoAttachPostTime;
	}

	/// <summary>
	/// テトロミノ移動
	/// </summary>
	public void MoveTetromino(Vector2i target)
	{
		Vector2i prev = m_Tetromino.GridPosition;

		target.y = Mathf.Max(target.y, CurrentLine);

		// 垂直移動
		if (target.y != m_Tetromino.GridPosition.y)
		{
			int dir = Mathf.Clamp(target.y - m_Tetromino.GridPosition.y, -1, 1);
			m_Tetromino.GridPosition += new Vector2i(0, dir);
			if (!UpdateTetromino())
			{
				m_Tetromino.GridPosition -= new Vector2i(0, dir);
			}
		}

		// 水平移動
		if (target.x != m_Tetromino.GridPosition.x)
		{
			int dir = Mathf.Clamp(target.x - m_Tetromino.GridPosition.x, -1, 1);
			// 移動不可ならストップ
			m_Tetromino.GridPosition += new Vector2i(dir, 0);
			if (!UpdateTetromino())
			{
				m_Tetromino.GridPosition -= new Vector2i(dir, 0);
			}
		}

		// ライン更新
		if (CurrentLine != m_Tetromino.GridPosition.y)
		{
			ResetDownLineWait();
			CurrentLine = m_Tetromino.GridPosition.y;
		}

		// 確定猶予
		if (prev != m_Tetromino.GridPosition)
		{
			ResetTetrominoAttachWait();
		}
	}

	/// <summary>
	/// テトロミノ回転
	/// </summary>
	public void RotateTetromino(bool negative)
	{
		m_Tetromino.Rotate(negative);

		// 回転のみ
		if (UpdateTetromino())
		{
			// 確定猶予
			ResetTetrominoAttachWait();

			// 成功
			return;
		}

		Vector2i center = m_Tetromino.GetCenterGrid();
		Vector2i min, max;
		m_Tetromino.GetShapeBox(out min, out max);
		int downCheckCount = center.y - min.y + 1;
		int upCheckCount = max.y - center.y + 1;
		int rightCheckCount = center.x - min.x + 1;
		int leftCheckCount = max.x - center.x + 1;

		List<PlayAreaBlock.Dir[]> checkOrders = new List<PlayAreaBlock.Dir[]>();
		checkOrders.Add( new PlayAreaBlock.Dir[]{ PlayAreaBlock.Dir.Right });
		checkOrders.Add( new PlayAreaBlock.Dir[]{ PlayAreaBlock.Dir.Left });
		checkOrders.Add( new PlayAreaBlock.Dir[]{ PlayAreaBlock.Dir.Down });
		checkOrders.Add( new PlayAreaBlock.Dir[]{ PlayAreaBlock.Dir.Up });
		checkOrders.Add( new PlayAreaBlock.Dir[]{ PlayAreaBlock.Dir.Down, PlayAreaBlock.Dir.Right });
		checkOrders.Add( new PlayAreaBlock.Dir[]{ PlayAreaBlock.Dir.Down, PlayAreaBlock.Dir.Left });
		checkOrders.Add( new PlayAreaBlock.Dir[]{ PlayAreaBlock.Dir.Up, PlayAreaBlock.Dir.Right });
		checkOrders.Add( new PlayAreaBlock.Dir[]{ PlayAreaBlock.Dir.Up, PlayAreaBlock.Dir.Left });
		checkOrders.Add( new PlayAreaBlock.Dir[]{ PlayAreaBlock.Dir.Right, PlayAreaBlock.Dir.Right });
		checkOrders.Add( new PlayAreaBlock.Dir[]{ PlayAreaBlock.Dir.Left, PlayAreaBlock.Dir.Left });
		checkOrders.Add( new PlayAreaBlock.Dir[]{ PlayAreaBlock.Dir.Down, PlayAreaBlock.Dir.Down });
		checkOrders.Add( new PlayAreaBlock.Dir[]{ PlayAreaBlock.Dir.Up, PlayAreaBlock.Dir.Up });

		foreach (PlayAreaBlock.Dir[] order in checkOrders)
		{
			Vector2i move = new Vector2i();
			for (int i = 0; i < order.Length; i++)
			{
				switch (order[i])
				{
					case PlayAreaBlock.Dir.Up:
						move.y = Mathf.Max(move.y - 1, -upCheckCount);
						break;
					case PlayAreaBlock.Dir.Down:
						move.y = Mathf.Min(move.y + 1, downCheckCount);
						break;
					case PlayAreaBlock.Dir.Left:
						move.x = Mathf.Max(move.x - 1, -leftCheckCount);
						break;
					case PlayAreaBlock.Dir.Right:
						move.x = Mathf.Min(move.x + 1, rightCheckCount);
						break;
				}
			}
			m_Tetromino.GridPosition += move;

			if (UpdateTetromino())
			{
				// 確定猶予
				ResetTetrominoAttachWait();

				// 成功
				CurrentLine = m_Tetromino.GridPosition.y;
				return;
			}
			else
			{
				m_Tetromino.GridPosition -= move;
			}
		}

		// 失敗
		m_Tetromino.Rotate(!negative);
	}

	/// <summary>
	/// テトロミノ内の座標か調べる
	/// </summary>
	public bool IsHitTetromino(Vector3 worldPosition, bool updateDistance)
	{
		Vector2i touch = ConvertWorldToGridPosition(worldPosition);
		
		for (int y = 0; y < m_Tetromino.ShapeSize; y++)
		{
			for (int x = 0; x < m_Tetromino.ShapeSize; x++)
			{
				if (m_Tetromino.GetShapeGrid(x, y) && touch == m_Tetromino.GridPosition - m_Tetromino.GetAxisGrid() + new Vector2i(x, y))
				{
					return true;
				}
			}
		}

		return false;
	}

	/// <summary>
	/// テトロミノのワールド座標を取得
	/// </summary>
	public Vector3 GetTetrominoPosition()
	{
		return ConvertGridToWorldPosition(m_Tetromino.GridPosition);
	}

	/// <summary>
	/// テトロミノが空中にある?
	/// </summary>
	public bool IsTetrominoFlying()
	{
		foreach (TRPanel panel in m_TetrominoParts)
		{
			TRPanelBlock block = panel.Block.GetLink(PlayAreaBlock.Dir.Down) as TRPanelBlock;
			if (block == null || block.AttachedPanel != null)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// ラインを下げる
	/// </summary>
	public void DownLine()
	{
		m_MoveDownWaitTime -= Time.deltaTime;
		if (m_MoveDownWaitTime < 0)
		{
			ResetDownLineWait();
			++CurrentLine;

			Vector2i prev = m_Tetromino.GridPosition;
			MoveTetromino(m_Tetromino.GridPosition);
		}
	}

	/// <summary>
	/// ライン下げ待機時間をリセット
	/// </summary>
	public void ResetDownLineWait()
	{
		m_MoveDownWaitTime = m_MoveDownInterval;
	}

	#endregion

	#region Panel

	/// <summary>
	/// パネル消滅
	/// </summary>
	public void VanishPanel()
	{
		foreach (TRPanelBlock[] blocks in m_Lines)
		{
			if (blocks[0].GetPanelCountDir(PlayAreaBlock.Dir.Right) == Width)
			{
				foreach (TRPanelBlock block in blocks)
				{
					block.AttachedPanel.BeginVanish();
				}
			}
		}
	}

	/// <summary>
	/// パネル落下
	/// </summary>
	public void FallPanel()
	{
		int fallCount = 0;
		for (int i = m_Lines.Count - 1; i >= 0; i--)
		{
			// ラインが全て空
			if (m_Lines[i][0].GetEmptyBlockCountDir(PlayAreaBlock.Dir.Right) == Width)
			{
				++fallCount;
				continue;
			}
			// パネルがあるので落下
			else if (fallCount > 0)
			{
				foreach (TRPanelBlock block in m_Lines[i])
				{
					if (block.AttachedPanel != null)
					{
						block.AttachedPanel.BeginFall(block.GetLink(PlayAreaBlock.Dir.Down, fallCount) as TRPanelBlock);
					}
				}
			}
		}
	}

	/// <summary>
	/// パネル除外
	/// </summary>
	public void RemovePanel(TRPanel panel)
	{
		if (!m_UnusedPanels.Contains(panel))
		{
			m_UnusedPanels.Add(panel);
		}
	}

	#endregion

	/// <summary>
	/// ワールド座標をグリッド座標に変換
	/// </summary>
	public Vector2i ConvertWorldToGridPosition(Vector3 worldPosition)
	{
		// エリア座標に変換
		Vector3 localPos = worldPosition - (m_Lines[0][0].Position - new Vector3(TRPanelBlock.HalfSize, -TRPanelBlock.HalfSize, 0));
		return new Vector2i((int)(localPos.x / TRPanelBlock.Size), (int)(localPos.y / TRPanelBlock.Size) * -1);
	}

	/// <summary>
	/// グリッド座標をワールド座標に変換
	/// </summary>
	public Vector3 ConvertGridToWorldPosition(Vector2i gridPosition)
	{
		Vector3 worldPos = m_Lines[0][0].Position;
		worldPos.x += gridPosition.x * TRPanelBlock.Size;
		worldPos.y -= gridPosition.y * TRPanelBlock.Size;
		return worldPos;
	}

	private void OnGUI()
	{
		GUIStyle style = new GUIStyle();
		GUIStyleState styleState = new GUIStyleState();
		styleState.textColor = Color.white;
		style.fontSize = 50;
		style.normal = styleState;

		string next = "";
		if (m_NextTetrominoTypes.Count > 0)
		{
			switch (m_NextTetrominoTypes[0])
			{
				case 0: next = "S"; break;
				case 1: next = "Z"; break;
				case 2: next = "T"; break;
				case 3: next = "L"; break;
				case 4: next = "J"; break;
				case 5: next = "I"; break;
				case 6: next = "O"; break;
			}
		}
		GUI.Label(new Rect(0, 0, Screen.width, Screen.height), next, style);

		/*
		for (int y = 0; y < m_Lines.Count; y++)
		{
			for (int x = 0; x < m_Lines[y].Length; x++)
			{
				Vector3 pos = m_Lines[y][x].Position;
				pos.y = -pos.y;
				pos = Camera.main.WorldToScreenPoint(pos);
				GUI.Label(new Rect(pos.x, pos.y, 100, 100), m_Lines[y][x].AttachedPanel != null ? "full" : "empty");
			}
		}
		*/
	}
}
