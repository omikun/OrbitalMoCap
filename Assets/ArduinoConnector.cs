
﻿/* ArduinoConnector by Alan Zucconi
 * http://www.alanzucconi.com/?p=2979
 */
using UnityEngine;
using System;
using System.Collections;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;


public class ArduinoConnector : MonoBehaviour {

    /* The serial port where the Arduino is connected. */
    [Tooltip("The serial port where the Arduino is connected")]
    public string port = "/dev/cu.usbmodem1421";
    /* The baudrate of the serial port. */
    [Tooltip("The baudrate of the serial port")]
    public int baudrate = 115200;
    private SerialPort stream;
	public bool UseThread = false;
    public bool UseRawInput = false;
    public Sensor[] sensors = new Sensor[4];
	float beginTime;
	private Thread t1;
    OrbitalIO ae = new OrbitalIO();
    List<float> animationStore = new List<float>();
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

        if (animationStore == null)
        {
            Debug.Log("Animation list not initialized not found");
        }
    }
    

    public void WriteToArduino(string message)
    {
        // Send the request
        stream.WriteLine(message);
        stream.BaseStream.Flush();
    }
	

	string sharedStr = "initial";
	float timeCount = 1;
	bool InitPos = true;
	bool FirstFrame = true;
    int frameNum = 0;
    bool readCSV = true;
    void Update()
    {
        if (readCSV)
        {
            animationStore = ae.ReadCSV("unitychan-test1");
            readCSV = false;
            return;
        } else {
            var i = frameNum * numSensors * 4;
            foreach (var sensor in sensors)
            {
                var quat = new Quaternion(animationStore[i+1],
                                          animationStore[i+2],
                                          animationStore[i+3],
                                          animationStore[i+0]
                );
                sensor.goDebug.transform.rotation = quat;
                i += 4;
            }
            frameNum++;
        }
		if (FirstFrame)
		{
			FirstFrame = false;
            foreach (var sensor in sensors)
            {
                sensor.SetDesiredQ();
            }
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

        InitPos = false;
		if (InitPos)
		{
			if (Time.time - beginTime > 25)
			{
                InitializeOffsets(); //also called from Master.cs
                Debug.Log("Finished initialization");
            } else if (Time.time - beginTime > timeCount)
			{
				timeCount += 1;
				Debug.Log("Time: " + timeCount);
			}
		} else {
			if (Time.time - beginTime > 10)
            {
                ae.Savecsv("unitychan-test1", numSensors, animationStore);
                beginTime = Time.time;
            }
		}
    }
    public void InitializeOffsets()
    {
        InitPos = false;
        foreach (var sensor in sensors)
        {
            sensor.InitializeOffset();
        }
    }
	
	//separate thread read every 40ms
    bool setupComplete = false;
    int numSensors = 4;
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
        if (!setupComplete && float.TryParse(tokens[0], out w) == false)
        {
            Debug.Log("1Read from arduino: " + str + " token1: |" + tokens[0] + "|");
            if (tokens[0] == "Setup" && tokens[1] == "complete")//tokens.Length == 9)
            {
                setupComplete = true;
                //TODO limit reads to only available sensors
                numSensors = (int)float.Parse(tokens[2]);
                Debug.Log("Setup complete! W/ " + numSensors + " sensors");
            }
            return;
        }
        //Debug.Log("Read from arduino: " + str + " w: " + w);
        if (setupComplete == true)
        {
            //proceed
            int i = 0;
            foreach (var sensor in sensors)
            {
                //sensor.go.transform.rotation = ToQ(tokens, i);
                sensor.Update(tokens, i*4, InitPos|UseRawInput);
                i++;
            }
            foreach (var sensor in sensors)
            {
                sensor.AddTo(animationStore);
            }
        }
        else
        {
            Debug.Log("2Read from arduino: " + str);
        }
	}
    public string ReadFromArduino(float timeout = 0)
    {
        //stream.ReadTimeout = (int)timeout;
        try
        {
            //TODO stream.Read and check for buffer size
            return stream.ReadLine();
        }
        catch (TimeoutException)
        {
            return null;
        }
    }
    
    ///////////////////////////////////////////////////
    ///////////////////////////////////////////////////
    //  Asynchronous operation via separate thread
    //  better to use asynchronous serial read instead
    ///////////////////////////////////////////////////
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
#if false
		if (tokens.Length == 9)
        {
            //proceed //update me
            //go1.transform.rotation = ToQ(tokens, 0);
            //go2.transform.rotation = ToQ(tokens, 4);
		}else {
            //Debug.Log("Read from arduino: " + str);
			sharedStr = "Read from arduino: " + str;
		}
#endif
			
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
