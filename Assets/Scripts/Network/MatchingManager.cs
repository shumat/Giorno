using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MatchingManager : MonoBehaviour
{
	/// <summary> メニュー </summary>
	private ObjectSelector m_Menu = null;
	/// <summary> オンラインメニュー </summary>
	private GameObject m_OnlineMenu = null;

	/// <summary> LANを使用 </summary>
	private bool m_UseLocalNetwork = false;

	/// <summary> マッチングコルーチン </summary>
	private IEnumerator m_MatchingCoroutine = null;

	/// <summary> デフォルトポート番号 </summary>
	private int m_DefaultNetworkPort = 0;

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

	/// <summary>
	/// マッチング
	/// </summary>
	public IEnumerator Matching()
	{
		Debug.Log("Start global match");

		NetworkGameManager nm = NetworkGameManager.Instance;

		// マッチ開始
		nm.StartMatchMaker();

		float findTime = 2f;
		while (findTime > 0f)
		{
			// ルーム検索
			yield return nm.FindMatch();

			// ルーム参加
			yield return nm.JoinMatch();

			// 成功
			if (nm.IsJoinedMatch)
			{
				break;
			}

			findTime -= Time.deltaTime;
		}

		// ルーム未参加ならルーム作成
		if (nm.IsJoinedMatch)
		{
			yield return nm.CreateMatch();

			// ルーム作成に失敗したらリスタート
			if (!nm.IsCreatedMatch)
			{
				StartCoroutine(Matching());
				yield break;
			}
		}

		// メンバーが揃うまで待機
		while (!nm.IsMatchComplete)
		{
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
		float connectWait = 5f;
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
			nm.StartHost();
		}

		// メンバーが揃うまで待機
		while (!nm.IsMatchComplete)
		{
			yield return null;
		}

		// ゲーム開始
		StartCoroutine(StartGame(true));
	}

	/// <summary>
	/// ゲーム開始
	/// </summary>
	public IEnumerator StartGame(bool online)
	{
		Debug.Log("Start game(" + (online ? "Online" : "Offline") + ")");

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

	/// <summary>
	/// マッチキャンセル
	/// </summary>
	public void CancelMatch()
	{
		// マッチング中止
		StopCoroutine(m_MatchingCoroutine);

		// ルーム解散
		if (NetworkGameManager.Instance.IsCreatedMatch)
		{
			NetworkGameManager.Instance.DestroyMatch();
		}
		// ルーム退室
		if (NetworkGameManager.Instance.IsJoinedMatch)
		{
			NetworkGameManager.Instance.DropMatch();
		}

		// ローカルマッチ停止
		NetworkGameManager.Instance.StopLocalMatch();

		// マッチ無効化
		NetworkGameManager.Instance.DisableMatchMaker();

		m_Menu.SelectByName("GameMode");
	}

	/// <summary>
	/// LAN使用切り替え
	/// </summary>
	public void ToggleLocalNetworkUsing()
	{
		m_UseLocalNetwork = !m_UseLocalNetwork;
	}
}
