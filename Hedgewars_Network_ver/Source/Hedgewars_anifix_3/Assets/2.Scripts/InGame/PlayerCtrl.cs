using UnityEngine;
using System.Collections.Generic;

public class PlayerCtrl : MonoBehaviour {
    // 나의 글로벌 id
    private int globalId = GlobalParam.get().global_account_id;

	private float h = 0.0f;
	private float v = 0.0f;

    // 캐릭터 기본 오른쪽 방향
    private bool rightFlag = true;
    private bool leftFlag = false;

    public float hp;
    private const float MAXHP = 1000.0f;
    private bool isDead = false;

    public float movSpeed = 1.0f;

    private const float MAX_TIMER = 3.0f;

    public Transform tr;
    public Transform trChild;
    public Transform firePos;
    public Rigidbody2D rb;
    public LineRenderer hpBar;

    // 점프 관련
    private bool isGrounded = false;
    public int jumpCount = 0;
    private const float JUMP_POWER = 300.0f;
    private const int MAX_JUMP_COUNT = 2;

    // 애니메이션 구현 요소
    public Animator animator = null;

    // 송신용 네트워크 정보 
    private const int PLOT_NUM = 4;     // 보존하는 점의 갯수
    private int m_send_count = 0;       // 보낸 횟수
    private int m_plotIndex = 0;        // 보간 인덱스
    private Vector3 m_prev;             // 변화량 벡터를 위한 전프레임 정보    
    // 추출한 좌표 보존
    private List<CharacterCoord> m_culling = new List<CharacterCoord>();
    // 보간한 좌표 보존
    //private List<CharacterCoord> m_plots = new List<CharacterCoord>();


    // Use this for initialization
    void Start () {
        hp = MAXHP;

        // 컴포넌트 불러오기
		tr = transform.GetComponent<Transform> ();
        trChild = GameObject.Find("Hedgehog").GetComponent<Transform>();
        firePos = GameObject.Find("FirePos").GetComponent<Transform>();
        hpBar = GameObject.Find("LocalHPBar").GetComponent<LineRenderer>();
        animator = gameObject.GetComponentInChildren<Animator>();

        animator.SetTrigger("Idle");

    }

	/*void OnCollisionEnter2D(Collision2D coll){
		if (coll.collider.tag == "BAZOOKA") {
			// 충돌된 위치 저장 
			Vector3 collPos = transform.position;
			Debug.Log ("!!"+collPos);
			Debug.Log ("Player"+tr.position);
		}
	}*/

	// Update is called once per frame
	void Update () {

        //죽음
        if (hp <= 0)
        {
            hp = 0;
            isDead = true;
            dead();
        }

        //h = AndroidInput._;
        // 좌 우 이동
        h = Input.GetAxis ("Horizontal");

        // Child컴포넌트만 회전해 다른 영향을 주지 않도록 한다.
        if (!leftFlag && h < -0.1f)
        {
            trChild.Rotate(0, 180.0f, 0);
            leftFlag = true;
            rightFlag = false;
        }
        if (!rightFlag && h > 0.1f)
        {
            trChild.Rotate(0, 180.0f, 0);
            rightFlag = true;
            leftFlag = false;
        }     
        tr.Translate (Vector2.right * (h) * movSpeed * Time.deltaTime, Space.Self);

        // 발사각 조절
        v = Input.GetAxis("Vertical");
        firePos.Rotate (0, 0, v);

        // Collider 가 반지름 0.12를 감안하여 접지판정
        Vector2 groundCheck = transform.position + Vector3.down * 0.16f;
        isGrounded = (Physics2D.OverlapPoint(groundCheck) != null) ? true : false;

        // 애니메이션 처리
        if (isGrounded)
        {
            jumpCount = 0;
            if(h != 0){ animator.SetTrigger("Walk"); }
            else { animator.SetTrigger("Idle"); }
        }
        //else { animator.SetTrigger("Jump"); }

        // 점프
		rb = GetComponent<Rigidbody2D> ();
        if (jumpCount < MAX_JUMP_COUNT)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                rb.AddForce(transform.up * JUMP_POWER);
                jumpCount++;
            }
        }

        // hp 바 표시
        hpBar.SetPosition(1, new Vector3(3.0f * (hp / MAXHP), 0.0f, 0.0f));


        // 나의 현재 위치를 패킷을 만들어 보내기
        do
        {
            // 10프레임마다 송신
            m_send_count = (m_send_count + 1) % SplineData.SEND_INTERVAL;

            if(m_send_count != 0 && m_culling.Count < PLOT_NUM)
            {
                break;
            }

            // 현재 위치 구조체에 담기
            Vector3 positionData = transform.position;
            CharacterCoord coord = new CharacterCoord(positionData.x, positionData.y);

            // 변화량을 체크하여 정지상태일때는 데이터를 보내지 않도록 한다.
            Vector3 diff = m_prev - positionData;
            if (diff.sqrMagnitude > 0.0001f)
            {
                // 좌표를 자료구조에 보존
                m_culling.Add(coord);

                // CharacterRoot에 있는 함수 실행
                GameObject.Find("basket").GetComponent<CharacterRoot>().SendCharacterCoord(globalId, m_plotIndex, m_culling);
                
                // 인덱스 증가
                ++m_plotIndex;

                // 가장 오래된 좌표 삭제
                if(m_culling.Count >= PLOT_NUM)
                {
                    m_culling.RemoveAt(0);
                }

                // 위치 변화량 초기화
                m_prev = positionData;
            }

        } while (false);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width - 100, 40, 80, 20), "HP   :");
        GUI.Label(new Rect(Screen.width - 50, 40, 50, 20), hp.ToString());

        // 죽었을 때 UI를 누르면 접속 종료
        if (isDead)
        {           
            GameObject.Find("basket").GetComponent<GameRoot>().NotifyDead();
        }
    }

    private void dead()
    {
        // 타겟 카메라를 배경으로 지정
        if (isDead)
        {
            animator.SetTrigger("dead");
        }
    }
}
