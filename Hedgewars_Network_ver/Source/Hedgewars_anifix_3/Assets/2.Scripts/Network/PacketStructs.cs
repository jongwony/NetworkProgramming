using UnityEngine;


public enum PacketId
{
	// 매칭용 패킷.
	MatchingRequest = 0,		// 매칭 요청 패킷.
	MatchingResponse, 			// 매칭 응답 패킷.
	SearchRoomResponse, 		// 방 검색 응답.
	StartSessionNotify, 		// 게임 시작 통지.

	// 게임용 패킷.
	GameSyncInfo,				// 게임전 동기화 정보.
	CharacterData,				// 캐릭터 좌표 정보.
	AttackData,					// 캐릭터 공격 정보.
	ItemData,					// 아이템 획득/ 폐기 정보.
	UseItem,					// 아이템 사용 정보.
	HpData,						// HP 통지.
	DamageData,					// 호스트에 대미지 통지.
	DamageNotify,				// 모든 단말에 대미지양을 통지.
	ChatMessage,				// 채팅 메시지.

	Max,
}


public enum MatchingRequestId
{
	CreateRoom = 0,
	JoinRoom,
	StartSession,
	SearchRoom,
	BackRoom,

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
	// 패킷 구별.
	public PacketId 	packetId;
}

//
// 매칭 요청.
//
public struct MatchingRequest
{
	public int					version;	// 패킷ID.
	public MatchingRequestId	request;	// 요청 내용.
	public int 					roomId;		// 요청 방 ID.
	public string				name;		// 생성할 방 이름.
	
	public const int roomNameLength = 32;	// 방 이름의 길이.
}

//
// 매칭 응답.
//
public struct MatchingResponse
{
	public MatchingResult		result;         // 요청 결과.
    public MatchingRequestId	request;        // 요청 내용.
    public int 					roomId;         // 응답 방 ID.
    public string			 	name;

	public int					members;        // 참가 인원.

    // 방 이름 길이.
    public const int roomNameLength = 32;
}

//
// 방 정보.
//
public struct RoomInfo
{
	public int 					roomId;         // 요청 방 ID.
    public string				name;           // 생성할 방 이름.
    public int					members;        // 참가 인원 

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

	// 방 정보..
	public RoomInfo[]	rooms;
}

//
// 접속처 정보.
//
public struct EndPointData
{
	public string		ipAddress;	
	public int 			port;

	// IP 어드레스 길이.
	public const int ipAddressLength = 32;
}

//
// 세션 정보.
//
public struct SessionData
{
	public MatchingResult 	result;
	public int				playerId;       // GlobalParam에 전달되는 요소
    public int				members;        // 참가 인원
    public EndPointData[]	endPoints;      // 접속된 IP주소들
}


//
//
// 게임용 패킷 데이터 정의.
//
//


//
// 게임전 동기화 정보.
//
public struct CharacterID
{
	public int			    globalId;	// 캐릭터의 글로벌 ID.
}

//
// 전원의 동기화 정보.
//
public struct GameSyncInfo
{
    public int              seed;		// 동기화할 난수의 시드.
	public CharacterID[]	members;	// 동기화할 멤버 정보.
}


//
// 아이템 획득정보.
//
public struct ItemData
{
	public string 		itemId;		// 아이템 식별자.
	public int			state;		// 아이템의 획득 상태.
	public string 		ownerId;	// 소유자 ID.

	public const int 	itemNameLength = 32;		// 아이템 이름의 길이.
	public const int 	charactorNameLength = 64;	// 캐릭터 ID의 길이.
}


//
// 캐릭터 좌표 정보.
//
public struct CharacterCoord
{
	public float	x;		// 캐릭터의 x좌표.
	public float	y;		// 캐릭터의 y좌표.
	
	public CharacterCoord(float x, float y)
	{
		this.x = x;
		this.y = y;
	}
	public Vector3	ToVector3()
	{
		return(new Vector3(this.x, this.y, 0.0f));
	}
	public static CharacterCoord	FromVector3(Vector3 v)
	{
		return(new CharacterCoord(v.x, v.y));
	}
	
	public static CharacterCoord	Lerp(CharacterCoord c0, CharacterCoord c1, float rate)
	{
		CharacterCoord	c = new CharacterCoord();
		
		c.x = Mathf.Lerp(c0.x, c1.x, rate);
		c.y = Mathf.Lerp(c0.y, c1.y, rate);
		
		return(c);
	}
}

//
// 캐릭터의 이동 정보.
//
public struct CharacterData
{
	public int 			    characterId;	// 캐릭터 ID.
	public int 				index;			// 위치 좌표의 인덱스.
	public int				dataNum;		// 좌표 데이터 수.
	public CharacterCoord[]	coordinates;	// 좌표 데이터.
}

//
// 캐릭터의 공격 정보.
//
public struct AttackData
{
	public int      		characterId;    // 캐릭터 ID.
    public float            fireForce;      // 발사 힘
    public Quaternion       fireAngle;      // 발사 각도
    public CharacterCoord   fireCoord;      // 발사 좌표
}

//
// 데미지양 정보.
//
public struct DamageData
{
	public string 			target;			// 공격받은 캐릭터 ID.
	public int	 			attacker;		// 공격한 어카운트 ID.
	public float			damage;			// 대미지양.

	public const int 		characterNameLength = 64;	// 캐릭터 ID 길이.
}

//
// 캐릭터 HP 정보.
//
public struct HpData
{
	public int 			    characterId;	// 캐릭터 ID.
	public float			hp;				// HP.
}

//
// 아이템 사용 정보.
//
public struct ItemUseData
{
	public int		itemFavor;	// 아이템 효과.
	public string	targetId;	// 효과를 적용할 캐릭터 ID.
	public string	userId;		// 아이템을 사용할 캐릭터 ID.

	public int		itemCategory;	// 아이템 효과 종류.

	public const int characterNameLength = 64;	// 캐릭터 ID 길이.
}

//
// 채팅 메시지.
//
public struct ChatMessage
{
	public string		characterId; // 캐릭터 ID.
	public string		message;	 // 채팅 메시지.
	
	public const int 	characterNameLength = 64;	// 캐릭터 ID 길이.
	public const int	messageLength = 64;
}

