using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class UnityLogExporter : MonoBehaviour
{
	/// <summary> 過去ログGUI表示数 </summary>
	public int maxShowHistoryOnGUI = 0;

	/// <summary> GUIフォントサイズ </summary>
	public int GUIFontSize = 24;

	/// <summary> GUIスタイル </summary>
	private GUIStyle m_GUIStyle;

	/// <summary> パス </summary>
	public string path = "Logs/log-{DATE}.txt";

	/// <summary> フルパス </summary>
	private string m_FullPath = "";

	/// <summary> 過去ログ </summary>
	private List<string> m_History = null;

	/// <summary>
	/// 生成
	/// </summary>
	private void Awake()
	{
		// フルパス作成
		m_FullPath = path.Replace("{DATE}", DateTime.Now.ToString("s").Replace(":", "").Replace("-", ""));

#if !UNITY_EDITOR
		m_FullPath = Application.dataPath + "/" + m_FullPath;
#endif

#if !UNITY_IOS
		// ディレクトリ作成
		string directoryPath = m_FullPath.Remove(m_FullPath.LastIndexOf("/"));
		if (!Directory.Exists(directoryPath))
		{
			Directory.CreateDirectory(directoryPath);
		}

		// ヘッダー書き込み
		StreamWriter sw = new StreamWriter(m_FullPath, true);
		sw.WriteLine("------------------------");
		sw.WriteLine("Unity Log Exporter");
		sw.WriteLine("Ver " + Application.version.ToString());
		sw.WriteLine("Date " + DateTime.Now.ToString("s"));
		sw.WriteLine("------------------------");
		sw.Flush();
		sw.Close();
#endif

		// GUIスタイル初期化
		GUIStyleState styleState = new GUIStyleState();
		styleState.textColor = Color.white;
		m_GUIStyle = new GUIStyle();
		m_GUIStyle.normal = styleState;
		m_GUIStyle.fontSize = GUIFontSize;

		// ログイベント追加
		Application.logMessageReceived += OnLogMessage;
	}

	/// <summary>
	/// 破棄
	/// </summary>
	private void OnDestroy()
	{
		Application.logMessageReceived -= OnLogMessage;
	}

	/// <summary>
	/// ログメッセージイベント
	/// </summary>
	private void OnLogMessage(string logText, string stackTrace, LogType type)
	{
		if (string.IsNullOrEmpty(logText))
		{
			return;
		}

		// タイム＋ログ
		StringBuilder sb = new StringBuilder();
		sb.AppendFormat("[{0}] {1}", Time.time.ToString("0.000"), logText).AppendLine();

		// スタックトレース
		if (!string.IsNullOrEmpty(stackTrace) && type != LogType.Log)
		{
			sb.Append(stackTrace).AppendLine();
		}

#if !UNITY_IOS
		StreamWriter sw = new StreamWriter(m_FullPath, true);
		sw.Write(sb.ToString());
		sw.Flush();
		sw.Close();
#endif

		if (maxShowHistoryOnGUI > 0)
		{
			if (m_History == null)
			{
				m_History = new List<string>();
			}
			if (m_History.Count >= maxShowHistoryOnGUI)
			{
				m_History.RemoveAt(m_History.Count - 1);
			}
			m_History.Insert(0, sb.ToString());
		}
	}

	/// <summary>
	/// GUI更新
	/// </summary>
	private void OnGUI()
	{
		// 過去ログ表示
		if (m_History != null && m_History.Count > 0)
		{
			StringBuilder sb = new StringBuilder();
			foreach (string str in m_History)
			{
				sb.Append(str);
			}
			GUI.Label(new Rect(0, 0, Screen.width, Screen.height), sb.ToString(), m_GUIStyle);
		}
	}
}
