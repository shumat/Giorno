using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DamageTable))]
public class DamageTableEditor : Editor
{
	/// <summary> テーブル </summary>
	private	DamageTable m_Table = null;
	
    private void OnEnable()
	{
		m_Table = target as DamageTable;
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		GUILayoutOption labelWidth = GUILayout.Width(150);

		for (int i = 0; i < m_Table.Data.Count; i++)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Level " + (i + 1).ToString(), GUILayout.Width(165));
			// 閾値
			m_Table.Data[i].thresholdValue = EditorGUILayout.IntField(m_Table.Data[i].thresholdValue, GUILayout.Width(80));
			GUILayout.FlexibleSpace();
			// 追加
			if (GUILayout.Button("Insert", GUILayout.Width(50)))
			{
				m_Table.Data.Insert(i, new DamageTable.DamageData());
				break;
			}
			// 削除
			if (GUILayout.Button("Delete", GUILayout.Width(50)))
			{
				m_Table.Data.RemoveAt(i);
				break;
			}
			EditorGUILayout.EndHorizontal();

			++EditorGUI.indentLevel;

			// マジカルドロップ
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("MagicalDrop Line", labelWidth);
			m_Table.Data[i].MDLine = EditorGUILayout.FloatField(m_Table.Data[i].MDLine);
			EditorGUILayout.EndHorizontal();

			// テトリス
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Tetris Line", labelWidth);
			m_Table.Data[i].TRLine = EditorGUILayout.FloatField(m_Table.Data[i].TRLine);
			EditorGUILayout.EndHorizontal();

			// パネポン
			EditorGUILayout.BeginHorizontal();
			// 妨害パネル数
			EditorGUILayout.LabelField("PanePon Disturbance", labelWidth);
			int disturbCount = EditorGUILayout.IntField(m_Table.Data[i].PPDisturb.Count);
			int count = disturbCount - m_Table.Data[i].PPDisturb.Count;
			// データ追加
			if (count > 0)
			{
				for (int j = 0; j < count; j++)
				{
					m_Table.Data[i].PPDisturb.Add(new DamageTable.PPDisturbData());
				}
			}
			// データ削除
			else if (count < 0)
			{
				for (int j = 0; j < Mathf.Abs(count); j++)
				{
					m_Table.Data[i].PPDisturb.RemoveAt(m_Table.Data[i].PPDisturb.Count - 1);
				}
			}
			EditorGUILayout.EndHorizontal();

			// 妨害パネルデータ
			++EditorGUI.indentLevel;
			int indent = EditorGUI.indentLevel;
			count = 0;
			foreach (DamageTable.PPDisturbData disturb in m_Table.Data[i].PPDisturb)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Disturbance[" + (count++).ToString() + "]", GUILayout.Width(164));
				EditorGUI.indentLevel = 0;
				EditorGUILayout.LabelField("W", GUILayout.Width(14));
				disturb.width = EditorGUILayout.IntField(disturb.width, GUILayout.Width(60));
				EditorGUILayout.LabelField("H", GUILayout.Width(14));
				disturb.height = EditorGUILayout.IntField(disturb.height, GUILayout.Width(60));
				EditorGUILayout.LabelField("x", GUILayout.Width(10));
				disturb.count = EditorGUILayout.IntField(disturb.count, GUILayout.Width(60));
				EditorGUI.indentLevel = indent;
				EditorGUILayout.EndHorizontal();
			}
			--EditorGUI.indentLevel;
			--EditorGUI.indentLevel;

			GUILayout.Space(8);
		}

		GUILayout.Space(20f);

		// 追加
		if (GUILayout.Button("Add", GUILayout.Height(30)))
		{
			m_Table.Data.Add(new DamageTable.DamageData());
		}

		EditorUtility.SetDirty(m_Table);
	}
}
