using UnityEngine;
using System.Collections.Generic;
using System;

public class GameServer : MonoBehaviour {

	// 게임 서버 버전.
	public const int			SERVER_VERSION = 1; 	

	// 통신 모듈 컴포넌트.
	Network						network_ = null;

	// 세션 관리 정보(노드번호)와 플레이어 ID 연결.
	Dictionary<int, int>		m_nodes = new Dictionary<int, int>();

	private int					m_playerNum = 0;
	
	private int 				m_currentPartyMask = 0;

	void Awake () {
		GameObject obj = new GameObject("Network-GameServer");
		network_ = obj.AddComponent<Network>();

		if (network_ != null) {
			DontDestroyOnLoad(network_);

            // 초기 동기화 정보 수신 함수 등록
            network_.RegisterReceiveNotification(PacketId.GameSyncInfo, this.OnReceiveGameSyncPacket);
			// HP 정보 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.HpData, this.OnReceiveReflectionPacket);
			// 대미지양 정보 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.DamageData, this.OnReceiveReflectionPacket);
			// 대미지 알림 정보 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.DamageNotify, this.OnReceiveReflectionPacket);		
			// 채팅 메시지 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.ChatMessage, this.OnReceiveReflectionPacket);

            // 캐릭터 이동 좌표
		}

	}
	
	// Update is called once per frame
	void Update () {

		// 이벤트 핸들링.
		EventHandling();
	}


	public bool StartServer(int playerNum)
	{
		Debug.Log("Gameserver launched.");

		if (network_ == null) {
			Debug.Log("GameServer start fail.");
			return false; 
		}

		// 참가 인원.
		m_playerNum = playerNum;

		// 참가 플레이어 마스크.
		for (int i = 0; i < m_playerNum; ++i) {
			m_currentPartyMask |= 1 << i;
		}
        Debug.Log("PartyMask:" + Convert.ToString(m_currentPartyMask));

        return network_.StartServer(NetConfig.GAME_SERVER_PORT, NetConfig.PLAYER_MAX, Network.ConnectionType.Reliable);
	}


	public void StopServer()
	{
		if (network_ == null) {
			Debug.Log("GameServer is not started.");

			return;
		}

		network_.StopServer();

		Debug.Log("Gameserver shutdown.");
	}

	// ================================================================ //

    public void OnReceiveGameSyncPacket(int node, PacketId id, byte[] data)
    {
        GameSyncPacket packet = new GameSyncPacket(data);
        GameSyncInfo info = packet.GetPacket();

        Debug.Log("[SERVER] Receive Init packet");

        // 게임서버에서 난수 동기화 작업
        TimeSpan ts = new TimeSpan(DateTime.Now.Ticks);
        double seconds = ts.TotalSeconds;
        info.seed = (int)((long)seconds - (long)(seconds / 1000.0) * 1000);
        Debug.Log("Seed: " + info.seed);

        // 세션 관리 정보와 플레이어 글로벌 ID를 연결
        info.members = new CharacterID[NetConfig.PLAYER_MAX];
        for(int i = 0; i < NetConfig.PLAYER_MAX; i++)
        {
            info.members[i].globalId = i;
            if (!m_nodes.ContainsKey(node))
            {
                m_nodes.Add(node, info.members[i].globalId);
            }
        }
    }


	public void OnReceiveReflectionPacket(int node, PacketId id, byte[] data)
	{
		if (network_ != null) {
			Debug.Log("[SERVER]OnReceiveReflectionData from node:" + node);
			network_.SendReliableToAll(id, data);
		}
	}


	// ================================================================ //

	private void DisconnectClient(int node)
	{
		Debug.Log("[SERVER]DisconnectClient");
		
		network_.Disconnect(node);

		if (m_nodes.ContainsKey(node) == false) {
			return;
		}

		// 현재 접속 중인 클라이언트의 플래그를 반전시킨다.
		int gid = m_nodes[node];
		m_currentPartyMask &= ~(1 << gid);
	}

	// ================================================================ //

	public void EventHandling()
	{
		NetEventState state = network_.GetEventState();

		if (state == null) {
			return;
		}
				
		switch (state.type) {
		case NetEventType.Connect:
			Debug.Log("[SERVER]NetEventType.Connect");
			break;
			
		case NetEventType.Disconnect:
			Debug.Log("[SERVER]NetEventType.Disconnect");
			DisconnectClient(state.node);
			break;
		}
	}

	// ================================================================ //
    /*
    // 이름으로 글로벌 인덱스 얻기
    private int getGlobalIdFromName(string name)
    {
        AccountData data = AccountManager.getInstance().getAccountData(name);
        return data.global_index;
    }
    */
}
