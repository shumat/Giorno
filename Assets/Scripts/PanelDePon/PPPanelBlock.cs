using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPPanelBlock : PlayAreaBlock
{
	/// <summary> アタッチされたパネル </summary>
	private PPPanel m_Panel = null;

	/// <summary> 消滅リクエストされた？ </summary>
	public bool VanishRequested { get; set; }

	/// <summary>
	/// パネルをアタッチ
	/// </summary>
	public void Attach(PPPanel panel, bool updatePanelPosition = false)
	{
		m_Panel = panel;
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
	/// パネルをデタッチ
	/// </summary>
	public void Detach()
	{
		if (m_Panel != null)
		{
			m_Panel.Block = null;
			m_Panel = null;
		}
	}

	/// <summary>
	/// アタッチされたパネル
	/// </summary>
	public PPPanel AttachedPanel
	{
		get { return m_Panel; }
	}

	/// <summary>
	/// タイプが一致するパネルがアタッチされた連続するブロックを取得
	/// </summary>
	private void GetMatchPanelBlocks(ref List<PPPanelBlock> match, PPPanel.Type type, Dir dir, bool first = true)
	{
		if (m_Panel == null || m_Panel.IsLocked() || m_Panel.PanelType != type || m_Panel.IsDisturbance)
		{
			return;
		}

		if (first)
		{
			match = new List<PPPanelBlock>();

			// 水平方向
			if (GetLink(Dir.Left) != null)
				(GetLink(Dir.Left) as PPPanelBlock).GetMatchPanelBlocks(ref match, type, Dir.Left, false);
			if (GetLink(Dir.Right) != null)
				(GetLink(Dir.Right) as PPPanelBlock).GetMatchPanelBlocks(ref match, type, Dir.Right, false);

			// 消滅に必要な数に満たない
			if (match.Count + 1 < PPGame.Config.MinVanishMatchCount)
			{
				match.Clear();
			}

			int horizCount = match.Count;

			// 垂直方向
			if (GetLink(Dir.Up) != null)
				(GetLink(Dir.Up) as PPPanelBlock).GetMatchPanelBlocks(ref match, type, Dir.Up, false);
			if (GetLink(Dir.Down) != null)
				(GetLink(Dir.Down) as PPPanelBlock).GetMatchPanelBlocks(ref match, type, Dir.Down, false);
			// 消滅に必要な数に満たない
			if (match.Count - horizCount + 1 < PPGame.Config.MinVanishMatchCount)
			{
				match.RemoveRange(horizCount, match.Count - horizCount);
			}

			// 自身を追加
			if (match.Count > 0)
			{
				match.Add(this);
			}
		}
		else
		{
			if (match != null)
			{
				match.Add(this);
			}
			if (GetLink(dir) != null)
			{
				(GetLink(dir) as PPPanelBlock).GetMatchPanelBlocks(ref match, type, dir, false);
			}
		}
	}

	/// <summary>
	/// タイプが一致するパネルがアタッチされた連続するブロックを取得
	/// </summary>
	public bool GetMatchPanelBlocks(out List<PPPanelBlock> match)
	{
		match = null;
		if (m_Panel != null)
		{
			GetMatchPanelBlocks(ref match, m_Panel.PanelType, 0);
			return match != null && match.Count > 0;
		}
		return false;
	}

	/// <summary>
	/// 指定方向に連続した同タイプのパネル数を取得
	/// </summary>
	public int GetMatchPanelCountDir(Dir dir)
	{
		int count = 0;
		if (AttachedPanel != null)
		{
			count = 1;
			PPPanel.Type type = AttachedPanel.PanelType;
			PPPanelBlock block = this;
			while ((block = block.GetLink(dir) as PPPanelBlock) != null && block.AttachedPanel != null && block.AttachedPanel.PanelType == type)
			{
				++count;
			}
		}
		return count;
	}

	/// <summary>
	/// パネル消滅
	/// </summary>
	public void VanishPanel(float delay, float endTime)
	{
		if (m_Panel != null)
		{
			m_Panel.BeginVanish(delay, endTime);
			VanishRequested = false;
		}
	}

	/// <summary>
	/// パネル座標更新
	/// </summary>
	public void UpdatePanelPosition()
	{
		if (m_Panel != null)
		{
			m_Panel.transform.position = Position;
		}
	}

	/// <summary>
	/// パネル落下時の通過ブロックを取得
	/// </summary>
	public bool GetFallPassBlock(ref List<PPPanelBlock> blocks)
	{
		// 真下にブロックがある
		PPPanelBlock under = GetLink(Dir.Down) as PPPanelBlock;
		if (under != null)
		{
			// 真下が空なら更に下へ
			if (under.AttachedPanel == null)
			{
				blocks.Add(under);
				return (GetLink(Dir.Down) as PPPanelBlock).GetFallPassBlock(ref blocks);
			}
			// 真下のパネルがロック中
			else if (under.AttachedPanel.IsLocked())
			{
				return false; // 落下不可
			}
			// 真下にパネルがある
			else
			{
				return blocks.Count > 0;
			}
		}
		// 真下にブロックがない
		else
		{
			return blocks.Count > 0;
		}
	}

	/// <summary>
	/// 最下段の空ブロックを取得
	/// </summary>
	public PPPanelBlock GetMostUnderEmptyBlock(ref int depth)
	{
		// 真下にブロックがある
		PPPanelBlock under = GetLink(Dir.Down) as PPPanelBlock;
		if (under != null)
		{
			// 真下が空なら更に下へ
			if (under.AttachedPanel == null)
			{
				++depth;
				return (GetLink(Dir.Down) as PPPanelBlock).GetMostUnderEmptyBlock(ref depth);
			}
//			// 真下のパネルがロック中
//			else if (under.AttachedPanel.IsLoced())
//			{
//				return null;
//			}
			// 真下にパネルがある
			else
			{
				return this;
			}
		}
		// 真下にブロックがない
		else
		{
			return this;
		}
	}

	/// <summary>
	/// 最下段の空ブロックを取得
	/// </summary>
	public PPPanelBlock GetMostUnderEmptyBlock()
	{
		int depth = 0;
		return GetMostUnderEmptyBlock(ref depth);
	}

	/// <summary>
	/// 上のパネルが落下待機中?
	/// </summary>
	public bool GetIsUpperBlockFallWait()
	{
		PPPanelBlock upper = GetLink(Dir.Up) as PPPanelBlock;
		return upper != null && upper.AttachedPanel != null && upper.AttachedPanel.CurrentState == PPPanel.State.FallReady;
	}

	/// <summary>
	/// ロック中？
	/// </summary>
	public bool IsLoced()
	{
		return m_Panel != null && m_Panel.IsLocked();
	}
}
