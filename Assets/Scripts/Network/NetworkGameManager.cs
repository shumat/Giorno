using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkGameManager : MonoBehaviour
{
	private static NetworkGameManager m_Instance = null;

	private NetworkManager m_NetworkManager = null;

	private MatchingManager m_MatchingManager = null;

	private List<PlayerController> m_Players = new List<PlayerController>();

	protected void Awake()
	{
		if (m_Instance != null)
		{
			Destroy(m_Instance);
		}
		m_Instance = this;

		m_NetworkManager = GetComponent<NetworkManager>();

		m_MatchingManager = new MatchingManager();
		m_MatchingManager.SetNetworkManager(m_NetworkManager);
	}

	public void AddPlayerController(PlayerController pc)
	{
		m_Players.Add(pc);
	}

	private void MatchEnd()
	{
		m_Players.Clear();
	}

	private void OnGUI()
	{
		bool isLocalHost = NetworkServer.active && NetworkClient.active;
		bool isLocalClient = !NetworkServer.active && NetworkClient.active;

		int width = 200;
		int height = 30;
		int y = 0;

		if (!isLocalClient)
		{
			if (GUI.Button(new Rect(0, y++ * height, width, height), isLocalHost ? "Stop Host" : "Start Host"))
			{
				if (isLocalHost)
				{
					MatchEnd();
					m_MatchingManager.StopHost();
				}
				else
				{
					m_MatchingManager.StartHost();
				}
			}
		}
		if (!isLocalHost)
		{
			if (GUI.Button(new Rect(0, y++ * height, width, height), isLocalClient ? "Stop Client" : "StartClient"))
			{
				if (isLocalClient)
				{
					MatchEnd();
					m_MatchingManager.StopClient();
				}
				else
				{
					m_MatchingManager.StartClient();
				}
			}
		}
		
		if (GUI.Button(new Rect(0, y++ * height, width, height), "StartMatchMaker"))
		{
			m_MatchingManager.StartMatchMaker();
		}
		
		if (GUI.Button(new Rect(0, y++ * height, width, height), "DisableMatchMaker"))
		{
			m_MatchingManager.DisableMatchMaker();
		}
		
		if (GUI.Button(new Rect(0, y++ * height, width, height), "CreateMatch"))
		{
			m_MatchingManager.CreateMatch();
		}

		if (GUI.Button(new Rect(0, y++ * height, width, height), "DestroyMatch"))
		{
			m_MatchingManager.DestroyMatch();
		}
		
		if (GUI.Button(new Rect(0, y++ * height, width, height), "FindMatch"))
		{
			m_MatchingManager.FindMatch();
		}

		if (GUI.Button(new Rect(0, y++ * height, width, height), "JoinMatch"))
		{
			m_MatchingManager.JoinMatch();
		}
	}

	public bool IsReadyUpdate(uint frame)
	{
		foreach (PlayerController player in m_Players)
		{
			if (!player.IsReadyUpdate(frame))
			{
				return false;
			}
		}
		return true;
	}

	public PlayerController[] GetPlayers()
	{
		return m_Players.ToArray();
	}

	public int PlayerCount
	{
		get { return m_Players.Count; }
	}

	public static NetworkGameManager Instance
	{
		get { return m_Instance; }
	}
}
