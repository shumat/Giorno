using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tetris/GameConfig")]
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

	public byte GetLineVanishDamageLevel(int count)
	{
		if (count > 0)
		{
			return (byte)(count + 3);
		}
		return 0;
	}

	public int GetDamageLineCount(byte level)
	{
		return (int)level - 3;
	}
}
