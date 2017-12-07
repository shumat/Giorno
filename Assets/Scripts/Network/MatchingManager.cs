using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MatchingManager : MonoBehaviour
{
	/// <summary> LANを使用 </summary>
	private bool m_UseLocalNetwork = false;

	/// <summary> デフォルトポート番号 </summary>
	private int m_DefaultNetworkPort = 0;

	/// <summary> ルーム検索時間 </summary>
	private float m_RoomFindTime = 2f;

	/// <summary> ルーム参加時に自分以外のプレイヤーを待つ時間 </summary>
	private float m_OtherPlayerCloneWaitTime = 1f;

	/// <summary> マッチングコルーチン </summary>
	private IEnumerator m_MatchingCoroutine = null;

	/// <summary> マッチキャンセル可能? </summary>
	private bool m_IsValidMatchCancel = true;

	/// <summary> メニュー </summary>
	private ObjectSelector m_Menu = null;

	/// <summary>
	/// 生成
	/// </summary>
	protected void Awake()
	{
		m_Menu = GameObject.Find("MatchingCanvas").transform.Find("Menu").GetComponent<ObjectSelector>();
		m_UseLocalNetwork = m_Menu.transform.Find("GameMode/UseLan").GetComponent<Toggle>().isOn;
	}

	/// <summary>
	/// 開始
	/// </summary>
	protected void Start()
	{
		m_DefaultNetworkPort = NetworkGameManager.Instance.networkPort;
	}

	#region Match

	/// <summary>
	/// マッチング
	/// </summary>
	public IEnumerator Matching()
	{
		Debug.Log("Start global match");

		NetworkGameManager nm = NetworkGameManager.Instance;

		// マッチ開始
		if (nm.matchMaker == null)
		{
			nm.StartMatchMaker();
		}

		float waitTime = m_RoomFindTime;
		while (waitTime > 0f)
		{
			float startTime = Time.time;

			// ルーム検索
			yield return nm.FindMatch();

			// ルーム参加
			yield return nm.JoinMatch();

			// 成功
			if (nm.IsJoinedMatch)
			{
				break;
			}

			nm.StopFindMatch();

			waitTime -= Time.time - startTime + Time.deltaTime;

			yield return null;
		}

		// ルーム未参加ならルーム作成
		if (!nm.IsJoinedMatch)
		{
			yield return nm.CreateMatch();

			// ルーム作成に失敗したらリスタート
			if (!nm.IsCreatedMatch)
			{
				nm.StopMatchMaker();
				StartCoroutine(m_MatchingCoroutine = Matching());
				yield break;
			}
		}

		// メンバーが揃うまで待機
		waitTime = m_OtherPlayerCloneWaitTime;
		while (NetworkGameManager.Instance.PlayerCount < nm.matchSize)
		{
			// 自分以外のプレイヤーが一定時間いなければ退室
			if (nm.IsJoinedMatch && waitTime < 0 && NetworkGameManager.Instance.PlayerCount <= 1)
			{
				Debug.LogWarning("Host player not found");
				yield return nm.DropMatch();
				StartCoroutine(m_MatchingCoroutine = Matching());
				yield break;
			}
			waitTime -= Time.deltaTime;

			yield return null;
		}

		// ゲーム開始
		StartCoroutine(StartGame(true));
	}

	/// <summary>
	/// ローカルマッチング
	/// </summary>
	public IEnumerator LocalMatching()
	{
		Debug.Log("Start local match");

		NetworkGameManager nm = NetworkGameManager.Instance;

		nm.networkPort = m_DefaultNetworkPort;

		// クライアントとして開始
		nm.StartClient();
		float connectWait = m_RoomFindTime;
		while (!nm.IsClientConnected() && connectWait > 0f)
		{
			connectWait -= Time.deltaTime;
			yield return null;
		}
		if (!nm.IsClientConnected())
		{
			nm.StopClient();
		}

		// クライアントとして開始できなければホストとして開始
		if (!NetworkClient.active)
		{
			if (nm.StartHost() == null)
			{
				StartCoroutine(LocalMatching());
				yield break;
			}
		}

		// メンバーが揃うまで待機
		while (NetworkGameManager.Instance.PlayerCount < nm.matchSize)
		{
			yield return null;
		}

		// ゲーム開始
		StartCoroutine(StartGame(true));
	}

	/// <summary>
	/// LAN使用切り替え
	/// </summary>
	public void ToggleLocalNetworkUsing()
	{
		m_UseLocalNetwork = !m_UseLocalNetwork;
	}

	#endregion

	#region Start

	/// <summary>
	/// ゲーム開始
	/// </summary>
	public IEnumerator StartGame(bool online)
	{
		Debug.Log("Start game(" + (online ? "Online" : "Offline") + ")");

		// オフライン対応のためPlayerのSpawnを待つ
		while (NetworkGameManager.Instance.LocalPlayer == null)
		{
			yield return null;
		}

		// マッチ成功イベント
		NetworkGameManager.Instance.LocalPlayer.OnMatchSucceed();

		m_Menu.SelectByName(null);

		GameManager.Instance.RequestUnloadScene("Matching");
		GameManager.Instance.RequestAddScene("DebugModeSelect", false);
		yield return GameManager.Instance.ApplySceneRequests();
	}

	/// <summary>
	/// オンラインで開始
	/// </summary>
	public void StartOnline()
	{
		m_Menu.SelectByName("Online");

		if (m_UseLocalNetwork)
		{
			StartCoroutine(m_MatchingCoroutine = LocalMatching());
		}
		else
		{
			StartCoroutine(m_MatchingCoroutine = Matching());
		}
	}

	/// <summary>
	/// オフラインで開始
	/// </summary>
	public void StartOffline()
	{
		NetworkGameManager.Instance.networkPort = 0;
		NetworkGameManager.Instance.StartHost();

		m_Menu.Select(null);
		StartCoroutine(StartGame(false));
	}

	#endregion

	#region Cancel
	
	/// <summary>
	/// マッチキャンセル開始
	/// </summary>
	public void BeginMatchCancel()
	{
		if (m_IsValidMatchCancel)
		{
			StartCoroutine(CancelMatch());
		}
	}

	/// <summary>
	/// マッチキャンセル
	/// </summary>
	public IEnumerator CancelMatch()
	{
		m_IsValidMatchCancel = false;

		// マッチング中止
		StopCoroutine(m_MatchingCoroutine);

		// ルーム解散
		if (NetworkGameManager.Instance.IsCreatedMatch)
		{
			yield return NetworkGameManager.Instance.DestroyMatch();
		}
		// ルーム退室
		if (NetworkGameManager.Instance.IsJoinedMatch)
		{
			yield return NetworkGameManager.Instance.DropMatch();
		}

		// ローカルマッチ停止
		NetworkGameManager.Instance.StopLocalMatch();

		// マッチ無効化
		NetworkGameManager.Instance.DisableMatchMaker();

		m_Menu.SelectByName("GameMode");
		m_IsValidMatchCancel = true;
	}

	#endregion
}
