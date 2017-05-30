
﻿/* ArduinoConnector by Alan Zucconi
 * http://www.alanzucconi.com/?p=2979
 */
using UnityEngine;
using System;
using System.Collections;
using System.IO.Ports;
using System.Threading;

[System.Serializable]
public class Sensor {
    public GameObject go;
    //debug
    public GameObject goiq, goinit, godq;
    Quaternion iq, iqi, dq, cq, dqiqi, iqidq, tq1, tq2;
}
public class ArduinoConnector : MonoBehaviour {

    /* The serial port where the Arduino is connected. */
    [Tooltip("The serial port where the Arduino is connected")]
    public string port = "/dev/cu.usbmodem1421";
    /* The baudrate of the serial port. */
    [Tooltip("The baudrate of the serial port")]
    public int baudrate = 115200;
    private SerialPort stream;
	public bool UseThread = false;

	public GameObject go1, go2, go3, go4;
	public GameObject goiq1, goiq2, goinitq1, goinitq2, goinitq3, goinitq4, godq1, godq2, godq3, godq4;
	Quaternion iq1, iqi1, dq1, cq1, dqiqi1, iqidq1, tq1, tq12;
	Quaternion iq2, iqi2, dq2, cq2, dqiqi2, iqidq2, tq2, tq22;
	Quaternion iq3, iqi3, dq3, cq3, dqiqi3, iqidq3, tq3, tq32;
	Quaternion iq4, iqi4, dq4, cq4, dqiqi4, iqidq4, tq4, tq42;
	float beginTime;
	private Thread t1;
	private volatile bool workInProgress = false;

    public void Open () {
        // Opens the serial port
        stream = new SerialPort(port, baudrate);
        stream.ReadTimeout = 50;
        stream.Open();
        //this.stream.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
	}

	void Start() {
		Open();
		return;
        StartCoroutine
        (
            AsynchronousReadFromArduino
            ((string s) => Debug.Log(s),     // Callback
                () => Debug.LogError("Error!"), // Error callback
                10f                             // Timeout (seconds)
            )
        );
		if (UseThread)
		{
            t1 = new Thread(PollQuats) { Name = "Thread 1" };
            t1.Start();
        }
		beginTime = Time.time;
    }

    public void WriteToArduino(string message)
    {
        // Send the request
        stream.WriteLine(message);
        stream.BaseStream.Flush();
    }
    Quaternion mulRot(Quaternion q, GameObject g1)
	{
		return q * rot(g1);
	}
	Quaternion mulRot(GameObject g1, Quaternion q)
	{
		return rot(g1) * q;
	}
	Quaternion rot(GameObject go)
	{
		return go.transform.rotation;
	}
	void setRot(GameObject go, Quaternion rot)
	{
		go.transform.rotation = rot;
	}
	Quaternion rawInputQ, riq1, riq2, riq3, riq4;
	Quaternion ToQ(string[] tokens, int offset=0)
    {
        float w = float.Parse(tokens[0+offset]);//, CultureInfo.InvariantCulture.NumberFormat);
        float x = float.Parse(tokens[1+offset]);
        float y = float.Parse(tokens[2+offset]);
        float z = float.Parse(tokens[3+offset]);
        //unity x y z
        //ardui y z x
        var inputQ = new Quaternion(x, y, z, w);
        rawInputQ = new Quaternion(x, y, z, w);
        rawInputQ = new Quaternion(y, -z, -x, w);
        rawInputQ = new Quaternion(-y, z, x, w);
        if (offset == 0)
        {
            riq1 = rawInputQ;
        }
        else
        {
            riq2 = rawInputQ;
        }
		if (InitPos)
		{
			if (offset == 0)
			{
				goiq1.transform.rotation = rawInputQ;
            } else { 
				goiq2.transform.rotation = rawInputQ;
			}
		} else {
	//iq1, iqi1, dq1, cq1, dqiqi1, iqidq1, tq1, tq12;
			if (offset == 0)
			{
				goiq1.transform.rotation = rawInputQ;
				inputQ = rawInputQ * dqiqi1;
			} else {
				goiq2.transform.rotation = rawInputQ;
				inputQ = rawInputQ * dqiqi2;
			}
		}
        return inputQ;
        //return new Quaternion(x, y, z, w);
    }

