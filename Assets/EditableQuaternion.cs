using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class EditableQuaternion {
    //[RangeAttribute(-3.14159f, 3.141519f)]
    [RangeAttribute(-180, 180)]
	public float x, y, z;
    public bool flip = false;
	public bool reset = false;
	public EditableQuaternion()
	{
        Clear();
    }
    public void Clear() {
		x = 0;
		y = 0;
		z = 0;
	}
    public void SetQ(Quaternion q)
    {
        x = q.eulerAngles.x;
        y = q.eulerAngles.y;
        z = q.eulerAngles.z;
    }
    public Quaternion GetQ()
    {
        if (reset) {
            reset = false;
            Clear();
        }
        var ret = Quaternion.Euler(x, y, z);
        if (flip)
            return Quaternion.Inverse(ret);
        else
            return ret;
    }

}
