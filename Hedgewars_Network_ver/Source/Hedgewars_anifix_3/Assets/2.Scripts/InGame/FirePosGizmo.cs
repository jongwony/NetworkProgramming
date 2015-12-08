using UnityEngine;
using System.Collections;

// 각도 가이드라인
// 게임에 표시되지않음

public class FirePosGizmo : MonoBehaviour {

	public float _radius = 0.05f;

	void OnDrawGizmos(){
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine (transform.position,transform.position + transform.right );
	}

}
