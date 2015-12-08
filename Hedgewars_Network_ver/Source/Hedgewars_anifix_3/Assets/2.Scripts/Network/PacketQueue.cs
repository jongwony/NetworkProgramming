
/// <summary>
/// 메인스레드와 통신스레드가 공유하는 큐
/// 통신스레드는 패킷 수신 버퍼와 큐를 가지고
/// 메인 스레드는 큐에서 데이터를 추출하여 사용한다.
/// </summary>

using System;                       // Math
using System.Collections.Generic;   // List<>
using System.IO;                    // MemoryStream

public class PacketQueue
{	
	// 패킷 저장 정보.
	struct PacketInfo
	{
		public int	offset;
		public int 	size;
	};

    // 데이터를 보존할 버퍼
    private MemoryStream 		m_streamBuffer;

    // 패킷 정보 관리 리스트
    private List<PacketInfo>	m_offsetList;

    // 메모리 배치 오프셋
    private int					m_offset = 0;

    // 세마포어 락
    private Object lockObj = new Object();
	
	//  생성자(여기서 초기화한다).
	public PacketQueue()
	{
		m_streamBuffer = new MemoryStream();
		m_offsetList = new List<PacketInfo>();
	}
	
	// 큐를 추가한다.
	public int Enqueue(byte[] data, int size)
	{
		PacketInfo	info = new PacketInfo();
	
		info.offset = m_offset;
		info.size = size;
			
		lock (lockObj) {
			// 패킷 저장 정보를 보존.
			m_offsetList.Add(info);
			
			// 패킷 데이터를 보존.
			m_streamBuffer.Position = m_offset;
			m_streamBuffer.Write(data, 0, size);
			m_streamBuffer.Flush();
			m_offset += size;
		}
		
		return size;
	}
	
	// 큐를 추출.
	public int Dequeue(ref byte[] buffer, int size) {

		if (m_offsetList.Count <= 0) {
			return -1;
		}

		int recvSize = 0;
		lock (lockObj) {	
			PacketInfo info = m_offsetList[0];
		
			// 버퍼에서 해당하는 패킷 데이터를 획득한다.
			int dataSize = Math.Min(size, info.size);
			m_streamBuffer.Position = info.offset;
			recvSize = m_streamBuffer.Read(buffer, 0, dataSize);
			
			// 큐 데이터를 추출했으므로 선두 요소를 삭제.
			if (recvSize > 0) {
				m_offsetList.RemoveAt(0);
			}
			
			// 모든 큐 데이터를 추출했을 때는 스티림을 클리어해서 메모리를 절약한다.
			if (m_offsetList.Count == 0) {
				Clear();
				m_offset = 0;
			}
		}
		
		return recvSize;
	}

	// 큐를 클리어한다.	
	public void Clear()
	{
		byte[] buffer = m_streamBuffer.GetBuffer();
		Array.Clear(buffer, 0, buffer.Length);
		
		m_streamBuffer.Position = 0;
		m_streamBuffer.SetLength(0);
	}
}

