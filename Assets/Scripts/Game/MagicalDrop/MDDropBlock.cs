using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MDDropBlock : PlayAreaBlock
{
	/// <summary> アタッチされたドロップ </summary>
	private MDDrop m_Drop = null;

	/// <summary> 予約扱い </summary>
	private bool m_Reserve = false;

	/// <summary>
	/// アタッチ
	/// </summary>
	public void Attach(MDDrop drop, bool updateDropPosition = false)
	{
		m_Drop = drop;
		m_Reserve = false;

		if (m_Drop != null)
		{
			m_Drop.Block = this;

			if (updateDropPosition)
			{
				m_Drop.transform.position = Position;
			}
		}
	}

	/// <summary>
	/// デタッチ
	/// </summary>
	public void Detach()
	{
		if (m_Drop != null)
		{
			m_Drop.Block = null;
			m_Drop = null;
		}
		m_Reserve = false;
	}

	/// <summary>
	/// アタッチ予約
	/// </summary>
	public void Reserve(MDDrop drop)
	{
		m_Drop = drop;
		m_Reserve = true;

		if (m_Drop != null)
		{
			m_Drop.Block = this;
		}
	}

	/// <summary>
	/// 予約ドロップをアタッチ
	/// </summary>
	public bool ApplyhReservedDrop()
	{
		if (m_Reserve)
		{
			m_Reserve = false;
			return m_Drop != null;
		}

		return false;
	}

	/// <summary>
	/// 予約キャンセル
	/// </summary>
	public void CancelReserve()
	{
		Detach();
	}

	/// <summary>
	/// アタッチされたドロップ
	/// </summary>
	public MDDrop AttachedDrop
	{
		get { return m_Reserve ? null : m_Drop; }
	}

	/// <summary>
	/// 予約されたドロップ
	/// </summary>
	public MDDrop ReservedDrop
	{
		get { return m_Reserve ? m_Drop : null; }
	}

	/// <summary>
	/// アタッチ or 予約されている?
	/// </summary>
	public bool IsAttachOrReserved
	{
		get { return m_Drop != null; }
	}

	/// <summary>
	/// 縦方向に繋がった同タイプドロップ数を取得
	/// </summary>
	public void GetVerticallMatchDropCount(out int count, out int validVanishCount, out bool existsPushedDrop, out bool forceVanish)
	{
		count = 0;
		validVanishCount = 0;
		existsPushedDrop = false;
		forceVanish = false;

		if (AttachedDrop == null)
		{
			return;
		}

		PlayAreaBlock block = this;
		MDDrop drop = null;
		Dir dir = Dir.Up;
		for (int i = 0; i < 2; i++)
		{
			while (block != null)
			{
				drop = (block as MDDropBlock).AttachedDrop;
				if (drop != null && drop.DropType == AttachedDrop.DropType && !drop.IsLocked())
				{
					++count;

					if (drop.IsValidVanish)
					{
						++validVanishCount;
					}
					if (drop.IsPushed)
					{
						existsPushedDrop = true;
					}
					if (drop.IsMatchPushed)
					{
						forceVanish = true;
					}

					block = block.GetLink(dir);
				}
				else
				{
					break;
				}
			}

			dir = Dir.Down;
			block = GetLink(dir);
		}
	}

	/// <summary>
	/// 同タイプの連結ブロックを全て取得
	/// </summary>
	public void GetMatchDrops(ref List<MDDrop> match, ref bool isAdjoinFrozen, MDDrop.Type type)
	{
		if (match == null)
		{
			match = new List<MDDrop>();
		}

		if (AttachedDrop != null && match.Contains(AttachedDrop))
		{
			return;
		}

		if (AttachedDrop != null && !AttachedDrop.IsLocked())
		{
			// 氷ドロップあり
			if (AttachedDrop.DropType == MDDrop.Type.Frozen)
			{
				isAdjoinFrozen = true;
			}

			// ドロップタイプが一致
			if (AttachedDrop.DropType == type)
			{
				match.Add(AttachedDrop);
			}
			// 不一致
			else
			{
				return;
			}
		}
		else
		{
			return;
		}

		// 各方向をチェック
		for (int i = 0; i < m_Links.Length; i++)
		{
			if (m_Links[i] != null)
			{
				(m_Links[i] as MDDropBlock).GetMatchDrops(ref match, ref isAdjoinFrozen, type);
			}
		}
	}

	/// <summary>
	/// 最も遠い空ブロックを取得
	/// </summary>
	public MDDropBlock GetFarthestEmptyBlock(Dir dir)
	{
		PlayAreaBlock emptyBlock = null;

		PlayAreaBlock block = GetLink(dir);
		while (block != null && !(block as MDDropBlock).IsAttachOrReserved)
		{
			emptyBlock = block;
			block = block.GetLink(dir);
		}

		return emptyBlock as MDDropBlock;
	}
}
