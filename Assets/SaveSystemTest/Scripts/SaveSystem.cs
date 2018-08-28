
using System;
using System.IO;
using UnityEngine;


public class SaveSystem : MonoBehaviour
{
	// Used to encrypt the savefiles to make them harder to modify.
	// If multiple saves are planned to be stored, this values should differ per file.
	static readonly string JSON_ENCRYPTED_KEY = "D@QUfKAqLw5^2fO#lss(!Fpkl8[h&BJ=";
	static readonly string JSON_ENCRYPTED_IV = "ytqF=5WKcqG&*lgx";
	private string filename = "saveFile.dat";
	private string saveFilePath; 
	private SaveFile saveFile;
	private AesEncryptor encryptor;


	void Awake()
	{
		//saveFilePath = Path.Combine(Application.persistentDataPath,"saveFile.json");
		saveFilePath = Path.Combine(Application.persistentDataPath,filename);
		saveFile = new SaveFile();
		encryptor = new AesEncryptor();


		if (!File.Exists(saveFilePath))
		{
			saveFile.saveVersion = 1.00f;
			saveFile.creationDate = Convert.ToString(DateTime.Now);
			saveFile.saveDate = Convert.ToString(DateTime.Now);
			// File.WriteAllText(saveFilePath, saveFile.GetJson());

			byte[] encryptedSaveFile = encryptor.Encrypt(saveFile.GetJson(), JSON_ENCRYPTED_KEY,JSON_ENCRYPTED_IV);
			File.WriteAllBytes(saveFilePath,encryptedSaveFile);
		}
		else
		{
			Load();
		}
		
	}


	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			Save();
		}

		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			Load();
		}
		if(Input.GetMouseButtonDown(0))
		{
			Debug.LogFormat("Save Version: {0}. Creation Date: {1}. Save Date: {2}.", saveFile.saveVersion ,saveFile.creationDate, saveFile.saveDate);
		}
	}



	void Save()
	{
		if (File.Exists(saveFilePath))
		{
			saveFile.saveDate = Convert.ToString(DateTime.Now);

			//File.WriteAllText(saveFilePath, saveFile.GetJson());
			File.WriteAllBytes(saveFilePath, encryptor.Encrypt(saveFile.GetJson(), JSON_ENCRYPTED_KEY,JSON_ENCRYPTED_IV));
		}
	}

	void Load()
	{
		if (File.Exists(saveFilePath))
		{
			//saveFile.SetFromJson(File.ReadAllText(saveFilePath));
			saveFile.SetFromJson(encryptor.Decrypt(File.ReadAllBytes(saveFilePath), JSON_ENCRYPTED_KEY,JSON_ENCRYPTED_IV));
		}
	}
}
