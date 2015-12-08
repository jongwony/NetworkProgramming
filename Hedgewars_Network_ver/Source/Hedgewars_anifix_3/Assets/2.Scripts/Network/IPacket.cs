
/// <summary>
/// 패킷들이 공통적으로 가져야할 요소 인터페이스
/// </summary>
/// <typeparam name="T">T는 패킷에 담을 다양한 구조체들</typeparam>

public interface IPacket<T>
{
	// 패킷 고유 ID를 얻음 
	PacketId 	GetPacketId();

    // 제네릭 T는 패킷에 담을 다양한 구조체를 구별하기 위한 요소
    T GetPacket();
 
	// 바이너리 데이터를 얻음 
	byte[] 		GetData();
}
