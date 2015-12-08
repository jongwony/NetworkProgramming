using UnityEngine;
using System.Collections;

public class FireCtrl : MonoBehaviour {

    private const float MAX_TIMER = 2.0f;

    private float coolTimer = 0.0f;
    private float timer = 0.0f;
    private float shotTimer = 0.0f;
    private bool shotEnable = false;
    private bool camPlayer = true;

    // 발사힘
    public float fireForce;
    public LineRenderer fireBar;

    private CamMgr cameraMgr;

    private float v = 0.0f;
	// 무기 프리팹
	public GameObject projectile;
    // null error 방지용
    private GameObject target_projectile;
	// 무기 발사좌표
	public Transform firePos;

    // Update is called once per frame
    void Update () {
        //if (UIMgr.OnClickFireButton ()) {
        //	Fire();
        //}

        fireBar = GameObject.Find("FirePos").GetComponent<LineRenderer>();

        // 2초로 파워 제한
        timer += Time.deltaTime;
        if (timer > MAX_TIMER) { timer = MAX_TIMER; }
        if (coolTimer >= 0) { coolTimer -= Time.deltaTime; }

        if (Input.anyKeyDown)
        {
            camPlayer = true;
        }
        
        
        if (coolTimer <= 0)
        {
            // 발사힘 결정
            if (Input.GetMouseButtonDown(0))
            {
                shotEnable = true;
                timer = 0.0f;
            }

            if (shotEnable)
            {
                // Fire 바 표시
                fireBar.SetPosition(1, new Vector3(1.0f * (timer / MAX_TIMER), 0.0f, 0.0f));
                fireBar.SetWidth(0.0f, 0.2f * (timer / MAX_TIMER));

                if (Input.GetMouseButtonUp(0))
                {
                    // 파워 저장
                    shotTimer = timer;
                    Fire();

                    // 초기화 작업
                    coolTimer = 3.0f;
                    shotEnable = false;
                    camPlayer = false;
                    fireBar.SetPosition(1, new Vector3(0.0f, 0.0f, 0.0f));
                    fireBar.SetWidth(0.0f, 0.0f);
                }
            }
        }

        GameObject cameraManager = GameObject.Find("Main Camera");
        GameObject localPlayer = GameObject.Find("basket").GetComponent<GameRoot>().GetLocalPlayer();
        cameraMgr = cameraManager.GetComponent<CamMgr>();
        if (!camPlayer)
        {
            // 타겟 카메라를 발사체로 지정
            if (target_projectile != null)
            {
                cameraMgr.replaceTarget(target_projectile);
            }
        }
        else
        {
            cameraMgr.replaceTarget(localPlayer);
        }


    }

    void OnGUI()
    {
        float cool = Mathf.Round(coolTimer*100.0f)*0.01f;
        if (cool < 0) { cool = 0; }
        GUI.Label(new Rect(Screen.width - 100, 10, 50, 20), "Cool :");
        GUI.Label(new Rect(Screen.width - 50, 10, 50, 20), cool.ToString());
    }

	void Fire(){

        // 발사 된 값을 패킷으로 전달
        Vector3 positionData = firePos.position + firePos.right * 0.2f;
        CharacterCoord packetcoord = new CharacterCoord(positionData.x, positionData.y);
        float fireForce = GetShotPower();
        Quaternion fireAngle = firePos.rotation * Quaternion.Euler(0, 0, v);
        int characterId = GlobalParam.get().global_account_id;

        // CharacterRoot에 있는 함수 실행
        GameObject.Find("basket").GetComponent<CharacterRoot>().SendAttackCoord(characterId, fireForce, fireAngle, packetcoord);

        // 프리팹 동적 생성
        target_projectile = (GameObject)Instantiate (projectile,positionData, fireAngle);
        target_projectile.GetComponent<BazookaCtrl>().globalId = GlobalParam.get().global_account_id;
    }

    public float GetShotPower()
    {
        return shotTimer;
    }

    public bool IsCamPlayer() { return camPlayer; }

}
