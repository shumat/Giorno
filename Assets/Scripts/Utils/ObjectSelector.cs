using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSelector : MonoBehaviour
{
	/// <summary> 初期選択オブジェクト </summary>
	[SerializeField]
	private GameObject m_Default = null;

	/// <summary> 選択オブジェクト </summary>
	private GameObject m_Current = null;

	/// <summary>
	/// 生成
	/// </summary>
	protected void Awake()
	{
		Select(m_Default);
	}

	/// <summary>
	/// オブジェクト選択
	/// </summary>
	public GameObject Select(GameObject obj)
	{
		m_Current = obj;

		GameObject child = null;
		for (int i = 0; i < transform.childCount; i++)
		{
			child = transform.GetChild(i).gameObject;
			child.SetActive(child == m_Current);
		}

		return m_Current;
	}

	/// <summary>
	/// オブジェクト選択
	/// </summary>
	public GameObject SelectByName(string name)
	{
		return Select(transform.Find(name).gameObject);
	}

	/// <summary>
	/// オブジェクト選択解除
	/// </summary>
	public void Deselect()
	{
		Select(null);
	}

	/// <summary>
	/// 選択オブジェクト
	/// </summary>
	public GameObject Current
	{
		get { return m_Current; }
	}
}
