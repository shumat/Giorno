using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MatchingManager : MonoBehaviour
{
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
	}

	protected void OnGUI()
	{
		string text = "";
		if (NetworkGameManager.Instance.PlayerCount > 0)
		{
			if (NetworkServer.active)
			{
				text += "Match created\n";
				text += "Waiting for player (" + NetworkGameManager.Instance.PlayerCount.ToString() + " / " + NetworkGameManager.Instance.matchSize.ToString() + ")\n";
			}
			else if (NetworkClient.active)
			{
				text += "Match joined\n";
			}
		}
		else if (m_MatchingCoroutine != null)
		{
			text += "Finding...";
		}
		ScaledGUI.Label(text, TextAnchor.MiddleCenter);
	}

	#region Match

	/// <summary>
	/// マッチング
	/// </summary>
	public IEnumerator Matching()
	{
		Debug.Log("Start global match");

		NetworkGameManager nm = NetworkGameManager.Instance;
		nm.networkPort = NetworkGameManager.Instance.DefaultNetworkPort;

		// マッチ開始
		if (nm.matchMaker == null)
		{
			nm.StartMatchMaker();
		}

		// ルーム検索
		yield return nm.FindMatch();
		// ルーム参加
		yield return nm.JoinMatch();

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
		float waitTime = m_OtherPlayerCloneWaitTime;
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

		m_MatchingCoroutine = null;
	}

	/// <summary>
	/// ローカルサーバーとして接続
	/// </summary>
	public IEnumerator ConnectLocalServer()
	{
		Debug.Log("Start local server");

		NetworkGameManager nm = NetworkGameManager.Instance;
		nm.networkPort = nm.DefaultNetworkPort;

		// 初期化
		nm.Discovery.Initialize();

		// サーバーとして開始
		if (nm.Discovery.StartAsServer() && nm.StartHost() != null)
		{
			// メンバーが揃うまで待機
			while (NetworkGameManager.Instance.PlayerCount < NetworkGameManager.Instance.matchSize)
			{
				yield return null;
			}

			nm.Discovery.StopBroadcast();

			// ゲーム開始
			StartCoroutine(StartGame(true));
		}
		else
		{
			m_MatchingCoroutine = null;
			yield return CancelMatch();
		}
	}

	/// <summary>
	/// ローカルクライアントとして接続
	/// </summary>
	public IEnumerator ConnectLocalClient()
	{
		Debug.Log("Start local client");

		NetworkGameManager nm = NetworkGameManager.Instance;
		nm.networkPort = nm.DefaultNetworkPort;

		// 初期化
		nm.Discovery.Initialize();

		// クライアントとして開始
		if (nm.Discovery.StartAsClient())
		{
			// サーバー検索
			while (nm.Discovery.broadcastsReceived == null || nm.Discovery.broadcastsReceived.Count == 0)
			{
				yield return null;
			}

			// アドレスを設定して開始
			foreach (var results in nm.Discovery.broadcastsReceived)
			{
				string address = results.Value.serverAddress;
				address = address.Substring(address.LastIndexOf(':') + 1);

				nm.networkAddress = address;
				nm.StartClient();
				break;
			}

			// メンバーが揃うまで待機
			while (NetworkGameManager.Instance.PlayerCount < NetworkGameManager.Instance.matchSize)
			{
				yield return null;
			}

			nm.Discovery.StopBroadcast();

			// ゲーム開始
			StartCoroutine(StartGame(true));
		}
		else
		{
			m_MatchingCoroutine = null;
			yield return CancelMatch();
		}
	}

	#endregion

	#region Start

	/// <summary>
	/// ゲーム開始
	/// </summary>
	public IEnumerator StartGame(bool online)
	{
		Debug.Log("Start game(" + (online ? "Online" : "Offline") + ")");

		// オフライン
		if (!online)
		{
			// PlayerのSpawnを待つ
			while (NetworkGameManager.Instance.LocalPlayer == null)
			{
				yield return null;
			}
			// ボットを生成
			for (int i = NetworkGameManager.Instance.PlayerCount; i < NetworkGameManager.Instance.matchSize; i++)
			{
				NetworkGameManager.Instance.SpawnBot();
			}
			// メンバーが揃うまで待機
			while (NetworkGameManager.Instance.PlayerCount < NetworkGameManager.Instance.matchSize)
			{
				yield return null;
			}
		}

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
		StartCoroutine(m_MatchingCoroutine = Matching());
	}

	/// <summary>
	/// ローカルサーバーとして開始
	/// </summary>
	public void StartLocalServer()
	{
		m_Menu.SelectByName("Online");
		StartCoroutine(m_MatchingCoroutine = ConnectLocalServer());
	}
	/// <summary>
	/// ローカルクライアントとして開始
	/// </summary>
	public void StartLocalClient()
	{
		m_Menu.SelectByName("Online");
		StartCoroutine(m_MatchingCoroutine = ConnectLocalClient());
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
		if (m_MatchingCoroutine != null)
		{
			StopCoroutine(m_MatchingCoroutine);
			m_MatchingCoroutine = null;
		}

		yield return NetworkGameManager.Instance.Disconnect();

		m_Menu.SelectByName("GameMode");
		m_IsValidMatchCancel = true;
	}

	#endregion
}
