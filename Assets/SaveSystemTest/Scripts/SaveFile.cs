using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The SaveFile class contains a series of fields that store the 
/// information necessary to maintain the play session progress. 
/// </summary>
[System.Serializable]
public class SaveFile
{
	public float saveVersion = 0.00f;
	public string creationDate;
	public string saveDate;

	/// <summary>
	/// Updates the SaveFile's values from a Json string.
	/// </summary>
	/// <param name="jsonSaveFile">
	/// Json string of a Save File.
	/// </param>
	public void SetFromJson(string jsonSaveFile)
	{
		JsonUtility.FromJsonOverwrite(jsonSaveFile, this);
	}

	/// <summary>
	/// Returns the SaveFile converted to a Json string.
	/// </summary>
	public string GetJson()
	{
		return JsonUtility.ToJson(this);
	}
}

