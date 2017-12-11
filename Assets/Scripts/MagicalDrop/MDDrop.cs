using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MDDrop : MonoBehaviour
{
	/// <summary>
	/// ドロップタイプ
	/// </summary>
	public enum Type
	{
		None,
		Red,
		Blue,
		Yellow,
		Green,
		Frozen,
		Bubble
	}

	/// <summary>
	/// ステート
	/// </summary>
	public enum State
	{
		None,
		Vanish,
		Close,
		Pull,
		Push,
		Change,
	}

	/// <summary> スプライトレンダラ </summary>
	private SpriteRenderer m_SpriteRenderer = null;
	
	/// <summary> プレイエリア </summary>
	private MDPlayArea m_Area = null;
	
	/// <summary> ドロップタイプ </summary>
	private Type m_Type = Type.None;
	
	/// <summary> スペシャルドロップ </summary>
	//private bool m_IsSpecial = false;

	/// <summary> ステート </summary>
	private State m_State = State.None;

	/// <summary> プレイ中のコルーチン </summary>
	private IEnumerator m_PlayingCoroutine = null;

	/// <summary> 移動ベクトル </summary>
	private Vector3 m_MoveDir = Vector3.zero;

	/// <summary> 所属ブロック </summary>
	public MDDropBlock Block { get; set; }

	/// <summary> 消滅可能? </summary>
	public bool IsValidVanish { get; set; }

	/// <summary> プッシュされた? </summary>
	public bool IsPushed { get; set; }

	/// <summary> 即座に消滅する形でプッシュされた? </summary>
	public bool IsMatchPushed { get; set; }

	/// <summary> マテリアルリスト </summary>
	private Dictionary<Type, Material> m_Materials = new Dictionary<Type, Material>();

	/// <summary> 消滅用マテリアル </summary>
	private Material m_DestroyMaterial = null;

	/// <summary>
	/// 生成
	/// </summary>
	protected void Awake()
	{
		m_SpriteRenderer = GetComponent<SpriteRenderer>();
		m_DebugText = GetComponentInChildren<TextMesh>();

		m_Materials.Add(Type.None, Resources.Load<Material>("MagicalDrop/Materials/Drop"));
		m_Materials.Add(Type.Frozen, Resources.Load<Material>("MagicalDrop/Materials/FrozenDrop"));
		m_DestroyMaterial = Resources.Load<Material>("MagicalDrop/Materials/DropDestroy");
	}

	private TextMesh m_DebugText = null;
	protected void Update()
	{
		m_DebugText.text = "";
		if (Block == null)
		{
			m_DebugText.text = "none\n";
		}

		if (m_State != State.None)
		{
			m_DebugText.text += m_State.ToString() + "\n";
		}

		if (IsMatchPushed)
		{
			m_DebugText.text += "MatchPush\n";
		}

		if (IsValidVanish)
		{
			m_DebugText.text += "ValidVanish\n";
		}
	}

	/// <summary>
	/// 初期化
	/// </summary>
	public void Initialize(MDPlayArea area)
	{
		gameObject.SetActive(true);

		m_Area = area;
		m_State = State.None;
		m_Type = (Type)area.Game.Controller.SyncRand.Next((int)Type.Red, (int)Type.Frozen + 1);
		IsValidVanish = false;
		IsPushed = false;
		IsMatchPushed = false;

		m_SpriteRenderer.enabled = true;
		SetMaterial(m_Type);
	}

	/// <summary>
	/// マテリアル登録
	/// </summary>
	private void SetMaterial(Type type)
	{
		if (type == Type.Frozen)
		{
			m_SpriteRenderer.material = m_Materials[Type.Frozen];
			m_SpriteRenderer.material.SetFloat("_Brightness_Fade_1", 0);
		}
		else
		{
			m_SpriteRenderer.material = m_Materials[Type.None];
			m_SpriteRenderer.material.SetFloat("_Brightness_Fade_1", 0);
			switch (type)
			{
				case Type.Red:
					m_SpriteRenderer.material.SetColor("_TintRGBA_Color_1", Color.red);
					break;
				case Type.Blue:
					m_SpriteRenderer.material.SetColor("_TintRGBA_Color_1", Color.blue);
					break;
				case Type.Yellow:
					m_SpriteRenderer.material.SetColor("_TintRGBA_Color_1", Color.yellow);
					break;
				case Type.Green:
					m_SpriteRenderer.material.SetColor("_TintRGBA_Color_1", Color.green);
					break;
			}
		}
	}

	/// <summary>
	/// 消滅開始
	/// </summary>
	public void BeginVanish()
	{
		if (m_State == State.None)
		{
			m_State = State.Vanish;
			StartCoroutine(m_PlayingCoroutine = Vanishing());
		}
	}

	/// <summary>
	/// 消滅
	/// </summary>
	private IEnumerator Vanishing()
	{
		float brightness = 0;
		while (brightness < 1)
		{
			brightness = Mathf.Min(brightness + GameManager.TimeStep * 4f, 1f);
			m_SpriteRenderer.material.SetFloat("_Brightness_Fade_1", brightness);
			yield return null;
		}

		m_SpriteRenderer.material = m_DestroyMaterial;

		float destroyerValue = 0;
		while (destroyerValue < 1)
		{
			destroyerValue = Mathf.Min(destroyerValue + GameManager.TimeStep * 2.5f, 1f);
			m_SpriteRenderer.material.SetFloat("_Destroyer_Value_1", destroyerValue);

			yield return null;
		}

		m_Area.RemoveDrop(this);
		gameObject.SetActive(false);

		m_State = State.None;
		m_PlayingCoroutine = null;
	}

	/// <summary>
	/// 上詰め開始
	/// </summary>
	public void BeginClose()
	{
		if (m_State == State.None)
		{
			m_State = State.Close;
			StartCoroutine(m_PlayingCoroutine = Pushing(null));
		}
	}

	/// <summary>
	/// プル開始
	/// </summary>
	public void BeginPull(MDPlayer player)
	{
		if (m_State == State.None)
		{
			m_State = State.Pull;
			StartCoroutine(m_PlayingCoroutine = Pulling(player));
		}
	}

	/// <summary>
	/// プル
	/// </summary>
	private IEnumerator Pulling(MDPlayer player)
	{
		m_MoveDir = Vector3.down;

		while (true)
		{
			float moveDelta = GameManager.TimeStep * MDGame.Config.DropPullSpeed;
			if (Vector3.Distance(player.transform.position, transform.position) > moveDelta)
			{
				// プレイヤーへのベクトル
				Vector3 dir = player.transform.position - transform.position;
				dir.z = 0;
				dir.Normalize();

				// 回転角を算出
				float angle = Mathf.Min(Vector3.Angle(m_MoveDir, dir), MDGame.Config.DropPullMaxAngle);

				// 今回の移動方向
				m_MoveDir = (Quaternion.AngleAxis(angle, Vector3.Cross(m_MoveDir, dir)) * m_MoveDir).normalized;

				// 移動
				transform.position += m_MoveDir * moveDelta;
			}
			else
			{
				transform.position = player.transform.position;
				break;
			}

			yield return null;
		}

		m_SpriteRenderer.enabled = false;
		m_State = State.None;
		m_PlayingCoroutine = null;
	}

	/// <summary>
	/// プッシュ開始
	/// </summary>
	public void BeginPush(Vector3? startPosition)
	{
		if (m_State == State.None)
		{
			m_State = State.Push;
			StartCoroutine(m_PlayingCoroutine = Pushing(startPosition));
		}
	}

	/// <summary>
	/// プッシュ
	/// </summary>
	private IEnumerator Pushing(Vector3? startPosition)
	{
		m_SpriteRenderer.enabled = true;

		if (startPosition != null)
		{
			transform.position = startPosition.Value;
		}

		while (true)
		{
			transform.localPosition += Vector3.up * GameManager.TimeStep * MDGame.Config.DropPushSpeed;

			// 通過してたら座標を戻す
			if (transform.localPosition.y > Block.LocalPosition.y)
			{
				transform.localPosition = Block.LocalPosition;
				break;
			}

			yield return null;
		}

		// ブロックにアタッチ
		Block.ApplyhReservedDrop();

		// プッシュ終了イベント
		if (m_State == State.Push)
		{
			m_Area.PushEndEvent(this);
		}

		m_State = State.None;
		m_PlayingCoroutine = null;
		yield return null;
	}

	/// <summary>
	/// タイプチェンジ開始
	/// </summary>
	public void BeginChange(Type type)
	{
		if (m_State == State.None)
		{
			m_State = State.Change;
			StartCoroutine(m_PlayingCoroutine = Changing(type));
		}
	}

	/// <summary>
	/// タイプチェンジ
	/// </summary>
	private IEnumerator Changing(Type type)
	{
		float brightness = 0;
		while (brightness < 1)
		{
			brightness = Mathf.Min(brightness + GameManager.TimeStep * 4f, 1f);
			m_SpriteRenderer.material.SetFloat("_Brightness_Fade_1", brightness);
			yield return null;
		}

		// マテリアル変更
		m_SpriteRenderer.material = m_Materials[Type.None];
		SetMaterial(type);

		while (brightness > 0)
		{
			brightness = Mathf.Max(brightness - GameManager.TimeStep * 4f, 0);
			m_SpriteRenderer.material.SetFloat("_Brightness_Fade_1", brightness);
			yield return null;
		}

		m_Type = type;

		m_State = State.None;
		m_PlayingCoroutine = null;
	}

	/// <summary>
	/// ステートキャンセル
	/// </summary>
	public void CancelState()
	{
		// プッシュ終了イベント
		if (m_State == State.Push)
		{
			m_Area.PushEndEvent(this);
		}

		m_State = State.None;
		if (m_PlayingCoroutine != null)
		{
			StopCoroutine(m_PlayingCoroutine);
			m_PlayingCoroutine = null;
		}
	}

	/// <summary>
	/// 一時停止
	/// </summary>
	public void BeginPause()
	{
		if (m_PlayingCoroutine != null)
		{
			StopCoroutine(m_PlayingCoroutine);
		}
	}

	/// <summary>
	/// 一時停止終了
	/// </summary>
	public void EndPause()
	{
		if (m_PlayingCoroutine != null)
		{
			StartCoroutine(m_PlayingCoroutine);
		}
	}

	/// <summary>
	/// ロック中?
	/// </summary>
	public bool IsLocked()
	{
		return m_State != State.None;
	}

	/// <summary>
	/// ステート
	/// </summary>
	public State CurrentState
	{
		get { return m_State; }
	}

	/// <summary>
	/// ドロップタイプ
	/// </summary>
	public Type DropType
	{
		get { return m_Type; }
	}

	/// <summary>
	/// スプライトカラー
	/// </summary>
	public Color SpriteColor
	{
		get { return m_SpriteRenderer.color; }
	}

	/// <summary>
	/// 消滅無効
	/// </summary>
	public bool IsIgnoreVanish
	{
		get { return m_Type == Type.Frozen || m_Type == Type.Bubble; }
	}
}
