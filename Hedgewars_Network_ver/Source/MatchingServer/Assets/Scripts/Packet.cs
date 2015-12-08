using System.Collections;
using System.IO;

//
// 매칭 매킷 정의.
//

// 매칭 요청 패킷 정의.
public class MatchingRequestPacket : IPacket<MatchingRequest>
{
	class MatchingRequestSerializer : Serializer
	{
		//
		public bool Serialize(MatchingRequest packet)
		{
			bool ret = true;

			ret &= Serialize(packet.version);
			int request = (int)packet.request;
			ret &= Serialize(request);
			ret &= Serialize(packet.roomId);
			ret &= Serialize(packet.name, MatchingRequest.roomNameLength);
			ret &= Serialize(packet.level);
			
			return ret;
		}
		
		//
		public bool Deserialize(ref MatchingRequest element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되어 있지 않습니다.
				return false;
			}

			bool ret = true;

			ret &= Deserialize(ref element.version);

			int request = 0;
			ret &= Deserialize(ref request);
			element.request = (MatchingRequestId) request;
			
			ret &= Deserialize(ref element.roomId);
			ret &= Deserialize(ref element.name, MatchingRequest.roomNameLength);
			ret &= Deserialize(ref element.level);
			
			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	MatchingRequest	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public MatchingRequestPacket(MatchingRequest data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하는 생성자.
	public MatchingRequestPacket(byte[] data)
	{
		MatchingRequestSerializer serializer = new MatchingRequestSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
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

			int result = (int)packet.result;
			ret &= Serialize(result);
			
			int request = (int)packet.request;
			ret &= Serialize(request);

			ret &= Serialize(packet.roomId);
			ret &= Serialize(packet.name, MatchingResponse.roomNameLength);
			ret &= Serialize(packet.members);
			
			return ret;
		}
		
		//
		public bool Deserialize(ref MatchingResponse element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되어 있지 않습니다.
				return false;
			}
		
			bool ret = true;

			int result = 0;
			ret &= Deserialize(ref result);
			element.result = (MatchingResult) result;
			
			int request = 0;
			ret &= Deserialize(ref request);
			element.request = (MatchingRequestId) request;
			
			ret &= Deserialize(ref element.roomId);
			ret &= Deserialize(ref element.name, MatchingResponse.roomNameLength);
			ret &= Deserialize(ref element.members);
			
			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	MatchingResponse	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public MatchingResponsePacket(MatchingResponse data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터에 디시리얼라이즈하기 위한 생성자.
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

			ret &= Serialize(packet.roomNum);
			
			for (int i = 0; i < packet.roomNum; ++i) {
				
				ret &= Serialize(packet.rooms[i].roomId);
				
				ret &= Serialize(packet.rooms[i].name, MatchingResponse.roomNameLength);
				
				ret &= Serialize(packet.rooms[i].members);
			}
			
			return ret;
		}
		
		//
		public bool Deserialize(ref SearchRoomResponse element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되어 있지 않습니다.
				return false;
			}

			bool ret = true;

			ret &= Deserialize(ref element.roomNum);
			
			element.rooms = new RoomInfo[element.roomNum];
			for (int i = 0; i < element.roomNum; ++i) {
				
				ret &= Deserialize(ref element.rooms[i].roomId);
				
				ret &= Deserialize(ref element.rooms[i].name, MatchingResponse.roomNameLength);
				
				ret &= Deserialize(ref element.rooms[i].members);
			}

			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
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

			int result = (int)packet.result;
			ret &= Serialize(result);
			ret &= Serialize(packet.playerId);
			ret &= Serialize(packet.members);

			for (int i = 0; i < packet.members; ++i) {
				
				ret &= Serialize(packet.endPoints[i].ipAddress, EndPointData.ipAddressLength);
				ret &= Serialize(packet.endPoints[i].port);
			}
			
			return ret;
		}
		
		//
		public bool Deserialize(ref SessionData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되어 있지 않습니다.
				return false;
			}

			bool ret = true;
			
			int result = 0;
			ret &= Deserialize(ref result);
			element.result = (MatchingResult) result;
			
			ret &= Deserialize(ref element.playerId);
			ret &= Deserialize(ref element.members);

			element.endPoints = new EndPointData[element.members];
			for (int i = 0; i < element.members; ++i) {
				
				ret &= Deserialize(ref element.endPoints[i].ipAddress, EndPointData.ipAddressLength);
				ret &= Deserialize(ref element.endPoints[i].port);
			}

			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
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
