using UnityEngine;
using System.Collections;

public class NetFireCtrl : MonoBehaviour
{
    // 무기 프리팹
    public GameObject projectile;

    // null error 방지
    public GameObject target_projectile;

    // 발사 힘
    private float fireForce;

    // 무기 발사좌표
    public CharacterCoord firePos;

    // 무기 발사각도
    public Quaternion fireAngle;

    public void SetFireAngle(Quaternion fireAngle)
    {
        this.fireAngle = fireAngle;
    }

    public void SetFirePos(CharacterCoord firePos)
    {
        this.firePos.x = firePos.x;
        this.firePos.y = firePos.y;
    }

    public void SetFireForce(float fireForce)
    {
        this.fireForce = fireForce;
    }

    public float GetShotPower()
    {
        return fireForce;
    }

    void Update()
    {
        // 이벤트로 받은 발사 힘이 0보다 크면 발사
        if(fireForce > 0)
        {
            Fire();

            // 힘 초기화
            fireForce = 0.0f;
        }
    }

    void Fire()
    {
        // 프리팹 동적 생성
        target_projectile = (GameObject)Instantiate(projectile, new Vector3(firePos.x,firePos.y), fireAngle);
        target_projectile.GetComponent<BazookaCtrl>().globalId = GetComponentInParent<NetPlayerCtrl>().globalId;
        target_projectile.GetComponent<BazookaCtrl>().shotPower = fireForce;
    }
}
