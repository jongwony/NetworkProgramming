using UnityEngine;
using System;
using System.Collections;
using System.Net;




public enum TransportRequest
{
	Connect = 0,

	Disconnect,

	UserData,

}




public enum PacketId
{
	// 매칭용 패킷.
	MatchingRequest = 0,		// 매칭 요청 패킷.
	MatchingResponse, 			// 매칭 응답 패킷.
	SearchRoomResponse, 		// 방 검색 응답.
	StartSessionNotify, 		// 게임 시작 통지.

	Max,
}



public enum MatchingRequestId
{
	CreateRoom = 0,
	JoinRoom,
	StartSession,
	SearchRoom,

	Max,
}

public enum MatchingResult 
{
	Success = 0,


	RoomIsFull,
	MemberIsFull,
	RoomIsGone,

}

public struct PacketHeader
{
	// 패킷 종별.
	public PacketId 	packetId;
}

//
// 매칭 요청
//
public struct MatchingRequest
{
	public int					version;	// 패킷ID.
	public MatchingRequestId	request;	// 요청 내용.
	public int 					roomId;		// 요청 방 ID.
	public string				name;		// 생성할 방이름.
	public int					level;		// 레벨 지정.
	
	public const int roomNameLength = 32;	// 방 이름 길이.
}

//
// 매칭 응답.
//
public struct MatchingResponse
{
	// 요청 결과.
	public MatchingResult		result;
	
	// 요청 내용.
	public MatchingRequestId	request;

	// 응답 방ID.
	public int 					roomId;

	// 
	public string			 	name;

	// 참가인원.
	public int					members;

	// 방 이름 길이.
	public const int roomNameLength = 32;
}

//
// 방 정보.
//
public struct RoomInfo
{
	// 방ID.
	public int 					roomId;
	
	// 방 이름.
	public string				name;

	//
	public int					members;

	// 방 이름 길이.
	public const int roomNameLength = 32;
}

//
// 방 검색 결과.
//
public struct SearchRoomResponse
{
	// 검색한 방 수.
	public int			roomNum;

	// 방 정보.
	public RoomInfo[]	rooms;
}

//
// 접속할 곳의 정보.
//
public struct EndPointData
{
	public string		ipAddress;
	
	public int 			port;

	// IP 주소의 길이.
	public const int ipAddressLength = 32;
}

//
// 세션 정보.
//
public struct SessionData
{
	public MatchingResult 	result;

	public int				playerId;		// 동일 단말에서 동작시킬 때 사용합니다.

	public int				members;

	public EndPointData[]	endPoints;
}

