using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRotation : MonoBehaviour {

	public GameObject iq1, dq1, cq1, tq1, tq12;
	// Use this for initialization
	void Start () {
		
	}
	Quaternion mulRot(GameObject g0, GameObject g1)
	{
		return rot(g0) * rot(g1);
	}
	Quaternion mulRot(Quaternion q, GameObject g1)
	{
		return q * rot(g1);
	}
	Quaternion mulRot(GameObject g1, Quaternion q)
	{
		return rot(g1) * q;
	}
	Quaternion mulRot(Quaternion q0, Quaternion q)
	{
		return q0 * q;
	}
	Quaternion rot(GameObject go)
	{
		return go.transform.rotation;
	}
	void setRot(GameObject go, GameObject go2)
	{
		go.transform.rotation = rot(go2);
	}
	void setRot(GameObject go, Quaternion rot)
	{
		go.transform.rotation = rot;
	}
	// Update is called once per frame
	bool first = true;
	Quaternion iq, dqiqi1;
	void Update () {
		if (first)
		{
            iq = rot(iq1);
            var iqi = Quaternion.Inverse(iq);
            dqiqi1 = rot(dq1) * iqi;
			first = false;
		}
/* 
		var iqidq1 = iqi * rot(dq1);
        setRot(cq1, mulRot(dq1, iq1));
        //setRot(cq1, mulRot(mulRot(iq1, cq1), iqi1));
        setRot(tq1, mulRot(iqi, iq1));
        //setRot(tq1, mulRot(dqiqi1, iq1));
*/
        //THIS IS CORRECT
        setRot(tq12, mulRot(iq1, dqiqi1));
    }
}
