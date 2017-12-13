using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaledGUI
{
	/// <summary> ターゲットスクリーンサイズ </summary>
	public static float targetScreenWidth = 1080;
	
	/// <summary> デフォルトアンカー </summary>
	public static TextAnchor defaultAlignment = TextAnchor.UpperLeft;
	/// <summary> デフォルトフォントカラー </summary>
	public static Color defaultFontColor = Color.white;
	/// <summary> デフォルトフォントサイズ </summary>
	public static int defaultFontSize = 50;

	/// <summary>
	/// テキスト描画
	/// </summary>
	public static void Label(string text, TextAnchor alignment, Vector2 position, Color color, int size)
	{
		position.x = Mathf.Max(0, Mathf.Min(1, position.x));
		position.y = Mathf.Max(0, Mathf.Min(1, position.y));
		Rect rect = new Rect(position.x * Screen.width, position.y * Screen.height, Screen.width, Screen.height);

		GUIStyleState styleState = new GUIStyleState();
		styleState.textColor = color;

		GUIStyle style = new GUIStyle();
		style.fontSize = (int)(size * (Screen.width / targetScreenWidth));
		style.normal = styleState;
		style.alignment = alignment;

		GUI.Label(rect, text, style);
	}
	/// <summary>
	/// テキスト描画
	/// </summary>
	public static void Label(string text, Color color)
	{
		Label(text, defaultAlignment, Vector2.zero, color, defaultFontSize);
	}
	/// <summary>
	/// テキスト描画
	/// </summary>
	public static void Label(string text, TextAnchor alignment)
	{
		Label(text, alignment, Vector2.zero, defaultFontColor, defaultFontSize);
	}
	/// <summary>
	/// テキスト描画
	/// </summary>
	public static void Label(string text, Vector2 position)
	{
		Label(text, defaultAlignment, position, defaultFontColor, defaultFontSize);
	}
	/// <summary>
	/// テキスト描画
	/// </summary>
	public static void Label(string text)
	{
		Label(text, defaultAlignment, Vector2.zero, defaultFontColor, defaultFontSize);
	}
}
