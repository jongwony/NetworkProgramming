// 한 대의 단말에서 동작시킬 겨우에 정의한다.
//#define UNUSE_MATCHING_SERVER

using UnityEngine;
using System.Collections;
using System.Net;
using System.Threading;

// 타이틀 화면 시퀀스.
public class TitleControl : MonoBehaviour {

	public Texture	title_image = null;					// 타이틀 화면.

	public const bool	is_single = false;				// 싱글 플레이?(디버그용).

	// ================================================================ //

	public enum STEP {

		NONE = -1,

		WAIT = 0,			// 입력 대기.
		MATCHING,			// 매칭.
		WAIT_MATCHING,		// 매칭 대기.
		SERVER_START,		// 서버 대기 시작.
		SERVER_CONNECT,		// 게임 서버 접속.
		CLIENT_CONNECT,		// 클라이언트 끼리 접속.
		PREPARE,			// 각종 준비.
		CONNECTION,			// 각종 접속.

		GAME_START,			// 게임 시작.

		ERROR,				// 오류 발생.
		WAIT_RESTART,		// 오류에서 복귀하길 기다림.

		NUM,
	};

	public STEP				step      = STEP.NONE;
	public STEP				next_step = STEP.NONE;

	private float			step_timer = 0.0f;

	private MatchingClient	m_client = null;

	private Network			m_network = null;
	
	private string			m_serverAddress = "";

    // 계정 ID... DB를 쓸 예정
    //private string          accountID = "";

	// 호스트 플래그.
	private bool			m_isHost = false;

	// 오류 메시지.
	private string			m_errorMessage = ""; 
	

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		this.step      = STEP.NONE;
		this.next_step = STEP.WAIT;

		GlobalParam.get().fadein_start = true;

