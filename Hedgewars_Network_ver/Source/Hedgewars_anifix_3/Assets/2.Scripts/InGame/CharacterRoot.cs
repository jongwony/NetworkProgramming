using UnityEngine;
using System.Collections.Generic;

// 캐릭터의 패킷이 오고가는 것을 관리하는 스크립트

public class CharacterRoot : MonoBehaviour
{
    private Network network = null;

    //==============================================================

    // Use this for initialization
    void Start()
    {
        // 네트워크 모듈 컴포넌트 획득
        GameObject netobj = GameObject.Find("Network");

        // 네트워크 이벤트 알림 등록
        // 수신한 패킷은 각 플레이어 스크립트에서 처리한다
        if(netobj != null)
        {
            network = netobj.GetComponent<Network>();
            if (network != null)
            {
                network.RegisterReceiveNotification(PacketId.CharacterData, OnReceiveCharacterPacket);
                network.RegisterReceiveNotification(PacketId.AttackData, OnReceiveAttackPacket);
                network.RegisterReceiveNotification(PacketId.HpData, OnReceiveHitPointPacket);
                //network.RegisterReceiveNotification(PacketId.ChatMessage, OnReceiveChatMessage);
                //network.RegisterReceiveNotification(PacketId.DamageData, OnReceiveDamageDataPacket);
                //network.RegisterReceiveNotification(PacketId.DamageNotify, OnReceiveDamageNotifyPacket);
            }
        }
    }

    //======================================================
    public void OnReceiveHitPointPacket(int node, PacketId id, byte[] data)
    {
        // 패킷 수신
        HitPointPacket packet = new HitPointPacket(data);
        HpData hpData = packet.GetPacket();

        // 송신한 플레이어 구별
        GameObject netplayer = findPlayer(hpData.characterId);

        // 캐릭터 hp 감소
        netplayer.GetComponent<NetPlayerCtrl>().hp = hpData.hp;

    }

    public void OnReceiveCharacterPacket(int node, PacketId id, byte[] data)
    {
        // 패킷 수신
        CharacterDataPacket packet = new CharacterDataPacket(data);
        CharacterData characterData = packet.GetPacket();

        // 송신한 플레이어 구별
        GameObject netplayer = findPlayer(characterData.characterId);

        // 캐릭터 좌표 보간
        if (netplayer != null)
        {
            netplayer.GetComponent<NetPlayerCtrl>().CalcCoordinates(characterData.index, characterData.coordinates);
        }
    }

    public void OnReceiveAttackPacket(int node, PacketId id, byte[] data)
    {
        // 패킷 수신
        AttackPacket packet = new AttackPacket(data);
        AttackData attackData = packet.GetPacket();
        Debug.Log("fireAngle:" + attackData.fireAngle);
        Debug.Log("fireForce:" + attackData.fireForce);

        // 송신한 플레이어 구별
        GameObject netplayer = findPlayer(attackData.characterId);

        // 해당 플레이어에게 좌표 전달
        if(netplayer != null)
        {
            netplayer.GetComponent<NetFireCtrl>().SetFirePos(attackData.fireCoord);
            netplayer.GetComponent<NetFireCtrl>().SetFireAngle(attackData.fireAngle);
            netplayer.GetComponent<NetFireCtrl>().SetFireForce(attackData.fireForce);
        }
    }

    //====================================================

    public void SendHitPointData(int charId, float hpData)
    {
        if(network != null)
        {
            HpData data = new HpData();
            data.characterId = charId;
            data.hp = hpData;

            HitPointPacket packet = new HitPointPacket(data);
            network.SendUnreliableToAll<HpData>(packet);
        }
    }

    // 10프레임마다 캐릭터 좌표를 송신
    public void SendCharacterCoord(int charId, int index, List<CharacterCoord> characterCoord)
    {
        if(network != null)
        {
            // 패킷 데이터 만들기
            CharacterData data = new CharacterData();
            data.characterId = charId;
            data.index = index;
            data.dataNum = characterCoord.Count;
            data.coordinates = new CharacterCoord[characterCoord.Count];
            for(int i = 0; i < characterCoord.Count; i++)
            {
                data.coordinates[i] = characterCoord[i];
            }

            // 캐릭터 좌표를 UDP송신
            CharacterDataPacket packet = new CharacterDataPacket(data);
            network.SendUnreliableToAll<CharacterData>(packet);
           
        }
    }

    public void SendAttackCoord(int charId, float fireForce, Quaternion fireAngle, CharacterCoord fireCoord)
    {
        if(network != null)
        {
            // 패킷 데이터 만들기
            AttackData data = new AttackData();
            data.characterId = charId;
            data.fireForce = fireForce;            
            data.fireAngle = fireAngle;
            data.fireCoord = fireCoord;

            // 데이터를 UDP송신
            AttackPacket packet = new AttackPacket(data);
            network.SendUnreliableToAll<AttackData>(packet);
        }
    }

    //=========================================================
    
    public GameObject findPlayer(int globalId) {        
        return GameObject.Find("basket").GetComponent<GameRoot>().GetNetPlayerObject(globalId);
    }

}
