using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TRTetromino
{
	///// <summary> 回転軸 </summary>
	private Vector2 m_RotaAxis = Vector2.zero;
	
	///// <summary> グリッド座標 </summary>
	public Vector2i GridPosition { get; set; }

	/// <summary> カラー </summary>
	public Color PanelColor { get; set; }
	
	/// <summary> 形状の大きさ </summary>
	public int ShapeSize { get; set; }

	/// <summary> 形状 </summary>
	private bool[] m_Shape = null;
	
	/// <summary> タイプ </summary>
	private int m_Type = 0;

	/// <summary>
	/// 初期化
	/// </summary>
	public void Initialize(int type)
	{
		m_Type = type;
		switch (m_Type)
		{
			// S
			case 0:
				PanelColor = Color.green;
				ShapeSize = 3;
				m_RotaAxis.Set(1f, 1f);

				m_Shape = new bool[ShapeSize * ShapeSize];
				SetShapeGrid(1, 0, true);
				SetShapeGrid(2, 0, true);
				SetShapeGrid(0, 1, true);
				SetShapeGrid(1, 1, true);
				break;

			// Z
			case 1:
				PanelColor = Color.red;
				ShapeSize = 3;
				m_RotaAxis.Set(1f, 1f);

				m_Shape = new bool[ShapeSize * ShapeSize];
				SetShapeGrid(0, 0, true);
				SetShapeGrid(1, 0, true);
				SetShapeGrid(1, 1, true);
				SetShapeGrid(2, 1, true);
				break;

			// T
			case 2:
				PanelColor = Color.magenta;
				ShapeSize = 3;
				m_RotaAxis.Set(1f, 1f);

				m_Shape = new bool[ShapeSize * ShapeSize];
				SetShapeGrid(1, 0, true);
				SetShapeGrid(0, 1, true);
				SetShapeGrid(1, 1, true);
				SetShapeGrid(2, 1, true);
				break;

			// L
			case 3:
				PanelColor = new Color(1f, 0.5f, 0f);
				ShapeSize = 3;
				m_RotaAxis.Set(1f, 1f);

				m_Shape = new bool[ShapeSize * ShapeSize];
				SetShapeGrid(0, 0, true);
				SetShapeGrid(0, 1, true);
				SetShapeGrid(1, 1, true);
				SetShapeGrid(2, 1, true);
				break;

			// J
			case 4:
				PanelColor = Color.blue;
				ShapeSize = 3;
				m_RotaAxis.Set(1f, 1f);

				m_Shape = new bool[ShapeSize * ShapeSize];
				SetShapeGrid(0, 1, true);
				SetShapeGrid(1, 1, true);
				SetShapeGrid(2, 1, true);
				SetShapeGrid(2, 0, true);
				break;

			// I
			case 5:
				PanelColor = Color.cyan;
				ShapeSize = 4;
				m_RotaAxis.Set(1.5f, 1.5f);

				m_Shape = new bool[ShapeSize * ShapeSize];
				SetShapeGrid(0, 2, true);
				SetShapeGrid(1, 2, true);
				SetShapeGrid(2, 2, true);
				SetShapeGrid(3, 2, true);
				break;

			// O
			case 6:
				PanelColor = Color.yellow;
				ShapeSize = 2;
				m_RotaAxis.Set(0.5f, 0.5f);

				m_Shape = new bool[ShapeSize * ShapeSize];
				SetShapeGrid(0, 0, true);
				SetShapeGrid(1, 0, true);
				SetShapeGrid(0, 1, true);
				SetShapeGrid(1, 1, true);
				break;
		}
	}

	/// <summary>
	/// 回転
	/// </summary>
	public void Rotate(bool negative)
	{
		bool[] newShape = new bool[ShapeSize * ShapeSize];
		for (int i = 0; i < ShapeSize * ShapeSize; i++)
		{
			if (m_Shape[i])
			{
				float disX = (i % ShapeSize) - m_RotaAxis.x;
				float disY = (i / ShapeSize) - m_RotaAxis.y;
				int x = (int)(m_RotaAxis.x + disY * (negative ? 1 : -1));
				int y = (int)(m_RotaAxis.y + disX * (negative ? -1 : 1));

				newShape[y * ShapeSize + x] = true;
			}
		}
		m_Shape = newShape;
	}

	/// <summary>
	/// 形状を取得
	/// </summary>
	public bool GetShapeGrid(int x, int y)
	{
		return m_Shape[y * ShapeSize + x];
	}

	/// <summary>
	/// 形状を登録
	/// </summary>
	private void SetShapeGrid(int x, int y, bool value)
	{
		m_Shape[y * ShapeSize + x] = value;
	}

	/// <summary>
	/// 回転軸を取得
	/// </summary>
	public Vector2i GetAxisGrid()
	{
		return new Vector2i((int)m_RotaAxis.x, (int)m_RotaAxis.y);
	}

	/// <summary>
	/// 中央のグリッド座標を取得
	/// </summary>
	public Vector2i GetCenterGrid()
	{
		Vector2i min, max;
		GetShapeBox(out min, out max);
		return new Vector2i(min.x + (int)Mathf.Ceil((max.x - min.x) / 2f), min.y + (int)Mathf.Ceil((max.y - min.y) / 2f));
	}

	/// <summary>
	/// 形状範囲ボックス取得
	/// </summary>
	public void GetShapeBox(out Vector2i min, out Vector2i max)
	{
		min = new Vector2i(ShapeSize - 1, ShapeSize - 1);
		max = new Vector2i();
		for (int i = 0; i < ShapeSize * ShapeSize; i++)
		{
			if (m_Shape[i])
			{
				min.x = Mathf.Min(i % ShapeSize, min.x);
				max.x = Mathf.Max(i % ShapeSize, max.x);

				min.y = Mathf.Min(i / ShapeSize, min.y);
				max.y = Mathf.Max(i / ShapeSize, max.y);
			}
		}
	}

	/// <summary>
	/// タイプ数
	/// </summary>
	public int NumType
	{
		get { return 7; }
	}
}
