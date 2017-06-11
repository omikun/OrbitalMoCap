using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class OrbitalIO
// : MonoBehaviour
{
	public List<float> ReadCSV(string model, int numSensors)
	{
        string filePath = model + "_" + numSensors.ToString() + "_.csv";
		List<float> ret = new List<float>();
        var rawText = File.ReadAllLines(filePath);
		Debug.Log("Read " + rawText.Length + " lines");
		for (int i = 0; i < rawText.Length; i++)
		{
            string[] tokens = rawText[i].Split(',');
			foreach (var token in tokens)
			{
				ret.Add(float.Parse(token));
			}
		}
		return ret;
	}
    public void Savecsv(string model, int numSensors, List<float> animation)
    {
        Debug.Log("Outputing Saved_data.csv");

        string filePath = model + "_" + numSensors.ToString() + "_.csv";
        string delimiter = ",";

        string[][] output = new string[][]{
             new string[]{"Col 1 Row 1", "Col 2 Row 1", "Col 3 Row 1"},
             new string[]{"Col1 Row 2", "Col2 Row 2", "Col3 Row 2"}
         };

        StringBuilder sb = new StringBuilder();
		int length = animation.Count / (numSensors*4);
		Debug.Log("animation length: " + animation.Count);
		for (int sample = 0; sample < length; sample++)
		{
		//num sample * num sensor * num components per sensor (4 in a quat)
			string[] line = new string[numSensors*4];
			var offset = sample * numSensors * 4;
            for (int i = 0; i < numSensors*4; i++)
			{
				line[i] = animation[offset + i].ToString();
			}
            sb.AppendLine(string.Join(delimiter, line));
		}

        File.AppendAllText(filePath, sb.ToString());
        Debug.Log("Export finished");
		animation.Clear();
    }
}