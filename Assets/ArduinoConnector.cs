
﻿/* ArduinoConnector by Alan Zucconi
 * http://www.alanzucconi.com/?p=2979
 */
using UnityEngine;
using System;
using System.Collections;
using System.IO.Ports;
using System.Threading;

public class ArduinoConnector : MonoBehaviour {

    /* The serial port where the Arduino is connected. */
    [Tooltip("The serial port where the Arduino is connected")]
    public string port = "/dev/cu.usbmodem1421";
    /* The baudrate of the serial port. */
    [Tooltip("The baudrate of the serial port")]
    public int baudrate = 115200;
    private SerialPort stream;
	public bool UseThread = false;

	public GameObject go1, go2;
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
    }

    public void WriteToArduino(string message)
    {
        // Send the request
        stream.WriteLine(message);
        stream.BaseStream.Flush();
    }
	Quaternion ToQ(string[] tokens, int offset=0)
    {
        float w = float.Parse(tokens[0+offset]);//, CultureInfo.InvariantCulture.NumberFormat);
        float x = float.Parse(tokens[1+offset]);
        float y = float.Parse(tokens[2+offset]);
        float z = float.Parse(tokens[3+offset]);
        return new Quaternion(x, y, z, w);
    }

	string sharedStr = "initial";
    void Update()
    {
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

	void OldUpdate()
	{
        var str = ReadFromArduino(.05f);
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
#if true
        if (tokens.Length == 9)
        {
            //proceed
            go1.transform.rotation = ToQ(tokens, 0);
            go2.transform.rotation = ToQ(tokens, 4);
        }
        else
        {
            Debug.Log("Read from arduino: " + str);
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
