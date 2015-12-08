using UnityEngine;
using System.Collections.Generic;

public class GameRoot : MonoBehaviour {

    public Texture loading_image;

    // 에러 UI발생
    public bool hostDisconnected = false;
    private float scene_timer = 0.0f;

    // 내 단말에서 로컬과 네트워크 플레이어 구별
    public Dictionary<int, GameObject> players = new Dictionary<int,GameObject>();

    private CamMgr cameraMgr;
    public GameObject localPlayer;
    public GameObject netPlayer;

    // 네트워크 모듈
    protected Network network;

    public void SetLocalPlayer(GameObject localPlayer)
    {
        this.localPlayer = localPlayer;
    }

    public GameObject GetLocalPlayer()
    {
        return localPlayer;
    }

    public GameObject GetNetPlayerObject(int globalId)
    {
        if (players.ContainsKey(globalId))
        {
            return players[globalId];
        }
        Debug.Log("No GameObject!");
        return null;
    }

    public bool isHost()
    {
        bool ret = true;

        if(network != null)
        {
            ret = GlobalParam.get().is_host;
        }
        else
        {
            return true;
        }
        return ret;
    }

    //===================================================

    // 로컬 플레이어 만들기
    public void createLocalPlayer(int account_global_index)
    {
        // 캐릭터 프리팹 만들기
        localPlayer = (GameObject)Instantiate(localPlayer, Vector3.zero, Quaternion.identity);

        // 로컬 지정
        SetLocalPlayer(localPlayer);

        // 구별하기
        players.Add(account_global_index, localPlayer);
    }

    public void createNetPlayer(int account_global_index)
    {
        // 캐릭터 프리팹 만들기
        netPlayer = (GameObject)Instantiate(netPlayer, Vector3.zero, Quaternion.identity);

        // 구별하기
        players.Add(account_global_index, netPlayer);
    }

    public void deleteNetPlayer(int account_global_index)
    {
        // 연결된 게임 오브젝트 제거
        Destroy(players[account_global_index]);

        // 노드 제거
        players.Remove(account_global_index);
        Debug.Log("DeleteNetPlayer global id:" + account_global_index);
    }

        //===================================================

    void Awake()
    {
        GameObject netobj = GameObject.Find("Network");
        if(netobj!= null)
        {
            network = netobj.GetComponent<Network>();
        }

        // 로컬 플레이어 생성
        createLocalPlayer(GlobalParam.get().global_account_id);
        Debug.Log("global_account_id" + GlobalParam.get().global_account_id);
        
        for(int i = 0; i < GlobalParam.get().playerNum; i++)
        {
            // 로컬 플레이어는 스킵
            if(i == GlobalParam.get().global_account_id)
            {
                continue;
            }

            // 연결된 모든 네트워크 플레이어 생성
            if(network != null)
            {
                createNetPlayer(i);

                // 생성된 네트워크 플레이어에 인덱스 부여
                // 이 인덱스는 노드의 키 값과 같다
                GetNetPlayerObject(i).GetComponent<NetPlayerCtrl>().globalId = i;
                
            }
        }
    }

	// Use this for initialization
	void Start () {

        GameObject cameraManager = GameObject.Find("Main Camera");
        cameraMgr = cameraManager.AddComponent<CamMgr>();
       
        // 타겟 카메라를 로컬로 지정
        cameraMgr.replaceTarget(localPlayer);
	}
	
	// Update is called once per frame
	void Update () {
        this.scene_timer += Time.deltaTime;
        
        // 오류 핸들링
        NetEventHandling();
	}

    void OnGUI()
    {
        // 배경 이미지
        if (GlobalParam.get().fadein_start)
        {
            float title_alpha = Mathf.InverseLerp(1.0f, 0.0f, this.scene_timer);
            if(title_alpha > 0.0f)
            {
                GUI.color = new Color(200.0f, 200.0f, title_alpha);
                GUI.DrawTexture(new Rect(0.0f, 0.0f, Screen.width, Screen.height), this.loading_image, ScaleMode.ScaleToFit, true);
            }
        }

        if(hostDisconnected)
        {
            NotifyError();
        }        
    }

    public void NotifyDead()
    {
        GUIStyle style = new GUIStyle(GUI.skin.GetStyle("button"));
        style.normal.textColor = Color.white;
        style.fontSize = 20;

        float sx = 400;
        float sy = 150;
        float px = Screen.width / 2 - sx * 0.5f;
        float py = Screen.height / 2 - sy * 0.5f;

        string message = "패배.\n게임을 종료합니다.";
        if (GUI.Button(new Rect(px, py, sx, sy), message, style))
        {
            // 네트워크 연결을 끊고
            if (GlobalParam.get().is_host)
            {
                network.StopGameServer();
            }
            network.StopServer();
            network.Disconnect();

            hostDisconnected = false;

            GameObject.Destroy(network);

            // 타이틀 씬으로 되돌아간다
            Application.LoadLevel("UILobby");
        }
    }

    // 오류 GUI
    private void NotifyError()
    {
        GUIStyle style = new GUIStyle(GUI.skin.GetStyle("button"));
        style.normal.textColor = Color.white;
        style.fontSize = 20;

        float sx = 400;
        float sy = 150;
        float px = Screen.width / 2 - sx * 0.5f;
        float py = Screen.height / 2 - sy * 0.5f;

        string message = "통신 오류가 발생했습니다.\n게임을 종료합니다.";
        if(GUI.Button(new Rect(px,py,sx, sy),message, style))
        {
            // 네트워크 연결을 끊고
            if (GlobalParam.get().is_host)
            {
                network.StopGameServer();
            }
            network.StopServer();
            network.Disconnect();

            hostDisconnected = false;

            GameObject.Destroy(network);

            // 타이틀 씬으로 되돌아간다
            Application.LoadLevel("UILobby");
        }
    }

    //====================================================

    // 이벤트 핸들링
    public void NetEventHandling()
    {
        if(network == null) { return; }

        // 이벤트 감지
        NetEventState state = network.GetEventState();
        if(state == null) { return; }

        switch (state.type)
        {
            case NetEventType.Connect:
                Debug.Log("[CLIENT]connect event handling:" + state.node);
                break;
            case NetEventType.Disconnect:
                Debug.Log("[CLIENT]disconnect event handling:" + state.node);
                DisconnectEventProc(state.node);
                break;
        }
    }

    // 접속 끊김에 관한 이벤트
    private void DisconnectEventProc(int node)
    {
        int global_Index = network.GetPlayerIdFromNode(node);
        // 서버와 호스트가 접속이 끊기면 종료
        if (node == network.GetServerNode() || global_Index == 0)
        {
            hostDisconnected = true;
        }
        else
        {
            Debug.Log("[CLIENT]disconnect character:" + global_Index);
            deleteNetPlayer(global_Index);
        }
    }
}
