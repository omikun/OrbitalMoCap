using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Sensor {
    public GameObject go, goDebug, grav;
    //debug
	public EditableQuaternion coordTransformQ, myDQ;
    //public GameObject goiq, goinit, godq;
    //initial q, inverse iq, desired q, current q, transform q,
    //raw iq
    Quaternion iq, iqi, dq, cq, dqiqi, iqidq, tq1, tq2, riq;
    public void SetDesiredQ()
    {
        dq = go.transform.rotation;
        //godq.transform.rotation = dq;
    }
    public void InitializeOffset()
    {
        // initial * transform = desired
        // inverse(initial) * initial * transform = inverse(initial) * desired
        // transform = inverse(initial) * desired
        // q1Offset = inverse(go transform) * q1;
        //goinit.transform.rotation = riq; 
        iqi = Quaternion.Inverse(riq);
        dqiqi = dq * iqi;

		initMyDQ = true;
		myDQ.Clear();
	}
	public void AddTo(List<float> list)
	{
		list.Add(go.transform.rotation.w);
		list.Add(go.transform.rotation.x);
		list.Add(go.transform.rotation.y);
		list.Add(go.transform.rotation.z);
	}
	void InitializeOffset_()
	{
		//relative rotation axis transform
        var rq = coordTransformQ.GetQ();
        var rqi = Quaternion.Inverse(coordTransformQ.GetQ());
        //return rq * rawInputQ;
		//iq = riq;//go.transform.rotation;
		//riq = raw input quat
		iq = rqi * riq * rq;
		iqi = Quaternion.Inverse(iq);
		myDQ.SetQ(dq * iqi);
    }
	bool initMyDQ = false;
	public bool ShowGravity = false;
    public void Update(string[] tokens, int offset, bool InitPos)
    {
        go.transform.rotation = ToQ(tokens, offset, InitPos);
		if (initMyDQ)
		{
			InitializeOffset_();
			initMyDQ = false;
        }

        goDebug.transform.rotation = go.transform.rotation;
    }
    public Quaternion ToQ(string[] tokens, int offset, bool InitPos)
    {
        Quaternion rawInputQ;
        float w = float.Parse(tokens[0+offset]);
        float x = float.Parse(tokens[1+offset]);
        float y = float.Parse(tokens[2+offset]);
        float z = float.Parse(tokens[3+offset]);
        //unity x y z
        //ardui y z x
        rawInputQ = new Quaternion(x, y, z, w);
        //rawInputQ = new Quaternion(y, -z, -x, w);
        rawInputQ = new Quaternion(-y, -z, x, w);
        riq = rawInputQ;

        var gvr = GetGravity(new Quaternion(x, y, z, w));
		var gvec = new Vector3(-gvr.y, -gvr.z, gvr.x);
		//var gvec = gvr;
        grav.transform.localPosition = gvec.normalized * 1.4f;
        //goiq.transform.rotation = rawInputQ;
		if (InitPos)
		{
		} else {
			var rq = coordTransformQ.GetQ();
			var rqi = Quaternion.Inverse(coordTransformQ.GetQ());
			//return rq * rawInputQ;
			return myDQ.GetQ() * rqi * rawInputQ * rq;
            //return dqiqi * rqi * rawInputQ * rq;
            //return iqi * rawInputQ * dq;
		}
        return riq;
    }
	public Vector3 GetGravity(Quaternion q)
	{
		//from Jeff Rowberg's i2cdev library for mpu6050
//		v -> x = 2 * (q -> x*q -> z - q -> w*q -> y);
//    v -> y = 2 * (q -> w*q -> x + q -> y*q -> z);
//    v -> z = q -> w*q -> w - q -> x*q -> x - q -> y*q -> y + q -> z*q -> z;
		Vector3 ret = new Vector3();
		ret.x = 2 * (q.x * q.z - q.w * q.y);
		ret.y = 2 * (q.w * q.x + q.y * q.z);
		ret.z = q.w * q.w - q.x * q.x - q.y * q.y + q.z * q.z;
		return ret;
	}
}