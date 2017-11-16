using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : MonoBehaviour
{
	/// <summary>
	/// 一時停止中
	/// </summary>
	public bool IsPause { get; protected set; }

	/// <summary>
	/// 初期化
	/// </summary>
	public virtual void Initialize(){}

	/// <summary>
	/// メイン処理
	/// </summary>
	public virtual void Process()
	{
		if (IsPause)
		{
			OnPause();
		}
		else
		{
			Play();
		}
	}

	/// <summary>
	/// 更新
	/// </summary>
	public virtual void Play(){}

	/// <summary>
	/// 一時停止中
	/// </summary>
	public virtual void OnPause(){}

	/// <summary>
	/// フレーム更新
	/// </summary>
	public virtual void StepUpdate()
	{
		IsPause = true;
		Play();
	}

	/// <summary>
	/// 一時停止
	/// </summary>
	public virtual void SetPause(bool pause)
	{
		IsPause = pause;
	}
}
