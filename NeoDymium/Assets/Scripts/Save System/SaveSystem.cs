using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

//Tutoral is here https://www.youtube.com/watch?v=Rgxbl5uIKO0
public static class SaveSystem
{
	private const string valSeparator = "#SAVED-VALUE#";
	private const string objSeparator = "#SAVED-OBJECT#";

	#region JSON
	public static void Save<T>(T objToSave, string key)
	{
		string savePath = Application.persistentDataPath + "/saves/";
		string json = JsonUtility.ToJson(objToSave);

		File.WriteAllText(savePath + key, json);
	}

	public static void Save<T>(T[] objsToSave, string key)
	{
		string savePath = Application.persistentDataPath + "/saves/";
		string json = string.Empty;
		foreach (T obj in objsToSave) json += JsonUtility.ToJson(obj) + objSeparator;

		File.WriteAllText(savePath + key, json);
	}

	public static T Load<T>(string key)
	{
		string savePath = Application.persistentDataPath + "/saves/";

		string json = File.ReadAllText(savePath + key);
		T objData = JsonUtility.FromJson<T>(json);
		return objData;
	}

	public static T[] LoadObjs<T>(string key)
	{
		string savePath = Application.persistentDataPath + "/saves/";

		string json = File.ReadAllText(savePath + key);
		string[] jsonArr = json.Split(new string[] { objSeparator }, System.StringSplitOptions.None);

		T[] objsData = new T[jsonArr.Length];
		for (int i = 0; i < objsData.Length; i++) objsData[i] = JsonUtility.FromJson<T>(jsonArr[i]);

		return objsData;
	}

	public static bool SaveExists(string key)
	{
		string savePath = Application.persistentDataPath + "/saves/" + key;
		return File.Exists(savePath);
	}

	public static void DeleteSaveProgress()
	{
		string savePath = Application.persistentDataPath + "/saves/";
		DirectoryInfo saveDir = new DirectoryInfo(savePath);
		saveDir.Delete(true);
		Directory.CreateDirectory(savePath);
	}
	#endregion

	#region Through Formatter.Serialize
	/*public static void Save<T>(T objToSave, string key)
	{
		string savePath = Application.persistentDataPath + "/saves/";
		Debug.Log(savePath);
		if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath); //Create a Directory if it does not Exist
		BinaryFormatter formatter = new BinaryFormatter();

		using (FileStream stream = new FileStream(savePath + key, FileMode.Create))
		{
			formatter.Serialize(stream, objToSave);
		}
	}

	public static T Load<T>(string key)
	{
		string savePath = Application.persistentDataPath + "/saves/";
		if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath); //Create a Directory if it does not Exist
		BinaryFormatter formatter = new BinaryFormatter();
		T objData = default(T);

		using (FileStream stream = new FileStream(savePath + key, FileMode.Open)) //Open File Path instead of Create
		{
			objData = (T)formatter.Deserialize(stream);
		}

		return objData;
	}

	public static bool SaveExists(string key)
	{
		string savePath = Application.persistentDataPath + "/saves/" + key;
		return File.Exists(savePath);
	}

	public static void DeleteSaveProgress()
	{
		string savePath = Application.persistentDataPath + "/saves/";
		DirectoryInfo saveDir = new DirectoryInfo(savePath);
		saveDir.Delete(true);
		Directory.CreateDirectory(savePath);
	}*/
	#endregion
}

[System.Serializable]
public class PlayerData
{
	//Player Movement and Position
	public float[] position;
	public float[] rotation;
	public bool isCrouching; 

	//For Hacking
	public bool inHackable;

	//Stealth Gauge
	public float stealthGauge;
	public float prevStealthGauge;
	public float increaseMult;
	public float decreaseMult;

	//Have a Constructor to save the Data based on the Class variables.
	public PlayerData(PlayerController player)
	{
		position = new float[3];
		position[0] = player.transform.position.x;
		position[1] = player.transform.position.y;
		position[2] = player.transform.position.z;

		rotation = new float[3];
		rotation[0] = player.transform.eulerAngles.x;
		rotation[1] = player.transform.eulerAngles.y;
		rotation[2] = player.transform.eulerAngles.z;

		isCrouching = player.isCrouching;

		inHackable = player.inHackable;

		stealthGauge = player.stealthGauge;
		prevStealthGauge = player.prevStealthGauge;
		increaseMult = player.increaseMult;
		decreaseMult = player.decreaseMult;
	}
}

[System.Serializable]
public class HackableData
{
	public float[] position;
	public float[] rotation;

	//Hacking Status
	public bool isHacked;

	public HackableData(IHackable hackable)
	{
		position = new float[3];
		position[0] = hackable.transform.position.x;
		position[1] = hackable.transform.position.y;
		position[2] = hackable.transform.position.z;

		//For now the Script is attached to Pivot of CCTV so it works
		rotation = new float[3];
		rotation[0] = hackable.transform.eulerAngles.x;
		rotation[1] = hackable.transform.eulerAngles.y;
		rotation[2] = hackable.transform.eulerAngles.z;

		isHacked = hackable.hacked;
	}
}

[System.Serializable]
public class ControlPanelData
{
	//Check if the Panel has been shut down.
	public bool disabled;

	public ControlPanelData(bool isDisabled)
	{
		disabled = isDisabled;
	}
}
