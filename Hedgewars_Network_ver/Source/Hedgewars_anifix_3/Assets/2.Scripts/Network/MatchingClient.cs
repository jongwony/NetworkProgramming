// 한 대의 단말에서 동작시킬 경우에 정의한다.
//#define UNUSE_MATCHING_SERVER

using UnityEngine;						// MonoBehaviour
using System.Linq;						// Enumerable.Repeat
using System.Collections.Generic;		// Dictionary 자료구조 
using System.Net;						// 소켓 주소 정보 

public class MatchingClient : MonoBehaviour
{
	// 매칭할 수 있는 최대 방 수.
	private const int 	maxRoomNum = 4;

	// 참가할 수 있는 최대 플레이어 수.
	private const int 	maxMemberNum = NetConfig.PLAYER_MAX;

    // 방 구성 요소
    private Dictionary<int, RoomContent> m_rooms = new Dictionary<int, RoomContent>();
    private RoomContent joinedRoom = new RoomContent();
    private class RoomContent
	{
		public int 		node = -1;

		public int 		roomId = -1;

		public string	roomName = "";

		public int[]	members = Enumerable.Repeat(-1, maxMemberNum).ToArray();
	}

    // 플레이어 리스트 관리
    private MemberList[] sessionMembers = new MemberList[maxMemberNum];
    private int m_memberNum = 0;
    public class MemberList
    {
        public int node = -1;

        public string accountID = "";

        public IPEndPoint endPoint;
    }



    // 게임오브젝트에 넣을 스크립트
    private Network 	network_ = null;

	// for client.
	private string		roomName = "";

	private	bool		isRoomOwner = false;
        
	private int				playerId = -1;

	private float			timer = 0.0f;

	private	int				serverNode = 0;

    // 플래그 관리 
    private State matchingState = State.Idle;
    private enum State
	{
		Idle = 0,
		MatchingServer,
		RoomCreateRequested,
		RoomSearchRequested,
		RoomJoinRequested,
		WaitMembers,
		StartSessionRequested,
		StartSessionNotified,
		MatchingEnded,

		RoomCreateFailed,
		RoomJoinFailed,

	}


	// Use this for initialization
	void Start()
	{
		GameObject obj = GameObject.Find("Network");
		network_ = obj.GetComponent<Network>();

        // 이벤트 감지
        if (network_ != null) {
			network_.RegisterReceiveNotification(PacketId.MatchingResponse, this.OnReceiveMatchingResponse);
			network_.RegisterReceiveNotification(PacketId.SearchRoomResponse, this.OnReceiveSearchRoom);
			network_.RegisterReceiveNotification(PacketId.StartSessionNotify, this.OnReceiveStartSession);
		}

		// 처음은 모든 방 검색.
		RequestSearchRoom(-1);

		matchingState = State.Idle;

	}

	// Update is called once per frame
	void Update()
	{
		switch (matchingState) {

		case State.RoomCreateRequested:
			WaitMembers(joinedRoom);
			break;

		case State.RoomJoinRequested:
			WaitMembers(joinedRoom);
			break;
		}

		timer += Time.deltaTime;
	}


	void WaitMembers(RoomContent room)
	{
		if (timer > 5.0f) {
			RequestSearchRoom(room.roomId);
			timer = 0.0f;
		}
	}
	
	public void OnGUIMatching()
	{
		switch (matchingState) {
		case State.Idle:
			OnGUISelectRoomType();
			break;

		case State.RoomCreateRequested:
			OnGUIRoomCreated();
			break;

		case State.RoomJoinRequested:
			DrawRoomInfo(false);
			break;

		case State.MatchingEnded:
			DrawRoomInfo(false);
			break;

		case State.RoomCreateFailed:
			NotifyError("방을 생성할 수 없습니다.");
			break;

		case State.RoomJoinFailed:
			NotifyError("방에 참가할 수 없습니다.");
			break;

		}
	}

	void OnGUISelectRoomType()
	{
		int px = Screen.width/2;
		int py = Screen.height/2 - 50;
		int sx = 180;
		int sy = 30;

		{
			GUIStyle style = new GUIStyle ();
			style.fontSize = 16;
			style.fontStyle = FontStyle.Bold;
			style.normal.textColor = Color.black;
			
			GUI.Label (new Rect (px - 100, py - 20, 160, 20), "방 이름", style);
			roomName = GUI.TextField (new Rect (px - 100, py, sx, sy), roomName);
		}

		if (GUI.Button(new Rect(px+90, py, sx-60, sy), "방 만들기")) {
			RequestCreateRoom(roomName);
			matchingState = State.RoomCreateRequested;
		}
		
		if (GUI.Button(new Rect(px+90, py+40, sx-60, sy), "방 찾기")) {
			RequestSearchRoom(-1);
		}

		DrawRoomInfo(true);
	}

