using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public class CameraOrthographicSizeSetter : MonoBehaviour
{
	public float horizontal = 9f;
	public float vertical = 16f;

	/// <summary>
	/// 生成
	/// </summary>
	private void Awake()
	{
		Camera camera = gameObject.GetComponent<Camera>();

		float targetAspect = horizontal / vertical;
		float aspect = (float)Screen.width / Screen.height;

		// 理想より横に広い
		if (aspect > targetAspect)
		{
			camera.orthographicSize += aspect - targetAspect;
		}
		// 理想より縦に長い
		else
		{
			camera.orthographicSize *= targetAspect / aspect;
		}

		Destroy (this);
	}
}
