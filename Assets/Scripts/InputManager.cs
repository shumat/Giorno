using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
	/// <summary>
	/// タッチ座標取得
	/// </summary>
	public static Vector3 GetTouchPosition()
	{
		Vector3 pos = Vector3.zero;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		pos = Input.mousePosition;
#else
		if (Input.touchCount > 0)
		{
			pos.x = Input.GetTouch(0).position.x;
			pos.y = Input.GetTouch(0).position.y;
		}
#endif
		return pos;
	}

	/// <summary>
	/// ワールドタッチ座標取得
	/// </summary>
	public static Vector3 GetWorldTouchPosition()
	{
		return Camera.main.ScreenToWorldPoint(GetTouchPosition());
	}

	/// <summary>
	/// タッチ中？
	/// </summary>
	public static bool IsTouch()
	{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		return Input.GetMouseButton(0);
#else
		if (Input.touchCount > 0)
		{
			return Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary;
		}
		return false;
#endif
	}

	/// <summary>
	/// タッチした瞬間？
	/// </summary>
	public static bool IsTouchDown()
	{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		return Input.GetMouseButtonDown(0);
#else
		if (Input.touchCount > 0)
		{
			return Input.GetTouch(0).phase == TouchPhase.Began;
		}
		return false;
#endif
	}


	/// <summary>
	/// タッチを離した瞬間？
	/// </summary>
	public static bool IsTouchUp()
	{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		return Input.GetMouseButtonUp(0);
#else
		if (Input.touchCount > 0)
		{
			return Input.GetTouch(0).phase == TouchPhase.Ended;
		}
		return false;
#endif
	}
}
