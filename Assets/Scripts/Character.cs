using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
	private SpriteRenderer m_SpriteRenderer = null;
	private Animator m_Animator = null;
	private int m_Id = 0;

	protected void Awake()
	{
		m_SpriteRenderer = GetComponent<SpriteRenderer>();
		m_Animator = GetComponent<Animator>();
		ShowChain(false, 0);
		Visible = false;
	}

	/// <summary>
	/// 初期化
	/// </summary>
	public void Initialize(int id)
	{
		m_Id = id;
		m_SpriteRenderer.sprite = Resources.Load("Characters/Chara_0" + (id).ToString(), typeof(Sprite)) as Sprite;
	}

	protected void Start()
	{
	}

	public void PlayAnim(string name)
	{
		m_Animator.Play(name, 0, 0f);
	}

	public bool IsAnimStoped()
	{
		return m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
	}

	public bool Visible
	{
		get { return m_SpriteRenderer.enabled; }
		set { m_SpriteRenderer.enabled = value; }
	}

	public void ShowChain(bool visible, int count)
	{
		GameObject obj = transform.Find("Chain").gameObject;
		obj.SetActive(visible);
		if (visible)
		{
			TextMesh mesh = obj.GetComponent<TextMesh>();
			mesh.text = count.ToString() + " れんさ";
		}
	}

	public int Id
	{
		get { return m_Id; }
	}
}
