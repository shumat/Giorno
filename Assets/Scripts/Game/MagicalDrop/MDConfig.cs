using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Giorno/MagicalDrop Config")]
public class MDConfig : ScriptableObject
{
	[Header("Game")]

	/// <summary> 初期ドロップ行数 </summary>
	[SerializeField]
	public int m_FirstDropLine = 4;

	/// <summary> 初期ドロップ行数 </summary>
	public int FirstDropLine
	{
		get { return m_FirstDropLine; }
	}

	/// <summary> 消滅するための最低連結数 </summary>
	[SerializeField]
	private int m_MinVanishMatchCount = 3;

	/// <summary> 消滅するための最低連結数 </summary>
	public int MinVanishMatchCount
	{
		get { return m_MinVanishMatchCount; }
	}

	/// <summary> 最大連鎖受付時間 </summary>
	[SerializeField]
	private float m_MaxChainReceiveTime = 1f;

	/// <summary> 最大連鎖受付時間 </summary>
	public float MaxChainReceiveTime
	{
		get { return m_MaxChainReceiveTime; }
	}

	/// <summary> ライン自動生成開始時間 </summary>
	[SerializeField]
	private float m_AutoLineCreateStartTime = 8f;

	/// <summary> ライン自動生成開始時間 </summary>
	public float AutoLineCreateStartTime
	{
		get { return m_AutoLineCreateStartTime; }
	}

	/// <summary> ライン自動生成間隔 </summary>
	[SerializeField]
	private List<float> m_AutoLineCreateIntervals = new List<float>(){ 8f };

	/// <summary> ライン自動生成間隔 </summary>
	public float GetLAutoLineCreateInterval(int level)
	{
		return m_AutoLineCreateIntervals[Mathf.Min(level, m_AutoLineCreateIntervals.Count - 1)];
	}

	/// <summary> 1度に自動生成するライン数 </summary>
	[SerializeField]
	public List<int> m_AutoLineCreateCounts = new List<int>(){ 3 };

	/// <summary> 1度に自動生成するライン数 </summary>
	public int GetAutoLineCreateCount(int level)
	{
		return m_AutoLineCreateCounts[Mathf.Min(level, m_AutoLineCreateCounts.Count - 1)];
	}

	[Header("Player")]

	/// <summary> 行動を開始するスワップ距離 </summary>
	[SerializeField]
	private float m_ActionStartSwipeDistance = 0.2f;

	/// <summary> 行動を開始するスワップ距離 </summary>
	public float ActionStartSwipeDistance
	{
		get { return m_ActionStartSwipeDistance; }
	}

	/// <summary> ライン生成を開始する最低タッチ時間 </summary>
	[SerializeField]
	private float m_LineCreateStartTouchTime = 0.5f;

	/// <summary> ライン生成を開始する最低タッチ時間 </summary>
	public float LineCreateStartTouchTime
	{
		get { return m_LineCreateStartTouchTime; }
	}

	/// <summary> ライン生成を続ける最低タッチ時間 </summary>
	[SerializeField]
	private float m_LineCreateContinueTouchTime = 0.3f;

	/// <summary> ライン生成を続ける最低タッチ時間 </summary>
	public float LineCreateContinueTouchTime
	{
		get { return m_LineCreateContinueTouchTime; }
	}

	[Header("PlayArea")]

	/// <summary> プレイエリア最大行数 </summary>
	[SerializeField]
	private int m_PlayAreaHeight = 12;

	/// <summary> プレイエリア最大行数 </summary>
	public int PlayAreaHeight
	{
		get { return m_PlayAreaHeight; }
	}

	/// <summary> プレイエリア最大列数 </summary>
	[SerializeField]
	private int m_PlayAreaWidth = 7;

	/// <summary> プレイエリア最大列数 </summary>
	public int PlayAreaWidth
	{
		get { return m_PlayAreaWidth; }
	}

	/// <summary> プレイエリアスクロールスピード </summary>
	[SerializeField]
	public float m_PlayAreaScrollSpeed = 5f;

	/// <summary> プレイエリアスクロールスピード </summary>
	public float PlayAreaScrollSpeed
	{
		get { return m_PlayAreaScrollSpeed; }
	}

	[Header("Drop")]

	/// <summary> ドロップサイズ </summary>
	[SerializeField]
	private float m_DropSize = 1f;

	/// <summary> ドロップサイズ </summary>
	public float DropSize
	{
		get { return m_DropSize; }
	}

	/// <summary> ドロッププルスピード </summary>
	[SerializeField]
	private float m_DropPullSpeed = 80f;

	/// <summary> ドロッププルスピード </summary>
	public float DropPullSpeed
	{
		get { return m_DropPullSpeed; }
	}

	/// <summary> ドロッププル時の最大方向転換角度 </summary>
	[SerializeField]
	private float m_DropPullMaxAngle = 45f;

	/// <summary> ドロッププル時の最大方向転換角度 </summary>
	public float DropPullMaxAngle
	{
		get { return m_DropPullMaxAngle; }
	}

	/// <summary> ドロッププッシュスピード </summary>
	[SerializeField]
	private float m_DropPushSpeed = 60f;

	/// <summary> ドロッププッシュスピード </summary>
	public float DropPushSpeed
	{
		get { return m_DropPushSpeed; }
	}

	[Header("Damage")]

	/// <summary> ダメージテーブル </summary>
	[SerializeField]
	private DamageTable m_DamageTable = null;

	/// <summary>
	/// ダメージデータ取得
	/// </summary>
	public DamageTable.DamageData GetDamageData(byte damageValue)
	{
		int index = -1;
		for (int i = m_DamageTable.Data.Count - 1; i >= 0; i--)
		{
			if (damageValue >= m_DamageTable.Data[i].thresholdValue)
			{
				index = i;
				break;
			}
		}
		return index >= 0 ? m_DamageTable.Data[index] : null;
	}
}
