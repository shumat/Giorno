using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAreaBlock
{
	/// <summary>
	/// 隣接方向
	/// </summary>
	public enum Dir
	{
		Left,
		Right,
		Up,
		Down,
	}

	/// <summary> ベーストランスフォーム </summary>
	protected Transform m_BaseTransform = null;

	/// <summary> ローカル座標 </summary>
	private Vector3 m_LocalPosition = Vector3.zero;

	/// <summary>
	/// ベーストランスフォーム
	/// </summary>
	public Transform BaseTransform
	{
		get { return m_BaseTransform; }
		set
		{
			m_BaseTransform = value;
			LocalPosition = LocalPosition; // ワールド座標の更新
			
		}
	}

	/// <summary>
	/// ワールド座標
	/// </summary>
	public Vector3 Position
	{
		get { return m_BaseTransform != null ? m_BaseTransform.position + VectorContentMultipul(m_BaseTransform.lossyScale, m_LocalPosition) : m_LocalPosition; }
		set { LocalPosition = value - (m_BaseTransform != null ? m_BaseTransform.position : Vector3.zero); }
	}

	/// <summary>
	/// ローカル座標
	/// </summary>
	public Vector3 LocalPosition
	{
		get { return m_LocalPosition; }
		set { m_LocalPosition = value; }
	}


	/// <summary> 隣接ブロック </summary>
	protected PlayAreaBlock[] m_Links = new PlayAreaBlock[System.Enum.GetNames(typeof(Dir)).Length];

	/// <summary>
	/// 隣接ブロック登録
	/// </summary>
	public void SetLink(PlayAreaBlock block, Dir dir)
	{
		m_Links[(int)dir] = block;
	}

	/// <summary>
	/// 隣接ブロック取得
	/// </summary>
	public PlayAreaBlock GetLink(Dir dir)
	{
		return m_Links[(int)dir];
	}
	
	/// <summary>
	/// 隣接ブロック取得
	/// </summary>
	public PlayAreaBlock GetLink(Dir dir, int distance)
	{
		PlayAreaBlock block = this;
		for (int i = 0; i < distance; i++)
		{
			block = block.GetLink(dir);
			if (block == null)
			{
				break;
			}
		}
		return block;
	}

	/// <summary>
	/// 隣接タイプを反転
	/// </summary>
	public static Dir ConvertReverseDir(Dir dir)
	{
		switch (dir)
		{
		case Dir.Left:
			return Dir.Right;
		case Dir.Right:
			return Dir.Left;
		case Dir.Up:
			return Dir.Down;
		case Dir.Down:
		default:
			return Dir.Up;
		}
	}

	/// <summary>
	/// 各方向にある連続したブロック数取得
	/// </summary>
	public int GetLinkCount(Dir dir)
	{
		int count = 0;
		PlayAreaBlock block = this;
		while ((block = block.GetLink(dir)) != null)
		{
			++count;
		}
		return count;
	}

	/// <summary>
	/// ブロック内に入っているか調べる
	/// </summary>
	public bool TestInsideY(float y, float size, bool addBlockSize = true)
	{
		float halfSize = size * 0.5f;
		float range = addBlockSize ? halfSize : 0;
		return Mathf.Abs(y + range - Position.y) <= halfSize|| Mathf.Abs(y - range - Position.y) <= halfSize;
	}

	/// <summary>
	/// ブロック内に入っているか調べる
	/// </summary>
	public bool TestInsideX(float x, float size, bool addBlockSize = true)
	{
		float halfSize = size * 0.5f;
		float range = addBlockSize ? halfSize : 0;
		return Mathf.Abs(x + range - Position.x) <= halfSize || Mathf.Abs(x - range - Position.x) <= halfSize;
	}

	/// <summary>
	/// ベクトル要素の乗算
	/// </summary>
	private Vector3 VectorContentMultipul(Vector3 a, Vector3 b)
	{
		a.x *= b.x;
		a.y *= b.y;
		a.z *= b.z;
		return a;
	}
}
