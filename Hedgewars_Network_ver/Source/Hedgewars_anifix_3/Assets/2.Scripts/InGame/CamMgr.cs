using UnityEngine;
using System.Collections;

public class CamMgr : MonoBehaviour {

	public Transform target;
	private Transform tr;

	void Start(){
		tr = GetComponent<Transform> ();
	}

	// 카메라 추적
	void LateUpdate(){		
		tr.position = GetTarget().position + Vector3.back;
		tr.LookAt (GetTarget());
	}

    public void replaceTarget(GameObject obj)
    {
        target = obj.transform;
    }

    public Transform GetTarget()
    {
        return target;
    }

	public void OnClickFocus(){
		Debug.Log ("Focus");
		//tr.LookAt (target);
	}

	public void OnClickScrollView(){
		Debug.Log ("ScrollView");
	}

	public void OnClickBackMain(){
		Debug.Log ("Back to Main");
	}
}
