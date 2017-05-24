using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Master : MonoBehaviour {
	// Use this for initialization
	public GameObject arduinoConnector;
	ArduinoConnector ac;
	void Start () {
		ac = arduinoConnector.GetComponent<ArduinoConnector>();
	}
	
	// Update is called once per frame
	void Update () {
		//figure out what got hit
		if(Input.GetMouseButtonDown(0))
		{
			//first frame where left mouse button is clicked
			var clickedGO = CheckForObjectUnderMouse();
			if (clickedGO == null ) { 
				Debug.Log("nothing clicked");
            }
			else {
				var clickedName = clickedGO.name;
				Debug.Log("clicked on " + clickedName);
				if (clickedName == "Initialize")
				{
					ac.InitializeOffsets();
				}
			}
		}
		//find matching commodity
		//reduce production of that commodity (signal to relevant agents)
	}
	private GameObject CheckForObjectUnderMouse()
    {
		RaycastHit hitInfo = new RaycastHit();
        bool hitFlag = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
		string output = "";
		GameObject ret = null;
		if (hitFlag)
		{
			output = hitInfo.transform.name;
			ret = hitInfo.transform.gameObject;
		}
        return ret;
    }

}
