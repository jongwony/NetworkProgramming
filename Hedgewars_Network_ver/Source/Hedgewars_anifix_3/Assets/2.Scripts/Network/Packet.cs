
///
/// <summary>
/// 
/// 패킷을 정의하는 곳
/// IPacket을 상속받으며 인터페이스 제네릭 안에 해당 구조체로
/// 고유한 패킷 구별을 한다.
/// 
/// 각 패킷은 생성자(시리얼라이즈 디시리얼라이즈)와
/// 인터페이스 상속내용
/// 고유 시리얼라이즈 이너클래스가 존재한다.
/// 
/// 이너클래스에는 시리얼라이즈와 디시리얼라이즈 두개의 메소드가 있다.
/// 
/// </summary>
/// 


//
// 매칭 패킷 정의.
//

// 매칭 요청 패킷 정의.
public class MatchingRequestPacket : IPacket<MatchingRequest>
{
    // 고유한 패킷 시리얼라이저 이너클래스
    class MatchingRequestSerializer : Serializer
	{
        // 1바이트 불리언에 저장
        // MatchingRequest 구조체 순서대로 시리얼라이즈 한다.
        // 하나라도 시리얼라이즈가 false가 되면 순서에 어긋나므로 패킷을 버린다
        public bool Serialize(MatchingRequest packet)
		{
			bool ret = true;

			ret &= Serialize(packet.version);
			int request = (int)packet.request;
			ret &= Serialize(request);
			ret &= Serialize(packet.roomId);
			ret &= Serialize(packet.name, MatchingRequest.roomNameLength);
			
			return ret;
		}		
		//
		public bool Deserialize(ref MatchingRequest element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;

            // 버전 정보
            ret &= Deserialize(ref element.version);

            // 요청 내용
            int request = 0;
			ret &= Deserialize(ref request);
			element.request = (MatchingRequestId) request;
            
            // 응답 방 ID
            ret &= Deserialize(ref element.roomId);

            // 방 이름
            ret &= Deserialize(ref element.name, MatchingRequest.roomNameLength);
			
			return ret;
		}
	}

