using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class EditableQuaternion {
    [RangeAttribute(-3.14159f, 3.141519f)]
	public float x, y, z;
	public bool reset = false;
	public EditableQuaternion()
	{
        Clear();
    }
    void Clear() {
		x = 0;
		y = 0;
		z = 0;
	}
    public Quaternion GetQ()
    {
        if (reset) {
            reset = false;
            Clear();
        }
        return Quaternion.EulerRotation(x, y, z);
    }

}
