using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Giorno/Panel de PON Config")]
public class PPConfig : ScriptableObject
{
	[Header("Game")]
	
	/// <summary> 消滅するための最低連結数 </summary>
	[SerializeField]
	private int m_MinVanishMatchCount = 3;
	
	/// <summary> 消滅するための最低連結数 </summary>
	public int MinVanishMatchCount
	{
		get { return m_MinVanishMatchCount; }
	}

	/// <summary> 自動せり上げ量</summary>
	[SerializeField]
	private float m_AutoElevateValue = 0.05f;
	
	/// <summary> 自動せり上げ量 </summary>
	public float AutoElevateValue
	{
		get { return m_AutoElevateValue; }
	}

	/// <summary> せり上げ実行間隔 </summary>
	[SerializeField]
	private List<float> m_AutoElevateInterval = new List<float>(){ 1f };

	/// <summary> せり上げ実行間隔 </summary>
	public float GetAutoElevateInterval(int level)
	{
		return m_AutoElevateInterval[Mathf.Min(level, m_AutoElevateInterval.Count - 1)];
	}

	/// <summary> 基礎せり上げ停止時間 </summary>
	[SerializeField]
	private float m_ElevateStopTimeBase = 1f;
	
	/// <summary> 基礎せり上げ停止時間 </summary>
	public float GetElevateStopTime(int chainCount)
	{
		return m_ElevateStopTimeBase * chainCount;
	}
	
	[Header("Player")]

	/// <summary> 手動せり上げ実行間隔 </summary>
	[SerializeField]
	private float m_ManualElevateInterval = 0.01f;

	/// <summary> 手動せり上げ実行間隔 </summary>
	public float ManualElevateInterval
	{
		get { return m_ManualElevateInterval; }
	}

	/// <summary> 手動せり上げ量 </summary>
	[SerializeField]
	private float m_ManualElevateValue = 0.08f;

	/// <summary> 手動せり上げ量 </summary>
	public float ManualElevateSpeed
	{
		get { return m_ManualElevateValue; }
	}

	/// <summary> 手動せり上げを開始する最低タッチ時間 </summary>
	[SerializeField]
	private float m_ElevateStartTouchTime = 0.3f;
	
	/// <summary> 手動せり上げを開始する最低タッチ時間 </summary>
	public float ElevateStartTouchTime
	{
		get { return m_ElevateStartTouchTime; }
	}
	
	/// <summary> パネル入れ替えを開始する最低スワイプ距離 </summary>
	[SerializeField]
	private float m_PanelSwapStartSwipeDistance = 0.3f;
	
	/// <summary> パネル入れ替えを開始する最低スワイプ距離 </summary>
	public float PanelSwapStartSwipeDistance
	{
		get { return m_PanelSwapStartSwipeDistance; }
	}
	
	/// <summary> パネル入れ替え方向を決定する閾値 </summary>
	[SerializeField]
	private float m_PanelSwapDirThresholdScale = 0.8f;

	/// <summary> パネル入れ替え方向を決定する閾値 </summary>
	public float PanelSwapDirThreshouldScale
	{
		get { return m_PanelSwapDirThresholdScale; }
	}
	
	/// <summary> パネル入れ替えを開始するタッチ座標と対象パネルとの距離 </summary>
	[SerializeField]
	private float m_PanelSwapStartDistance = 0.4f;

	/// <summary>
	/// パネル入れ替えを開始するタッチ座標と対象パネルとの距離
	/// </summary>
	public float PanelSwapStartDistance
	{
		get { return m_PanelSwapStartDistance; }
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
	private int m_PlayAreaWidth = 6;
	
	/// <summary> プレイエリア最大列数 </summary>
	public int PlayAreaWidth
	{
		get { return m_PlayAreaWidth; }
	}

	[Header("Panel")]
	
	/// <summary> パネルサイズ </summary>
	[SerializeField]
	private float m_PanelSize = 1f;
	
	/// <summary> パネルサイズ </summary>
	public float PanelSize
	{
		get { return m_PanelSize; }
	}
	
	/// <summary> パネル落下待機時間 </summary>
	[SerializeField]
	private float m_PanelFallWaitTime = 0.25f;
	
	/// <summary> パネル落下待機時間 </summary>
	public float PanelFallWaitTime
	{
		get { return m_PanelFallWaitTime; }
	}
	
	/// <summary> パネル落下スピード </summary>
	[SerializeField]
	private float m_PanelFallSpeed = 20f;
	
	/// <summary> パネル落下スピード </summary>
	public float PanelFallSpeed
	{
		get { return m_PanelFallSpeed; }
	}
	
	/// <summary> パネル入れ替えスピード </summary>
	[SerializeField]
	private float m_PanelSwapSpeed = 30f;
	
	/// <summary> パネル入れ替えスピード </summary>
	public float PanelSwapSpeed
	{
		get { return m_PanelSwapSpeed; }
	}
	
	/// <summary> パネル消滅ディレイ </summary>
	[SerializeField]
	private float m_PanelVanishDelay = 0.2f;
	
	/// <summary> パネル消滅ディレイ </summary>
	public float PanelVanishDelay
	{
		get { return m_PanelVanishDelay; }
	}

	/// <summary> 妨害パネル分解ディレイ </summary>
	[SerializeField]
	private float m_PanelDissolveDelay = 0.2f;

	/// <summary> 妨害パネル分解ディレイ </summary>
	public float PanelDissolveDelay
	{
		get { return m_PanelDissolveDelay; }
	}

	[Header("Damage")]

	/// <summary> 連鎖ダメージテーブル </summary>
	[SerializeField]
	private DamageTable m_ChainDamageTable = null;

	/// <summary>
	/// 連鎖ダメージデータ取得
	/// </summary>
	public DamageTable.DamageData GetChainDamageData(byte damageValue)
	{
		int index = -1;
		for (int i = m_ChainDamageTable.Data.Count - 1; i >= 0; i--)
		{
			if (damageValue >= m_ChainDamageTable.Data[i].thresholdValue)
			{
				index = i;
				break;
			}
		}
		return index >= 0 ? m_ChainDamageTable.Data[index] : null;
	}

	/// <summary> 消滅ダメージテーブル </summary>
	[SerializeField]
	private DamageTable m_VanishDamageTable = null;

	/// <summary>
	/// 消滅ダメージデータ取得
	/// </summary>
	public DamageTable.DamageData GetVanishDamageData(byte damageValue)
	{
		int index = -1;
		for (int i = m_VanishDamageTable.Data.Count - 1; i >= 0; i--)
		{
			if (damageValue >= m_VanishDamageTable.Data[i].thresholdValue)
			{
				index = i;
				break;
			}
		}
		return index >= 0 ? m_VanishDamageTable.Data[index] : null;
	}
}
