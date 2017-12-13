using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TRPanel : MonoBehaviour
{
	/// <summary> 所属ブロック </summary>
	private TRPanelBlock m_Block = null;

	/// <summary>
	/// 所属ブロック
	/// </summary>
	public TRPanelBlock Block
	{
		get { return m_Block; }
		set { m_Block = value; }
	}
	
	/// <summary> プレイエリア </summary>
	private TRPlayArea m_PlayArea = null;

	/// <summary> スプライトレンダラ </summary>
	private SpriteRenderer m_SpriteRenderer = null;

	/// <summary>
	/// 生成
	/// </summary>
	protected void Awake()
	{
		m_SpriteRenderer = GetComponent<SpriteRenderer>();
	}

	/// <summary>
	/// 初期化
	/// </summary>
	public void Initialize(TRPlayArea playArea, Color color)
	{
		m_PlayArea = playArea;
		m_SpriteRenderer.color = color;
		gameObject.SetActive(true);
	}

	/// <summary>
	/// デアクティベート
	/// </summary>
	public void Deactivate()
	{
		gameObject.SetActive(false);
	}

	/// <summary>
	/// 消滅開始
	/// </summary>
	public void BeginVanish()
	{
		Block.Detach();
		m_PlayArea.RemovePanel(this);
		Deactivate();
	}

	/// <summary>
	/// 落下開始
	/// </summary>
	public void BeginFall(TRPanelBlock target)
	{
		Block.Detach();
		target.Attach(this, true);
	}
}
