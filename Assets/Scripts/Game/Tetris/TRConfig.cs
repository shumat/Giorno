using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Giorno/Tetris Config")]
public class TRConfig : ScriptableObject
{
	[Header("Game")]

	[Header("PlayArea")]

	/// <summary> プレイエリア行数 </summary>
	[SerializeField]
	private int m_PlayAreaHeight = 16;

	/// <summary> プレイエリア行数 </summary>
	public int PlayAreaHeight
	{
		get { return m_PlayAreaHeight; }
	}

	/// <summary> プレイエリア列数 </summary>
	[SerializeField]
	private int m_PlayAreaWidth = 8;

	/// <summary> プレイエリア列数 </summary>
	public int PlayAreaWidth
	{
		get { return m_PlayAreaWidth; }
	}

	[Header("TetroMino")]

	/// <summary> グリッドサイズ </summary>
	[SerializeField]
	private float m_GridSize = 1f;

	/// <summary> グリッドサイズ </summary>
	public float GridSize
	{
		get { return m_GridSize; }
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