	string sharedStr = "initial";
	float timeCount = 1;
	bool InitPos = true;
	bool FirstFrame = true;
    void Update()
    {
		if (FirstFrame)
		{
			FirstFrame = false;
			dq1 = rot(go1);
			dq2 = rot(go2);
			dq3 = rot(go3);
			dq4 = rot(go4);
			setRot(godq1, dq1);
			setRot(godq2, dq2);
			setRot(godq3, dq3);
			setRot(godq4, dq4);
		}

        if (UseThread)
        {
            string lastStr = null;
            if (lastStr != sharedStr)
            {
                Debug.Log(sharedStr);
                lastStr = sharedStr;
            }
		} else {
            OldUpdate();
		}

		if (InitPos)
		{
			if (Time.time - beginTime > 25)
			{
				InitializeOffsets();
                Debug.Log("Finished initialization");
            } else if (Time.time - beginTime > timeCount)
			{
				timeCount += 1;
				Debug.Log("Time: " + timeCount);
			}
		} else {
			//debug
		}
    }
	public void InitializeOffsets()
	{
        InitPos = false;
        // initial * transform = desired
        // inverse(initial) * initial * transform = inverse(initial) * desired
        // transform = inverse(initial) * desired
        // q1Offset = inverse(go transform) * q1;
        setRot(goinitq1, riq1);
        setRot(goinitq2, riq2);
        setRot(goinitq3, riq3);
        setRot(goinitq4, riq4);
        iqi1 = Quaternion.Inverse(riq1);
        iqi2 = Quaternion.Inverse(riq2);
        iqi3 = Quaternion.Inverse(riq3);
        iqi4 = Quaternion.Inverse(riq4);
        dqiqi1 = dq1 * iqi1;
        dqiqi2 = dq2 * iqi2;
        dqiqi3 = dq3 * iqi3;
        dqiqi4 = dq4 * iqi4;

		Debug.Log("Initialized offsets");
    }
	//separate thread read every 40ms
	void PollQuats()
	{
		var str = "test";
		str = ReadFromArduino(.05f);
		float lastTime = Time.time;
		while (true)
		{
			if (Time.time - lastTime < 0.040f)
				continue;
            sharedStr = "40ms has passed";
			lastTime = Time.time;
            str = ReadFromArduino(.1f);
            string[] tokens = str.Split(',');
            //Debug.Log("length: " + tokens.Length);
            //Debug.Log(tokens[0] + " " + tokens[4] + " " + tokens[8]);
			float w = 0;
			if (float.TryParse(tokens[0], out w) == false)
			{
                Debug.Log("Read from arduino: " + str);
				return;
			}
            //Debug.Log("Read from arduino: " + str + " w: " + w);
			sharedStr = "Read from arduino: " + str + " w: " + w;
#if true
		if (tokens.Length == 9)
        {
            //proceed
            go1.transform.rotation = ToQ(tokens, 0);
            go2.transform.rotation = ToQ(tokens, 4);
		}else {
            //Debug.Log("Read from arduino: " + str);
			sharedStr = "Read from arduino: " + str;
		}
#endif
			
        }
    }

    bool firstFlag = true;
    bool setupComplete = false;
	void OldUpdate()
	{
        var str = ReadFromArduino(.01f);
        if (str == null)
        {
            return;
        }
        string[] tokens = str.Split(',');
        //Debug.Log("length: " + tokens.Length);
        //Debug.Log(tokens[0] + " " + tokens[4] + " " + tokens[8]);
        float w = 0;
        if (float.TryParse(tokens[0], out w) == false)
        {
            Debug.Log("1Read from arduino: " + str + " token1: |" + tokens[0] + "|");
            if (tokens[0] == "Setup" && tokens[1] == "complete")//tokens.Length == 9)
            {
                setupComplete = true;
                Debug.Log("Reading from " + float.Parse(tokens[2]) + " sensors");
            }
            return;
        }
        //Debug.Log("Read from arduino: " + str + " w: " + w);
#if true
        if (setupComplete == true)
        {
			if (firstFlag)
			{
				firstFlag = false;
				beginTime = Time.time;
			}
            //proceed
            go1.transform.rotation = ToQ(tokens, 0);
            go2.transform.rotation = ToQ(tokens, 4);
            go3.transform.rotation = ToQ(tokens, 8);
            go4.transform.rotation = ToQ(tokens, 12);
        }
        else
        {
            Debug.Log("2Read from arduino: " + str);
        }
        
#endif
	}
    public string ReadFromArduino(float timeout = 0)
    {
        //stream.ReadTimeout = (int)timeout;
        try
        {
            return stream.ReadLine();
        }
        catch (TimeoutException)
        {
            return null;
        }
    }
    

    public IEnumerator AsynchronousReadFromArduino(Action<string> callback, Action fail = null, float timeout = float.PositiveInfinity)
    {
        DateTime initialTime = DateTime.Now;
        DateTime nowTime;
        TimeSpan diff = default(TimeSpan);

        string dataString = null;

        do
        {
            // A single read attempt
            try
            {
                dataString = stream.ReadLine();
            }
            catch (TimeoutException)
            {
                dataString = null;
            }

            if (dataString != null)
            {
                callback(dataString);
                yield return null;
            } else
                yield return new WaitForSeconds(0.01f);

            nowTime = DateTime.Now;
            diff = nowTime - initialTime;

        } while (diff.Milliseconds < timeout);

        if (fail != null)
            fail();
        yield return null;
    }

    public void Close()
    {
        stream.Close();
    }
}