    // 패킷 데이터 구조체
    MatchingRequest m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public MatchingRequestPacket(MatchingRequest data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public MatchingRequestPacket(byte[] data)
	{
		MatchingRequestSerializer serializer = new MatchingRequestSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
    // 인터페이스 부분
	public PacketId	GetPacketId()
	{
		return PacketId.MatchingRequest;
	}
	
	public MatchingRequest	GetPacket()
	{
		return m_packet;
	}

    public byte[] GetData()
	{
		MatchingRequestSerializer serializer = new MatchingRequestSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

// 매칭 요청 패킷 정의.
public class MatchingResponsePacket : IPacket<MatchingResponse>
{
	class MatchingResponseSerializer : Serializer
	{
		//
		public bool Serialize(MatchingResponse packet)
		{
			bool ret = true;

            // 요청 결과 enum -> int
            int result = (int)packet.result;
			ret &= Serialize(result);

            // 요청 내용 enum -> int
            int request = (int)packet.request;
			ret &= Serialize(request);

            // 응답 방 ID
            ret &= Serialize(packet.roomId);

            // 방 이름 string(name, size)
            ret &= Serialize(packet.name, MatchingResponse.roomNameLength);
            
            // 참가 인원
            ret &= Serialize(packet.members);
			
			return ret;
		}
		
		//
		public bool Deserialize(ref MatchingResponse element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}
		
			bool ret = true;

            // 요청 결과 int -> enum
            int result = 0;
			ret &= Deserialize(ref result);
			element.result = (MatchingResult) result;

            // 요청 내용 int -> enum
            int request = 0;
			ret &= Deserialize(ref request);
			element.request = (MatchingRequestId) request;

            // 응답 방 ID
            ret &= Deserialize(ref element.roomId);
            
            // 방 이름 string(name, size)
            ret &= Deserialize(ref element.name, MatchingResponse.roomNameLength);

            // 참가 인원
            ret &= Deserialize(ref element.members);
			
			return ret;
		}
	}
	
	// 패킷 데이터 구조체.
	MatchingResponse	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public MatchingResponsePacket(MatchingResponse data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public MatchingResponsePacket(byte[] data)
	{
		MatchingResponseSerializer serializer = new MatchingResponseSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	public PacketId	GetPacketId()
	{
		return PacketId.MatchingResponse;
	}
	
	public MatchingResponse	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		MatchingResponseSerializer serializer = new MatchingResponseSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

// 방 검색 결과 패킷 정의.
public class SearchRoomPacket : IPacket<SearchRoomResponse>
{
	class SearchRoomSerializer : Serializer
	{
		//
		public bool Serialize(SearchRoomResponse packet)
		{
			bool ret = true;

            // 검색한 방 수
            ret &= Serialize(packet.roomNum);

            // 구조체를 시리얼라이즈하는 것은 정의하지 않았으므로(할 수가 없다)
            // 방 수만큼 RoomInfo 구조체 성분을 시리얼라이즈
            for (int i = 0; i < packet.roomNum; ++i) {
                // RoomInfo.roomId
                ret &= Serialize(packet.rooms[i].roomId);
                // RoomInfo.name
                ret &= Serialize(packet.rooms[i].name, MatchingResponse.roomNameLength);
                // RoomInfo.members
                ret &= Serialize(packet.rooms[i].members);
			}
			
			return ret;
		}
		
		//
		public bool Deserialize(ref SearchRoomResponse element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;

            // 검색한 방 수
            ret &= Deserialize(ref element.roomNum);

            // 구조체를 디시리얼라이즈하는 것은 정의하지 않았으므로(할 수가 없다)
            // 방 수만큼 RoomInfo 구조체 성분을 디시리얼라이즈
            element.rooms = new RoomInfo[element.roomNum];
			for (int i = 0; i < element.roomNum; ++i) {
                // 시리얼라이즈 했던 순서대로 디시리얼라이즈
                // RoomInfo.roomId
                ret &= Deserialize(ref element.rooms[i].roomId);
                // RoomInfo.name
                ret &= Deserialize(ref element.rooms[i].name, MatchingResponse.roomNameLength);
                // RoomInfo.members
                ret &= Deserialize(ref element.rooms[i].members);
			}

			return ret;
		}
	}
	
	// 패킷 데이터 구조체.
	SearchRoomResponse	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public SearchRoomPacket(SearchRoomResponse data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public SearchRoomPacket(byte[] data)
	{
		SearchRoomSerializer serializer = new SearchRoomSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	public PacketId	GetPacketId()
	{
		return PacketId.SearchRoomResponse;
	}
	
	public SearchRoomResponse	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		SearchRoomSerializer serializer = new SearchRoomSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}


//
// 세션 통지 패킷 정의.
//
public class SessionPacket : IPacket<SessionData>
{
	class SessionSerializer : Serializer
	{
		//
		public bool Serialize(SessionData packet)
		{
			bool ret = true;

            // 매칭 결과 enum -> int
            int result = (int)packet.result;
            ret &= Serialize(result);
            // 단말 구별
            ret &= Serialize(packet.playerId);
            // 참가 인원
            ret &= Serialize(packet.members);

            // 각 참가인원 별로 IP주소가 따로 있으며
            // EndPointData 구조체 역시 각 성분 별로 시리얼라이즈해야한다.
            for (int i = 0; i < packet.members; ++i) {
                // EndPointData.ipAddress
                ret &= Serialize(packet.endPoints[i].ipAddress, EndPointData.ipAddressLength);
                // EndPointData.port
                ret &= Serialize(packet.endPoints[i].port);
			}
			
			return ret;
		}
		
		//
		public bool Deserialize(ref SessionData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;

            // 매칭 결과 int -> enum
            int result = 0;
			ret &= Deserialize(ref result);
			element.result = (MatchingResult) result;

            // 단말 구별
            ret &= Deserialize(ref element.playerId);

            // 참가 인원
            ret &= Deserialize(ref element.members);

            // 구조체를 디시리얼라이즈하는 것은 정의하지 않았으므로(할 수가 없다)
            // 참가 인원 수만큼 EndPointData 구조체 성분을 디시리얼라이즈
            element.endPoints = new EndPointData[element.members];
			for (int i = 0; i < element.members; ++i) {
                // EndPointData.ipAddress
                ret &= Deserialize(ref element.endPoints[i].ipAddress, EndPointData.ipAddressLength);
                // EndPointData.port
                ret &= Deserialize(ref element.endPoints[i].port);
			}

			return ret;
		}
	}
	
	// 패킷 데이터 구조체.
	SessionData	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public SessionPacket(SessionData data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public SessionPacket(byte[] data)
	{
		SessionSerializer serializer = new SessionSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	public PacketId	GetPacketId()
	{
		return PacketId.StartSessionNotify;
	}
	
	public SessionData	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		SessionSerializer serializer = new SessionSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}




//
//
// 게임용 패킷 데이터 정의.
//
//

// 게임 전 동기화 정보 패킷 정의.
public class GameSyncPacket : IPacket<GameSyncInfo>
{
	class GameSyncerializer : Serializer
	{
		//
		public bool Serialize(GameSyncInfo packet)
		{
			bool ret = true;

			// 동기화할 난수의 시드.
			ret &= Serialize(packet.seed);

			// 동기화할 정보.
			for (int i = 0; i < NetConfig.PLAYER_MAX; ++i) {
				// 캐릭터의 글로벌 ID.
				ret &= Serialize(packet.members[i].globalId);	
			}

			return ret;
		}
		
		//
		public bool Deserialize(ref GameSyncInfo element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}
	
			bool ret = true;

			// 동기화할 난수의 시드.
			ret &= Deserialize(ref element.seed);
			
			// 동기화할 장비 정보.
			element.members = new CharacterID[NetConfig.PLAYER_MAX];
			for (int i = 0; i < NetConfig.PLAYER_MAX; ++i) {
				// 캐릭터의 글로벌 ID.
				ret &= Deserialize(ref element.members[i].globalId);
			}

			return ret;
		}
	}
	
	
	// 패킷 데이터의 실체.
	GameSyncInfo	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public GameSyncPacket(GameSyncInfo data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public GameSyncPacket(byte[] data)
	{
		GameSyncerializer serializer = new GameSyncerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	public PacketId	GetPacketId()
	{
		return PacketId.GameSyncInfo;
	}
	
	public GameSyncInfo	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		GameSyncerializer serializer = new GameSyncerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
	
}

// 캐릭터 좌표 패킷 정의.
public class CharacterDataPacket : IPacket<CharacterData>
{
	class CharactorDataSerializer : Serializer
	{
		//
		public bool Serialize(CharacterData packet)
		{
		
			bool ret = true;

            //ret &= Serialize(packet.characterId, CharacterData.characterNameLength);
            ret &= Serialize(packet.characterId);
            ret &= Serialize(packet.index);
			ret &= Serialize(packet.dataNum);

			for (int i = 0; i < packet.dataNum; ++i) {
				// CharactorCoord
				ret &= Serialize(packet.coordinates[i].x);
				ret &= Serialize(packet.coordinates[i].y);
			}	
			
			return ret;
		}
		
		//
		public bool Deserialize(ref CharacterData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;
            
            ret &= Deserialize(ref element.characterId);
            ret &= Deserialize(ref element.index);
			ret &= Deserialize(ref element.dataNum);

			element.coordinates = new CharacterCoord[element.dataNum];
			for (int i = 0; i < element.dataNum; ++i) {
				// CharactorCoord
				ret &= Deserialize(ref element.coordinates[i].x);
				ret &= Deserialize(ref element.coordinates[i].y);
			}
			
			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	CharacterData		m_packet;
	
	public CharacterDataPacket(CharacterData data)
	{
		m_packet = data;
	}
	
	public CharacterDataPacket(byte[] data)
	{
		CharactorDataSerializer serializer = new CharactorDataSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	public PacketId	GetPacketId()
	{
		return PacketId.CharacterData;
	}
	
	public CharacterData	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		CharactorDataSerializer serializer = new CharactorDataSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

// 캐릭터 공격 패킷 정의.
public class AttackPacket : IPacket<AttackData>
{
	protected class AttackDataSerializer : Serializer
	{
		//
		public bool Serialize(AttackData packet)
		{
			bool ret = true;
			
            // 캐릭터 ID
			ret &= Serialize(packet.characterId);
            // 발사 힘
            ret &= Serialize(packet.fireForce);
            // 발사 각도(사원수기 때문에 4번의 디시리얼라이즈를 해주어야한다.)
            ret &= Serialize(packet.fireAngle.w);
            ret &= Serialize(packet.fireAngle.x);
            ret &= Serialize(packet.fireAngle.y);
            ret &= Serialize(packet.fireAngle.z);
            // 발사 좌표
            ret &= Serialize(packet.fireCoord.x);
            ret &= Serialize(packet.fireCoord.y);
        

			return ret;
		}
		
		//
		public bool Deserialize(ref AttackData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}
			
			bool ret = true;
			
            // 캐릭터 ID
			ret &= Deserialize(ref element.characterId);
            // 발사 힘
            ret &= Deserialize(ref element.fireForce);
            // 발사 각도(사원수기 때문에 4번의 디시리얼라이즈를 해주어야한다.)
            ret &= Deserialize(ref element.fireAngle.w);
            ret &= Deserialize(ref element.fireAngle.x);
            ret &= Deserialize(ref element.fireAngle.y);
            ret &= Deserialize(ref element.fireAngle.z);
            // 발사 좌표
            ret &= Deserialize(ref element.fireCoord.x);
            ret &= Deserialize(ref element.fireCoord.y);

			return true;
		}
	}
	
	// 패킷 데이터의 실체.
	AttackData m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public AttackPacket(AttackData data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public AttackPacket(byte[] data)
	{
		AttackDataSerializer serializer = new AttackDataSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	public PacketId	GetPacketId()
	{
		return PacketId.AttackData;
	}
	
	public AttackData	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		AttackDataSerializer serializer = new AttackDataSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

// 아이템 패킷 정의.
public class ItemPacket : IPacket<ItemData>
{
	class ItemSerializer : Serializer
	{
		//
		public bool Serialize(ItemData packet)
		{
			bool ret = true;

			ret &= Serialize(packet.itemId, ItemData.itemNameLength);
			ret &= Serialize(packet.state);
			ret &= Serialize(packet.ownerId, ItemData.charactorNameLength);
			
			return ret;
		}
		
		//
		public bool Deserialize(ref ItemData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;
			
			ret &= Deserialize(ref element.itemId, ItemData.itemNameLength);
			ret &= Deserialize(ref element.state);
			ret &= Deserialize(ref element.ownerId, ItemData.charactorNameLength);
			
			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	ItemData	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public ItemPacket(ItemData data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public ItemPacket(byte[] data)
	{
		ItemSerializer serializer = new ItemSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	public PacketId	GetPacketId()
	{
		return PacketId.ItemData;
	}
	
	// 게임에서 사용할 패킷 데이터 획득.
	public ItemData	GetPacket()
	{
		return m_packet;
	}
	
	// 송신용 byte[]형 데이터 획득.
	public byte[] GetData()
	{
		ItemSerializer serializer = new ItemSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

// 아이템 사용 패킷 정의.
public class ItemUsePacket : IPacket<ItemUseData>
{
	class ItemUseSerializer : Serializer
	{
		//
		public bool Serialize(ItemUseData packet)
		{
			bool ret = true;
			ret &= Serialize(packet.itemFavor);
			ret &= Serialize(packet.targetId, ItemUseData.characterNameLength);
			ret &= Serialize(packet.userId, ItemUseData.characterNameLength);
			ret &= Serialize(packet.itemCategory);

			return true;
		}
		
		//
		public bool Deserialize(ref ItemUseData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;
			ret &= Deserialize(ref element.itemFavor);
			ret &= Deserialize(ref element.targetId, ItemUseData.characterNameLength);
			ret &= Deserialize(ref element.userId, ItemUseData.characterNameLength);
			ret &= Deserialize(ref element.itemCategory);

			return true;
		}
	}
	
	// 패킷 데이터의 실체.
	ItemUseData	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public ItemUsePacket(ItemUseData data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public ItemUsePacket(byte[] data)
	{
		ItemUseSerializer serializer = new ItemUseSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	public PacketId	GetPacketId()
	{
		return PacketId.UseItem;
	}
	
	public ItemUseData	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		ItemUseSerializer serializer = new ItemUseSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

// HP 통지 패킷 정의.
public class HitPointPacket : IPacket<HpData>
{
	protected class HpDataSerializer : Serializer
	{
		//
		public bool Serialize(HpData packet)
		{
			bool ret = true;

			ret &= Serialize(packet.characterId);
			ret &= Serialize (packet.hp);
			
			return ret;
		}
		
		//
		public bool Deserialize(ref HpData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;

			ret &= Deserialize(ref element.characterId);
			ret &= Deserialize (ref element.hp);
			
			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	HpData m_packet;

	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public HitPointPacket(HpData data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public HitPointPacket(byte[] data)
	{
		HpDataSerializer serializer = new HpDataSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	public PacketId	GetPacketId()
	{
		return PacketId.HpData;
	}
	
	public HpData	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		HpDataSerializer serializer = new HpDataSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

// 데미지양 패킷 정의.
public class DamageDataPacket : IPacket<DamageData>
{
	protected class DamageDataSerializer : Serializer
	{
		//
		public bool Serialize(DamageData packet)
		{
			bool ret = true;

			ret &= Serialize(packet.target, DamageData.characterNameLength);
			ret &= Serialize(packet.attacker);
			ret &= Serialize (packet.damage);
			
			return ret;
		}
		
		//
		public bool Deserialize(ref DamageData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;

			ret &= Deserialize(ref element.target, DamageData.characterNameLength);
			ret &= Deserialize(ref element.attacker);
			ret &= Deserialize (ref element.damage);
			
			return ret;
		}
	}

	// 패킷 데이터의 실체.
	protected DamageData m_packet;
	
	// 상속용 생성자.
	public DamageDataPacket()
	{
	}

	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public DamageDataPacket(DamageData data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public DamageDataPacket(byte[] data)
	{
		DamageDataSerializer serializer = new DamageDataSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	virtual public PacketId	GetPacketId()
	{
		return PacketId.DamageData;
	}
	
	public DamageData	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		DamageDataSerializer serializer = new DamageDataSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

public class DamageNotifyPacket : DamageDataPacket
{

	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public DamageNotifyPacket(DamageData data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public DamageNotifyPacket(byte[] data)
	{
		DamageDataSerializer serializer = new DamageDataSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}

	public override PacketId	GetPacketId()
	{
		return PacketId.DamageNotify;
	}
}

// 채팅 패킷 정의.
public class ChatPacket : IPacket<ChatMessage>
{
	class ChatSerializer : Serializer
	{
		//
		public bool Serialize(ChatMessage packet)
		{
			bool ret = true;
			
			ret &= Serialize(packet.characterId, ChatMessage.characterNameLength);
			ret &= Serialize(packet.message, ChatMessage.messageLength);
			
			return ret;
		}
		
		//
		public bool Deserialize(ref ChatMessage element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;
			
			ret &= Deserialize(ref element.characterId, ChatMessage.characterNameLength);
			ret &= Deserialize(ref element.message, ChatMessage.messageLength);

			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	ChatMessage	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public ChatPacket(ChatMessage data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public ChatPacket(byte[] data)
	{
		ChatSerializer serializer = new ChatSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	public PacketId	GetPacketId()
	{
		return PacketId.ChatMessage;
	}
	
	public ChatMessage	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		ChatSerializer serializer = new ChatSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}
