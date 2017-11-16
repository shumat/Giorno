using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugDisplay : MonoBehaviour
{
	private Text m_FrameRate = null;

	private void Start()
	{
		m_FrameRate = transform.Find("FrameRate").GetComponentInChildren<Text>();
	}

	private void Update()
	{
		m_FrameRate.text = "fps: " + (1f / Time.deltaTime).ToString();
	}
}
