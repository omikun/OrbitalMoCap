﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Sensor {
    public GameObject go;
    //debug
	public EditableQuaternion coordTransformQ, myDQ;
    public GameObject goiq, goinit, godq;
    //initial q, inverse iq, desired q, current q, transform q,
    //raw iq
    Quaternion iq, iqi, dq, cq, dqiqi, iqidq, tq1, tq2, riq;
    public void SetDesiredQ()
    {
        dq = go.transform.rotation;
        godq.transform.rotation = dq;
    }
    public void InitializeOffset()
    {
        // initial * transform = desired
        // inverse(initial) * initial * transform = inverse(initial) * desired
        // transform = inverse(initial) * desired
        // q1Offset = inverse(go transform) * q1;
        goinit.transform.rotation = riq; 
        iqi = Quaternion.Inverse(riq);
        dqiqi = dq * iqi;
    }
    public void Update(string[] tokens, int offset, bool InitPos)
    {
        go.transform.rotation = ToQ(tokens, offset, InitPos);

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
        //rawInputQ = new Quaternion(x, y, z, w);
        //rawInputQ = new Quaternion(y, -z, -x, w);
        rawInputQ = new Quaternion(-y, -z, x, w);

        riq = rawInputQ;

        goiq.transform.rotation = rawInputQ;
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
}