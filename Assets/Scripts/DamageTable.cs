using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Giorno/DamageTable")]
public class DamageTable : ScriptableObject
{
	/// <summary>
	/// ダメージデータ
	/// </summary>
	[System.Serializable]
	public class DamageData
	{
		public int thresholdValue = -1;
		public float MDLine = 0f;
		public float TRLine = 0f;
		public List<PPDisturbData> PPDisturb = new List<PPDisturbData>();
	}

	/// <summary>
	/// パネポン妨害パネルデータ
	/// </summary>
	[System.Serializable]
	public class PPDisturbData
	{
		public int width = 1;
		public int  height = 1;
		public int count = 1;
	}

	/// <summary> データ </summary>
	[SerializeField]
	private List<DamageData> m_Table = new List<DamageData>();
	
	/// <summary> データ取得 </summary>
	public List<DamageData> Data
	{
		get { return m_Table; }
	}
}