		if(!TitleControl.is_single) {

			this.m_serverAddress = "";
	
			// 호스트 이름을 얻는다.
			string hostname = Dns.GetHostName();
	
			// 호스트 이름에서 IP주소를 얻는다.
			IPAddress[]	adrList = Dns.GetHostAddresses(hostname);
			m_serverAddress = adrList[0].ToString();
	
			GameObject obj = GameObject.Find("Network");
			if (obj == null) {
				obj = new GameObject ("Network");
			}
	
			if (m_network == null) {
				m_network = obj.AddComponent<Network>();
				if (m_network != null) {
					DontDestroyOnLoad(m_network);
				}
			}
		}
	}
	
	void	Update()
	{
		// 오류 핸들링.
		NetEventHandling();

		// ---------------------------------------------------------------- //
		// 스텝 내 경과 시간 진행.

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 전환 시 초기화.

		while(this.next_step != STEP.NONE) {

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			switch(this.step) {
	
			case STEP.MATCHING:
			{
				int serverNode = -1;
				if (m_network != null) {
					serverNode = m_network.Connect(m_serverAddress, NetConfig.MATCHING_SERVER_PORT, Network.ConnectionType.Reliable);
					if (serverNode >= 0) {

						GameObject obj = new GameObject("MatchingClient");
						m_client = obj.AddComponent<MatchingClient>();
						if (m_client == null) {
							// 오류 강제 전환.
							m_errorMessage = "매칭을 시작할 수 없습니다.\n\n버튼을 누르세요.";
							this.step = STEP.ERROR;
						}
						m_client.SetServerNode(serverNode);

					}
					else {
						// 오류.
						m_errorMessage = "매칭 서버에\n접속할 수 없습니다.\n\n버튼을 누르세요.";
						this.step = STEP.ERROR;
					}
				}
				else {
					// 오류.
					m_errorMessage = "통신을 시작할 수 없습니다.\n\n버튼을 누르세요.";
					this.step = STEP.ERROR;
				}
		    }
			break;

			case STEP.SERVER_START:
				{
					m_network.ClearReceiveNotification();

					Debug.Log("Launch Listening socket.");
					// 같은 단말에서 실행할 수 있게 포트 번호를 다르게 한다.
					// 다른 단말에서 실행할 때는 포트 번호가 같은 것을 사용한다.
					int port = NetConfig.GAME_PORT + GlobalParam.get().global_account_id;
					bool ret = m_network.StartServer(port, NetConfig.PLAYER_MAX, Network.ConnectionType.Unreliable);
					if (m_isHost) {
						// 게임 서버 시작.
						int playerNum = m_client.GetMemberNum();
						ret &= m_network.StartGameServer(playerNum);
					}
					if (ret == false) {
						// 오류로 강제 전환.
						m_errorMessage = "게임 통신을 시작할 수 없습니다.\n\n버튼을 누르세요.";
						this.step = STEP.ERROR;
					}
				}
				break;

			case STEP.SERVER_CONNECT:
				{
					// 호스트 이름 획득.
					if (m_isHost) {
						string hostname = Dns.GetHostName();
						IPAddress[] adrList = Dns.GetHostAddresses(hostname);
						m_serverAddress = adrList[0].ToString();
					}
					// 게임 서버에 접속.
					Debug.Log("Connect to GameServer.");
					int serverNode = m_network.Connect(m_serverAddress, NetConfig.GAME_SERVER_PORT, Network.ConnectionType.Reliable);
					if (serverNode >= 0) {
						m_network.SetServerNode(serverNode);
					}
					else {
						// 오류로 강제 전환.
						m_errorMessage = "게임 서버와 통신할 수 없습니다.\n\n버튼을 누르세요.";
						this.step = STEP.ERROR;
					}
				}
				break;
				
			case STEP.CLIENT_CONNECT:
				{
					Debug.Log("Connect to host.");
					MatchingClient.MemberList[] list = m_client.GetMemberList();
					int playerNum = m_client.GetMemberNum();
					for (int i = 0; i < playerNum; ++i) {
						// 같은 단말에서 실행할 때는 전용으로 부여한 플레이어ID로 판별한다.
						// 다른 단말에서 실행할 때는 IP주소로 판별할 수 있다.
						if (m_client.GetPlayerId() == i) {
							// 자기 자신은 접속하지 않는다.
							continue;
						}
						if (list[i] == null) {
							continue;
						}
						// 같은 단말에서 실행할 수 있게 포트 번호를 다르게 한다.
						// 다른 단말에서 실행할 때는 포트 번호가 같은 것을 사용한다.
						int port = NetConfig.GAME_PORT + i;
						string memberAddress = list[i].endPoint.Address.ToString();
						int clientNode = m_network.Connect(memberAddress, port, Network.ConnectionType.Unreliable);

						if (clientNode >= 0) {
							m_network.SetClientNode(i, clientNode);
						}
						else {
							// 오류로 강제 전환.
							m_errorMessage = "게임을 시작할 수 없습니다.\n\n버튼을 누르세요.";
							this.step = STEP.ERROR;
						}
					}
				}
				break;

			case STEP.GAME_START:
				{
                        GlobalParam.get().fadein_start = true;
                        GlobalParam.get().playerNum = m_client.GetMemberNum();
                        Application.LoadLevel("scGame");
				}
				break;

				
			case STEP.WAIT_RESTART:
				{
					if (m_isHost) {
						m_network.StopGameServer();
					}
					m_network.StopServer();
					m_network.Disconnect();
				}
				break;
			}
			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step) {
				
			case STEP.MATCHING:
			{
				this.next_step = STEP.WAIT_MATCHING;
			}
			break;

			case STEP.WAIT_MATCHING:
			{
				if (m_client != null && m_client.IsFinishedMatching()) {
				
					GlobalParam.get().global_account_id = m_client.GetPlayerId();

					m_isHost = m_client.IsRoomOwner();

					if (m_isHost) {
						GlobalParam.get().is_host = true;
					}

					this.next_step = STEP.SERVER_START;
				}
			}
			break;

			case STEP.SERVER_START:
			{
			// 서버가 시작하길 기다린다.
				if (this.step_timer > 3.0f){
					this.next_step = STEP.SERVER_CONNECT;
				}
			}
			break;
				
			case STEP.SERVER_CONNECT:
			{
				this.next_step = STEP.CLIENT_CONNECT;
			}
			break;
				
			case STEP.CLIENT_CONNECT:
			{
				this.next_step = STEP.GAME_START;
			}
			break;

            case STEP.ERROR:
                {
                    this.next_step = STEP.WAIT_RESTART;
                }
                break;
		}

		// ---------------------------------------------------------------- //
	}

	void	OnGUI()
	{

		// 배경 이미지.
		int x = Screen.width/2 - 80;
		int y = Screen.height/2 - 20;


		if (this.step == STEP.WAIT_RESTART) {
			NotifyError();
			return;
		}

		if (m_client != null &&
			this.step >= STEP.WAIT_MATCHING && 
			this.step <= STEP.GAME_START) {
			m_client.OnGUIMatching();
		}
				
		if (this.step != STEP.WAIT) {
			return;
		}
        
        {
			GUIStyle style = new GUIStyle();
			style.fontSize = 14;
			style.fontStyle = FontStyle.Bold;
			style.normal.textColor = Color.black;

            //
            //GUI.Label(new Rect(x, y - 100, 160, 20), "ID 입력", style);
            //accountID = GUI.TextField(new Rect(x, y - 80, 160, 20), accountID);

            GUI.Label(new Rect(x, y-50, 160, 20), "매칭 서버 주소", style);
			m_serverAddress = GUI.TextField(new Rect(x, y-30, 160, 20), m_serverAddress);
		}
				
		if(GUI.Button(new Rect(x, y, 160, 20), "Connect to Server")) {
			this.next_step = STEP.MATCHING;
		}
	}

	// ================================================================ //
	
	
	public void NetEventHandling()
	{
		if (m_network == null) {
			return;
		}

		NetEventState state = m_network.GetEventState();
		
		if (state == null) {
			return;
		}
		
		switch (state.type) {
		case NetEventType.Connect:
			Debug.Log("[CLIENT]connect event handling:" + state.node);
			break;
			
		case NetEventType.Disconnect:
			Debug.Log("[CLIENT]disconnect event handling:" + state.node);
				if (this.step < STEP.SERVER_START) {
				m_errorMessage = "서버와 연결이 끊어졌습니다.\n\n버튼을 누르세요.";
				this.step = STEP.ERROR;
			}
			break;
		}
	}
	
	// 오류 알림.
	private void NotifyError()
	{
		GUIStyle style = new GUIStyle(GUI.skin.GetStyle("button"));
		style.normal.textColor = Color.white;
		style.fontSize = 25;
		
		float sx = 450;
		float sy = 200;
		float px = Screen.width / 2 - sx * 0.5f;
		float py = Screen.height / 2 - sy * 0.5f;
		
		if (GUI.Button (new Rect (px, py, sx, sy), m_errorMessage, style)) {
			this.step      = STEP.WAIT;
			this.next_step = STEP.NONE;
		}
	}
}
