using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MatchingManager
{
	/// <summary>
	/// ネットワークマネージャ
	/// </summary>
	private NetworkManager m_NetworkManager = null;

	/// <summary>
	/// ネットワークマネージャ登録
	/// </summary>
	public void SetNetworkManager(NetworkManager newNetworkManager)
	{
		m_NetworkManager = newNetworkManager;
	}

	/// <summary>
	/// サーバ開始
	/// </summary>
	public void StartServer()
	{
		if (IsOffline)
		{
			m_NetworkManager.StartServer();
			Debug.Log("StartServer");
		}
	}

	/// <summary>
	///  ホスト開始
	/// </summary>
	public void StartHost()
	{
		if (IsOffline)
		{
			m_NetworkManager.StartHost();
			Debug.Log("StartHost");
		}
	}

	/// <summary>
	/// ホスト停止
	/// </summary>
	public void StopHost()
	{
		if (NetworkServer.active && NetworkClient.active)
		{
			m_NetworkManager.StopHost();
			Debug.Log("StopHost");
		}
	}

	/// <summary>
	/// クライアント開始
	/// </summary>
	public void StartClient()
	{
		if (IsOffline)
		{
			m_NetworkManager.StartClient();
			Debug.Log("StartClient");
		}
	}

	/// <summary>
	/// クライアント停止
	/// </summary>
	public void StopClient()
	{
		if (!NetworkServer.active && NetworkClient.active)
		{
			m_NetworkManager.StopClient();
			Debug.Log("StopClient");
		}
	}

	/// <summary>
	/// マッチングメーカー有効化
	/// </summary>
	public void StartMatchMaker()
	{
		if (IsOffline)
		{
			m_NetworkManager.StartMatchMaker();
			Debug.Log("StartMatchMaker");
		}
	}

	/// <summary>
	/// マッチングメーカー無効化
	/// </summary>
	public void DisableMatchMaker()
	{
		if (m_NetworkManager.matchMaker != null)
		{
			m_NetworkManager.StopMatchMaker();
			Debug.Log("StopMatchMaker");
		}
	}

	/// <summary>
	/// ルーム作成
	/// </summary>
	public Coroutine CreateMatch()
	{
		if (m_NetworkManager.matchMaker != null && m_NetworkManager.matchInfo == null && m_NetworkManager.matches == null)
		{
			Debug.Log("CreateMatch");
			System.DateTime now = System.DateTime.Now;
			m_NetworkManager.matchName = "RM" + now.Hour.ToString() + now.Minute.ToString() + now.Second.ToString();
			return m_NetworkManager.matchMaker.CreateMatch(m_NetworkManager.matchName, m_NetworkManager.matchSize, true, "", "", "", 0, 0, m_NetworkManager.OnMatchCreate);
		}
		return null;
	}

	/// <summary>
	/// ルーム解散
	/// </summary>
	public Coroutine DestroyMatch()
	{
		if (m_NetworkManager.matchMaker != null && m_NetworkManager.matchInfo != null)
		{
			Debug.Log("DestroyMatch");
			return m_NetworkManager.matchMaker.DestroyMatch(m_NetworkManager.matchInfo.networkId, m_NetworkManager.matchInfo.domain, m_NetworkManager.OnDestroyMatch);
		}
		return null;
	}

	/// <summary>
	/// ルーム検索
	/// </summary>
	public Coroutine FindMatch()
	{
		if (m_NetworkManager.matchMaker != null && m_NetworkManager.matchInfo == null && m_NetworkManager.matches == null)
		{
			Debug.Log("FindMatch");
			return m_NetworkManager.matchMaker.ListMatches(0, 20, "", false, 0, 0, m_NetworkManager.OnMatchList);
		}
		return null;
	}

	/// <summary>
	/// ルーム検索中止
	/// </summary>
	public void StopFindMatch()
	{
		m_NetworkManager.matches = null;
		Debug.Log("StopFindMatch");
	}

	/// <summary>
	/// ルームインデックスリスト取得
	/// </summary>
	public int[] GetMatchIndices()
	{
		List<int> matches = new List<int>();
		for (int i = 0; i < m_NetworkManager.matches.Count; i++)
		{
			if (m_NetworkManager.matches[i].currentSize > 0)
			{
				matches.Add(i);
			}
		}
		return matches.ToArray();
	}

	/// <summary>
	/// ルーム参加
	/// </summary>
	public Coroutine JoinMatch(int index = -1)
	{
		if (m_NetworkManager.matchMaker != null && m_NetworkManager.matchInfo == null && m_NetworkManager.matches != null)
		{
			if (index == -1)
			{
				int[] matches = GetMatchIndices();
				if (matches.Length > 0)
				{
					index = matches[0];
				}
				else
				{
					return null;
				}
			}

			if (m_NetworkManager.matches.Count > index)
			{
				Debug.Log("JoinMatch");
				m_NetworkManager.matchName = m_NetworkManager.matches[index].name;
				m_NetworkManager.matchSize = (uint)m_NetworkManager.matches[index].maxSize;
				return m_NetworkManager.matchMaker.JoinMatch(m_NetworkManager.matches[index].networkId, "", "", "", 0, 0, m_NetworkManager.OnMatchJoined);
			}
		}
		return null;
	}

	/// <summary>
	/// オフライン?
	/// </summary>
	public bool IsOffline
	{
		get { return !NetworkServer.active && !NetworkClient.active && m_NetworkManager.matchMaker == null; }
	}
}
