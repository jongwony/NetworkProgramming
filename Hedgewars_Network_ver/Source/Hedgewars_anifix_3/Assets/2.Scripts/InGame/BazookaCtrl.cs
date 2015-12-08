using UnityEngine;
using System.Collections;

public class BazookaCtrl : MonoBehaviour {

    public int globalId = -1;
    public float shotPower;

    private CamMgr cameraMgr;

    public float damage = 200.0f;
	public float speed = 200.0f;

	// 폭발효과 파티클 연결 변수
	public GameObject expEffect;

	public Rigidbody2D rb;
	public Vector3 firePos = Vector3.zero;
    public Vector3 collPos = Vector3.zero;
    public Vector3 playerPos = Vector3.zero;

    private Vector3 fVec = Vector3.zero;
    private float player_damaged = 0.0f;
    private bool camPlayer;


	// Use this for initialization
	void Start () {

        GameObject shotPlayer = GameObject.Find("basket").GetComponent<GameRoot>().players[globalId];
        
        if (shotPlayer != null)
        {
            if (globalId == GlobalParam.get().global_account_id)
            {
                shotPower = shotPlayer.GetComponent<FireCtrl>().GetShotPower();
            }
            Debug.Log("Global Id : " + globalId);
            Debug.Log("Shot Power:" + shotPower * speed);
        }
        // 발사 초기값
        rb = GetComponent<Rigidbody2D> ();
		rb.AddForce (transform.right * speed * shotPower);

		
		// 총알 생성위치 = FirePos
		firePos = transform.position;

	}

	void OnCollisionEnter2D(Collision2D coll){

		// 충돌된 위치 저장 
		collPos = transform.position;

		// 충돌된 위치에서 이펙트 프리팹 생성 
		Object obj = Instantiate(expEffect,collPos,Quaternion.identity);
		// 0.1초 후 이펙트 제거
		Destroy (obj,0.1f);

		// 반경 1.0f 안에 있는 collider2d 추출 
		Collider2D[] colls = Physics2D.OverlapCircleAll (collPos, 1.0f);
		foreach (Collider2D findcoll in colls) {
			// 태그가 player일 경우 
			if(findcoll.gameObject.tag == "Player" || findcoll.gameObject.tag == "NetPlayer"){
				playerPos = findcoll.gameObject.transform.position;
				Debug.Log ("!!"+findcoll.gameObject.transform.position);
				Debug.Log ("collPos = "+collPos);

                // 데미지 계산
                fVec = collPos - playerPos;
                player_damaged = Mathf.Sqrt(fVec.x * fVec.x + fVec.y * fVec.y + fVec.z + fVec.z) * 100.0f;
                Debug.Log("Damaged:" + player_damaged);

				// 해당 방향으로 밀려난다
				findcoll.attachedRigidbody.mass = 1.0f;
				findcoll.attachedRigidbody.AddForce((playerPos - collPos)*1000.0f);
			}

            // 반지름을 1로 설정했으므로 그냥 곱하면된다   로컬플레이어만 데미지를 받아 중복계산을 방지한다.
            if (findcoll.gameObject.tag == "Player")
            {
                findcoll.GetComponent<PlayerCtrl>().hp -= (damage - player_damaged);

                // hp를 패킷으로 브로드 캐스트한다.
                GameObject.Find("basket").GetComponent<CharacterRoot>().SendHitPointData(GlobalParam.get().global_account_id, findcoll.GetComponent<PlayerCtrl>().hp);

            }
        }

        // 타겟 카메라를 로컬로 지정
        GameObject cameraManager = GameObject.Find("Main Camera");
        GameObject localPlayer = GameObject.Find("basket").GetComponent<GameRoot>().GetLocalPlayer();
        camPlayer = localPlayer.GetComponent<FireCtrl>().IsCamPlayer();
        if (!camPlayer)
        {
            cameraMgr = cameraManager.GetComponent<CamMgr>();
            cameraMgr.replaceTarget(localPlayer);
        }
        // 총알 제거
        DestroyObject (gameObject);
	}


}
