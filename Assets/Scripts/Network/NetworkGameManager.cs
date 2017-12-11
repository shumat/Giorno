using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class NetworkGameManager : NetworkManager
{
	protected void Start()
	{
		DefaultNetworkPort = networkPort;
	}

	#region Player

	/// <summary> プレイヤー </summary>
	private List<PlayerController> m_Players = new List<PlayerController>();

	/// <summary> ローカルプレイヤー </summary>
	public PlayerController LocalPlayer { get; private set; }

	/// <summary> 同期カウント </summary>
	public int SyncCount { get; set; }

	/// <summary>
	/// プレイヤー追加
	/// </summary>
	public void AddPlayerController(PlayerController pc)
	{
		m_Players.Add(pc);

		if (pc.isLocalPlayer)
		{
			LocalPlayer = pc;
		}
	}

	/// <summary>
	/// プレイヤー破棄
	/// </summary>
	public void ReleasePlayer()
	{
		m_Players.Clear();
	}

	/// <summary>
	/// 更新可能?
	/// </summary>
	public bool IsReadyUpdate(byte frame)
	{
		if (m_Players == null || m_Players.Count == 0)
		{
			return false;
		}

		foreach (PlayerController player in m_Players)
		{
			if (!player.IsReadyUpdate(frame))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// 全プレイヤー取得
	/// </summary>
	public PlayerController[] GetPlayers()
	{
		return m_Players.ToArray();
	}

	/// <summary>
	/// プレイヤー数
	/// </summary>
	public int PlayerCount
	{
		get { return m_Players.Count; }
	}

	/// <summary>
	/// ボット生成
	/// </summary>
	public PlayerController SpawnBot()
	{
		if (NetworkServer.active)
		{
			PlayerController bot = Instantiate(playerPrefab).GetComponent<PlayerController>();
			bot.IsBot = true;
			NetworkServer.Spawn(bot.gameObject);
			return bot;
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// ボット取得
	/// </summary>
	public PlayerController[] GetBots()
	{
		List<PlayerController> bots = new List<PlayerController>();
		foreach (var player in m_Players)
		{
			if (player.IsBot)
			{
				bots.Add(player);
			}
		}
		return bots.ToArray();
	}

	/// <summary>
	/// 更新
	/// </summary>
	protected void LateUpdate()
	{
		// 古いコマンドを破棄
		ulong? minFrame = null;
		foreach (PlayerController player in m_Players)
		{
			if (minFrame == null || minFrame.Value > player.TotalFrameCount)
			{
				minFrame = player.TotalFrameCount;
			}
		}
		if (minFrame != null && minFrame.Value > 0)
		{
			foreach (PlayerController player in m_Players)
			{
				player.RemoveCommand((ulong)(minFrame.Value - 1));
			}
		}
	}

	/// <summary>
	/// 同期待機
	/// </summary>
	public void StandbySync()
	{
		if (NetworkClient.active)
		{
			LocalPlayer.CmdStandbySync();
		}
	}

	/// <summary>
	/// 同期完了
	/// </summary>
	public bool IsCompleateSync()
	{
		if (SyncCount == m_Players.Count)
		{
			SyncCount = 0;
			return true;
		}
		return false;
	}

	#endregion

	#region Matching

	/// <summary> デフォルトポート番号 </summary>
	public int DefaultNetworkPort { get; private set; }

	/// <summary> ルーム作成した? </summary>
	public bool IsCreatedMatch { get; private set; }

	/// <summary> ルーム参加した? </summary>
	public bool IsJoinedMatch { get; private set; }
	
	/// <summary> 無効なマッチID </summary>
	private List<ulong> m_IgnoreMatchId = new List<ulong>();

	/// <summary>
	/// マッチングメーカー無効化
	/// </summary>
	public void DisableMatchMaker()
	{
		if (matchMaker != null)
		{
			StopMatchMaker();
		}
	}

	/// <summary>
	/// ルーム作成
	/// </summary>
	public Coroutine CreateMatch()
	{
		if (!IsCreatedMatch && matchMaker != null && matchInfo == null)
		{
			System.DateTime now = System.DateTime.Now;
			matchName = now.Hour.ToString() + now.Minute.ToString() + now.Second.ToString();
			return matchMaker.CreateMatch(matchName, matchSize, true, "", "", "", 0, 0, OnMatchCreate);
		}
		return null;
	}

	/// <summary>
	/// ルーム作成通知
	/// </summary>
	public override void OnMatchCreate(bool success, string extendedInfo, UnityEngine.Networking.Match.MatchInfo match)
	{
		base.OnMatchCreate(success, extendedInfo, match);
		IsCreatedMatch = success;

		if (success)
			Debug.Log("Match create succeed");
		else
			Debug.LogWarning("Match create failed");
	}

	/// <summary>
	/// ルーム解散
	/// </summary>
	public Coroutine DestroyMatch()
	{
		if (matchMaker != null && matchInfo != null)
		{
			return matchMaker.DestroyMatch(matchInfo.networkId, matchInfo.domain, OnDestroyMatch);
		}
		return null;
	}

	/// <summary>
	/// ルーム解散通知
	/// </summary>
	public override void OnDestroyMatch(bool success, string extendedInfo)
	{
		base.OnDestroyMatch(success, extendedInfo);
		if (success)
		{
			IsCreatedMatch = false;
			ReleasePlayer();
			StopServer();
		}

		if (success)
			Debug.Log("Match destroy succeed");
		else
			Debug.LogWarning("Match destroy failed");
	}

	/// <summary>
	/// ルーム検索
	/// </summary>
	public Coroutine FindMatch()
	{
		if (matchMaker != null && matchInfo == null && matches == null)
		{
			return matchMaker.ListMatches(0, 20, "", false, 0, 0, OnMatchList);
		}
		return null;
	}

	/// <summary>
	/// ルーム検索中止
	/// </summary>
	public void StopFindMatch()
	{
		matches = null;
	}

	/// <summary>
	/// ルームインデックスリスト取得
	/// </summary>
	public int[] GetMatchIndices()
	{
		List<int> dest = new List<int>();
		for (int i = 0; i < matches.Count; i++)
		{
			if (matches[i].currentSize > 0 && !m_IgnoreMatchId.Contains((ulong)matches[i].networkId))
			{
				dest.Add(i);
			}
		}
		return dest.ToArray();
	}

	/// <summary>
	/// ルーム参加
	/// </summary>
	public Coroutine JoinMatch(int index = -1)
	{
		if (!IsJoinedMatch && matchMaker != null && matchInfo == null && matches != null)
		{
			if (index == -1)
			{
				int[] indices = GetMatchIndices();
				if (indices.Length > 0)
				{
					index = indices[0];
				}
				else
				{
					return null;
				}
			}

			if (matches.Count > index)
			{
				matchName = matches[index].name;
				matchSize = (uint)matches[index].maxSize;
				return matchMaker.JoinMatch(matches[index].networkId, "", "", "", 0, 0, OnMatchJoined);
			}
		}
		return null;
	}

	/// <summary>
	/// ルーム参加通知
	/// </summary>
	public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo match)
	{
		base.OnMatchJoined(success, extendedInfo, match);
		IsJoinedMatch = success;

		if (success)
			Debug.Log("Match join succeed");
		else
			Debug.LogWarning("Match join failed");
	}

	/// <summary>
	/// ルーム退室
	/// </summary>
	public Coroutine DropMatch()
	{
		if (IsJoinedMatch && matchInfo != null)
		{
			AddIgnoreMatchId((ulong)matchInfo.networkId);
			return matchMaker.DropConnection(matchInfo.networkId, matchInfo.nodeId, 0, OnDropMatch);
		}
		return null;
	}

	/// <summary>
	/// ルーム退室通知
	/// </summary>
	public void OnDropMatch(bool success, string extendedInfo)
	{
		if (success)
		{
			IsJoinedMatch = false;
			ReleasePlayer();
			StopClient();
		}

		if (success)
			Debug.Log("Match drop succeed");
		else
			Debug.LogWarning("Match drop failed");
	}

	/// <summary>
	/// マッチ完了?
	/// </summary>
	public bool IsMatchComplete
	{
		get { return matchSize == PlayerCount; }
	}

	/// <summary>
	/// オフライン?
	/// </summary>
	public bool IsOffline
	{
		get { return !NetworkServer.active && !NetworkClient.active && matchMaker == null; }
	}

	/// <summary>
	/// 無効なマッチIDを追加
	/// </summary>
	public void AddIgnoreMatchId(ulong id)
	{
		m_IgnoreMatchId.Add(id);
	}

	/// <summary>
	/// 無効なマッチIDを削除
	/// </summary>
	public void ClearIgnoreMatchId()
	{
		m_IgnoreMatchId.Clear();
	}

	#endregion
	
	#region Connect

	/// <summary>
	/// 切断通知
	/// </summary>
	public void OnDisconnected()
	{
		// マッチング以外での切断
		if (!GameManager.Instance.IsExistsScene("Matching"))
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
		}
	}

	/// <summary>
	/// 切断
	/// </summary>
	public Coroutine Disconnect()
	{
		return StartCoroutine(Disconnecting());
	}

	/// <summary>
	/// 切断
	/// </summary>
	private IEnumerator Disconnecting()
	{
		// ルーム解散
		if (IsCreatedMatch)
		{
			yield return DestroyMatch();
		}
		// ルーム退室
		if (IsJoinedMatch)
		{
			yield return DropMatch();
		}
		
		// サーバー停止
		if (NetworkServer.active)
		{
			StopHost();
		}
		// クライアント停止
		if (NetworkClient.active)
		{
			StopClient();
		}

		// マッチ無効化
		DisableMatchMaker();

		// プレイヤー破棄
		ReleasePlayer();

		IsCreatedMatch = false;
		IsJoinedMatch = false;

		OnDisconnected();
	}

	/// <summary>
	/// サーバーへの接続通知 (Client)
	/// </summary>
	public override void OnClientConnect(NetworkConnection conn)
	{
		Debug.Log("[Client] Connect");
		base.OnClientConnect(conn);
	}

	/// <summary>
	/// サーバーの切断通知 (Client)
	/// </summary>
	public override void OnClientDisconnect(NetworkConnection conn)
	{
		Debug.Log("[Client] Disconnect");
		base.OnClientDisconnect(conn);

		IsJoinedMatch = false;

		// クライアント停止
		StopClient();

		// マッチ無効化
		DisableMatchMaker();

		// プレイヤー破棄
		ReleasePlayer();

		OnDisconnected();
	}

	/// <summary>
	/// ネットワークエラー通知 (Client)
	/// </summary>
	public override void OnClientError(NetworkConnection conn, int errorCode)
	{
		Debug.Log("[Client] Network error");
		base.OnClientError(conn, errorCode);
	}

	/// <summary>
	/// クライアントの接続通知 (Server)
	/// </summary>
	public override void OnServerConnect(NetworkConnection conn)
	{
		Debug.Log("[Server] Connect");
		base.OnServerConnect(conn);
	}
	
	/// <summary>
	/// クライアントの切断通知 (Server)
	/// </summary>
	public override void OnServerDisconnect(NetworkConnection conn)
	{
		Debug.Log("[Server] Disconnect");
		base.OnServerDisconnect(conn);

		Disconnect();
	}

	/// <summary>
	/// ネットワークエラー通知 (Server)
	/// </summary>
	public override void OnServerError(NetworkConnection conn, int errorCode)
	{
		Debug.Log("[Server] Network error");
		base.OnServerError(conn, errorCode);
	}

	#endregion

	/// <summary>
	/// インスタンス
	/// </summary>
	public static NetworkGameManager Instance
	{
		get { return singleton as NetworkGameManager; }
	}
}