	void OnGUIRoomCreated()
	{
		int px = Screen.width/2;
		int py = Screen.height/2 - 50;

		if (GUI.Button(new Rect(px-100, py, 200, 30), "게임 시작")) {
			RequestStartSession(joinedRoom);
		}

		DrawRoomInfo(false);
	}

	private void DrawRoomInfo(bool enable)
	{
		int px = Screen.width/2 - 150;
		int py = Screen.height/2 + 70;
		int sy = 30;

		{
			GUIStyle style = new GUIStyle();
			style.fontSize = 16;
			style.fontStyle = FontStyle.Bold;
			style.normal.textColor = Color.black;

			if (matchingState == State.Idle) {
				GUI.Label(new Rect(px+30, py-20, 160, 20), "발견한 방", style);
			}
			else if (matchingState == State.RoomCreateRequested || 
			         matchingState == State.RoomJoinRequested ||
			         matchingState == State.MatchingEnded) {
				GUI.Label(new Rect(px+110, py-20, 160, 20), "방의 상황", style);
			}

		}

		int index = 1;
		foreach (RoomContent room in m_rooms.Values) {
			string label = "방" + index + " : " + room.roomName + 
				" (앞으로: " + (maxMemberNum - room.members[0]).ToString() + "명)";
			if (GUI.Button(new Rect(px, py, 300, sy), label)) {
				if (enable) {
					RequestJoinRoom(room);
				}
			}
			py += sy + 10;
			++index;
		}
	}

	public bool IsFinishedMatching()
	{
		return (matchingState == State.MatchingEnded);
	}

	public bool	IsRoomOwner()
	{
		return isRoomOwner;
	}

	public void SetServerNode(int node)
	{
		serverNode = node;
	}

	public int GetPlayerId()
	{
		return playerId;
	}

	public MemberList[] GetMemberList()
	{
		return sessionMembers;
	}

	public int GetMemberNum()
	{
		return m_memberNum;
	}

	//
	// 클라이언트 측 처리.
	//

	void RequestCreateRoom(string name)
	{
		MatchingRequest request = new MatchingRequest();

		request.request = MatchingRequestId.CreateRoom;
		request.version = NetConfig.SERVER_VERSION;
		request.name = name;
		request.roomId = -1;

		MatchingRequestPacket packet = new MatchingRequestPacket(request);

		if (network_ != null) {
			network_.SendReliable<MatchingRequest>(serverNode, packet);
		}
	}

	void RequestSearchRoom(int roomId)
	{
		MatchingRequest request = new MatchingRequest();

		request.request = MatchingRequestId.SearchRoom;
        request.version = NetConfig.SERVER_VERSION;
		request.name = "";
		request.roomId = roomId;

		MatchingRequestPacket packet = new MatchingRequestPacket(request);
		
		network_.SendReliable<MatchingRequest>(serverNode, packet);
	}

	void RequestJoinRoom(RoomContent room)
	{
		MatchingRequest request = new MatchingRequest();
		
		request.request = MatchingRequestId.JoinRoom;
        request.version = NetConfig.SERVER_VERSION;
        request.name = "";
		request.roomId = room.roomId;

		if (network_ != null) {
			MatchingRequestPacket packet = new MatchingRequestPacket(request);
		
			network_.SendReliable<MatchingRequest>(serverNode, packet);
		}

		string str = "RequestJoinRoom:" + room.roomId;
		Debug.Log(str);
	}
	
	void RequestStartSession(RoomContent room)
	{
		MatchingRequest request = new MatchingRequest();
		
		request.request = MatchingRequestId.StartSession;
        request.version = NetConfig.SERVER_VERSION;
        request.name = room.roomName;
		request.roomId = room.roomId;

		Debug.Log("Request start session[room:" + room.roomName + " " + room.roomId + "]");

		if (network_ != null) {
			MatchingRequestPacket packet = new MatchingRequestPacket(request);
		
			network_.SendReliable<MatchingRequest>(serverNode, packet);
		}
	}

