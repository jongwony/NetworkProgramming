using UnityEngine;
using System.Collections.Generic;

public class NetPlayerCtrl : MonoBehaviour {

    // 네트워크 플레이어 컴포넌트의 글로벌id (생성시점에 GameRoot가 결정함)
    public int globalId = -1;

    public float hp;
    public float movSpeed = 1.0f;

    private const float MAXHP = 1000.0f;

    public Transform tr;
    public Rigidbody2D rb;
    public LineRenderer hpBar;
    public Transform trChild;

    private bool leftFlag = false;
    private bool rightFlag = true;

    public bool isGrounded;

    // 이전 프레임 x좌표
    private float prev_x;

    public Animator animator = null;

    private bool isDead = false;


    // 캐릭터 좌표 보간
    private const int PLOT_NUM = 4;
    private int m_plotIndex = 0;

    // 추출한 좌표 보존
    private List<CharacterCoord> m_culling = new List<CharacterCoord>();
    // 보간한 좌표 보존
    private List<CharacterCoord> m_plots = new List<CharacterCoord>();

    // 좌표 보간 함수
    public void CalcCoordinates(int index, CharacterCoord[] data)
    {
        // 중복계산 방지
        do
        {
            // 데이터가 비는 경우나
            if(data.Length <= 0) { break; }

            // 계산할 새로운 데이터가 없으면 루프를 탈출
            if (index <= m_plotIndex) { break; }

            // m_plotIndex          m_culling[]의 마지막 점의 인덱스.
            // index                data[]의 마지막 점의 인덱스.
            // 보간할 좌표만 저장한다
            for(int i = 0; i < data.Length; i++)
            {
                if (index - i - (PLOT_NUM - 1) < m_plotIndex)
                {
                    m_culling.Add(data[i]);
                }
            }

            // 최신 좌표 설정
            m_plotIndex = index;

            // 스플라인 곡선을 구해서 보간
            SplineData spline = new SplineData();
            spline.CalcSpline(m_culling);

            // 구한 스플라인 곡선을 좌표 정보로 보존
            CharacterCoord plot = new CharacterCoord();
            for (int i = 0; i < spline.GetPlotNum(); i++)
            {
                spline.GetPoint(i, out plot);
                m_plots.Add(plot);
            }

            // 가장 오래된 좌표를 삭제한다.
            if (m_culling.Count > PLOT_NUM)
            {
                m_culling.RemoveRange(0, m_culling.Count - PLOT_NUM);
            }
        } while (false);
    }

    // Use this for initialization
    void Start() {
        hp = 1000.0f;

        tr = transform.GetComponent<Transform>();
        hpBar = GameObject.Find("NetHPBar").GetComponent<LineRenderer>();

        // 캐릭터 로컬 x좌표만 바꾼다
        //trChild = GameObject.Find("Hedgehog").GetComponent<Transform>();
        prev_x = tr.position.x;

        animator = gameObject.GetComponentInChildren<Animator>();

        animator.SetTrigger("Idle");

        isGrounded = false;
    }
	
	// Update is called once per frame
	void Update () {
        //죽음
        if (hp <= 0)
        {
            hp = 0;
            isDead = true;
            dead();
        }


        rb = GetComponent<Rigidbody2D>();

        // hp 바 표시
        hpBar.SetPosition(1, new Vector3(3.0f * (hp / MAXHP), 0.0f, 0.0f));

        // 애니메이션 처리
        if (tr.position.x - prev_x < -0.0001f) { animator.SetTrigger("Walk"); }
        else { animator.SetTrigger("Idle"); }

        if(m_plots.Count > 0)
        {
            // 보간한 좌표로 이동
            CharacterCoord coord = m_plots[0];            
            transform.position = new Vector3(coord.x, coord.y);

            // 이동했으니 리스트에서 삭제
            m_plots.RemoveAt(0);

        }

        // 좌 우 회전
        // Child컴포넌트만 회전해 다른 영향을 주지 않도록 한다.
        if (!leftFlag && (tr.position.x - prev_x < -0.0001f))
        {
            trChild.localScale = new Vector3(trChild.localScale.x * (-1), trChild.localScale.y, trChild.localScale.z);
            leftFlag = true;
            rightFlag = false;
        }
        if (!rightFlag && (tr.position.x - prev_x > 0.0001f))
        {
            trChild.localScale = new Vector3(trChild.localScale.x * (-1), trChild.localScale.y, trChild.localScale.z);
            rightFlag = true;
            leftFlag = false;
        }


        prev_x = tr.position.x;
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
