using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TRPanelBlock : PlayAreaBlock
{
	/// <summary> アタッチされたパネル </summary>
	private TRPanel m_Panel = null;

	/// <summary> 予約扱い </summary>
	private bool m_Reserve = false;

	/// <summary>
	/// アタッチ
	/// </summary>
	public void Attach(TRPanel panel, bool updatePanelPosition = false)
	{
		m_Panel = panel;
		m_Reserve = false;
		if (m_Panel != null)
		{
			m_Panel.Block = this;

			if (updatePanelPosition)
			{
				m_Panel.transform.position = Position;
			}
		}
	}

	/// <summary>
	/// デタッチ
	/// </summary>
	public void Detach()
	{
		m_Reserve = false;
		if (m_Panel != null)
		{
			m_Panel.Block = null;
			m_Panel = null;
		}
	}

	/// <summary>
	/// アタッチ予約
	/// </summary>
	public void Reserve(TRPanel panel, bool updatePanelPosition = false)
	{
		Attach(panel, updatePanelPosition);
		m_Reserve = true;
	}

	/// <summary>
	/// 予約パネルをアタッチ
	/// </summary>
	public bool ApplyhReservedDrop()
	{
		if (m_Reserve)
		{
			m_Reserve = false;
			return m_Panel != null;
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
	/// アタッチされたパネル
	/// </summary>
	public TRPanel AttachedPanel
	{
		get { return m_Reserve ? null : m_Panel; }
	}

	/// <summary>
	/// 予約されたパネル
	/// </summary>
	public TRPanel ReservedPanel
	{
		get { return m_Reserve ? m_Panel : null; }
	}

	/// <summary>
	/// アタッチ or 予約されている?
	/// </summary>
	public bool IsAttachOrReserved
	{
		get { return m_Panel != null; }
	}

	/// <summary>
	/// 指定方向に連続したパネル数を取得
	/// </summary>
	public int GetPanelCountDir(Dir dir)
	{
		int count = 0;
		if (AttachedPanel != null)
		{
			count = 1;
			TRPanelBlock block = this;
			while ((block = block.GetLink(dir) as TRPanelBlock) != null && block.AttachedPanel != null)
			{
				++count;
			}
		}
		return count;
	}

	/// <summary>
	/// 指定方向に連続した空ブロック数を取得
	/// </summary>
	public int GetEmptyBlockCountDir(Dir dir)
	{
		int count = 0;
		if (AttachedPanel == null)
		{
			count = 1;
			TRPanelBlock block = this;
			while ((block = block.GetLink(dir) as TRPanelBlock) != null && block.AttachedPanel == null)
			{
				++count;
			}
		}
		return count;
	}
}