	//
	// 패킷 수신 처리.
	//
	
	void OnReceiveMatchingResponse(int node, PacketId id, byte[] data)
	{
		MatchingResponsePacket packet = new MatchingResponsePacket(data);
		MatchingResponse response = packet.GetPacket();

		string str = "ReceiveMatchingResponse:" + response.request;
		Debug.Log(str);

		switch (response.request) {
		case MatchingRequestId.CreateRoom:
			CreateRoomResponse(response.result, response.roomId);
			break;

		case MatchingRequestId.JoinRoom:
			JoinRoomResponse(response.result, response.roomId) ;
			break;

		default:
			Debug.Log("Unknown request.");
			break;
		}
	}

	void OnReceiveSearchRoom(int node, PacketId id, byte[] data)
	{
		Debug.Log("ReceiveSearchRoom");

		SearchRoomPacket packet = new SearchRoomPacket(data);
		SearchRoomResponse response = packet.GetPacket();

		string str = "Created room num:" + response.roomNum;
		Debug.Log(str);

		SearchRoomResponse(response.roomNum, response.rooms);
	}

	void OnReceiveStartSession(int node, PacketId id, byte[] data)
	{
		Debug.Log("ReceiveStartSession");

		SessionPacket packet = new SessionPacket(data);
		SessionData response = packet.GetPacket();

		playerId = response.playerId;

		SetSessionMembers(response.result, response.members, response.endPoints);

		matchingState = State.MatchingEnded;
	}

	void CreateRoomResponse(MatchingResult result, int roomId)
	{
		Debug.Log("CreateRoomResponse");

		if (result == MatchingResult.Success) {
			isRoomOwner = true;
			joinedRoom.roomId = roomId;
			matchingState = State.RoomCreateRequested;

			RequestSearchRoom(roomId);

			string str = "Create room is success [id:" + roomId + "]";
			Debug.Log(str);
		}
		else {
			matchingState = State.RoomCreateFailed;

			Debug.Log("Create room is failed.");
		}
	}

	void SearchRoomResponse(int roomNum, RoomInfo[] rooms)
	{
		m_rooms.Clear();

		for (int i = 0; i < roomNum; ++i) {
			RoomContent r = new RoomContent();

			r.roomId = rooms[i].roomId;
			r.roomName = rooms[i].name;
			r.members[i] = rooms[i].members;

			m_rooms.Add(rooms[i].roomId, r);

			string str = "Room name[" + i + "]:" + rooms[i].name + 
				" [id:" + rooms[i].roomId + ":" + rooms[i].members +"]";
			Debug.Log(str);
		}
	}

	void JoinRoomResponse(MatchingResult result, int roomId) 
	{
		Debug.Log("JoinRoomResponse");

		if (result == MatchingResult.Success) {
			joinedRoom.roomId = roomId;
			matchingState = State.RoomJoinRequested;

			string str = "Join room was success [id:" + roomId + "]";
			Debug.Log(str);
		}
		else {
			matchingState = State.RoomJoinFailed;
			RequestSearchRoom(-1);
			Debug.Log("Join room was failed.");
		}
	}

	void SetSessionMembers(MatchingResult result, int memberNum, EndPointData[] endPoints)
	{
		Debug.Log("StartSessionNotify");

		string str = "MemberNum:" + memberNum + " result:" + result;
		Debug.Log(str);

		m_memberNum = memberNum;

		for (int i = 0; i < memberNum; ++i) {

			MemberList member = new MemberList();

			member.node = i;
            //.accountID = ;
			member.endPoint = new IPEndPoint(IPAddress.Parse(endPoints[i].ipAddress), endPoints[i].port);

			sessionMembers[i] = member;

			str = "member[" + i + "]:" + member.endPoint.Address.ToString() + " : " + endPoints[i].port;
			Debug.Log(str);
		}
	}

	// 오류 알림.
	private void NotifyError(string message)
	{
		GUIStyle style = new GUIStyle(GUI.skin.GetStyle("button"));
		style.normal.textColor = Color.white;
		style.fontSize = 20;
		
		float sx = 400;
		float sy = 200;
		float px = Screen.width / 2 - sx * 0.5f;
		float py = Screen.height / 2 - sy * 0.5f;
		
		if (GUI.Button (new Rect (px, py, sx, sy), message, style)) {
			matchingState = State.Idle;
		}
	}
}

