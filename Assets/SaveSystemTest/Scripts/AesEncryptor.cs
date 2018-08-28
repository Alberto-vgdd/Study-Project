using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class AesEncryptor
{
	public string Decrypt(byte[] encryptedBytes, string key, string iv)
	{
		string decryptedString = "";

		try
		{
			byte[] ivBytes = Encoding.ASCII.GetBytes(iv);
			byte[] keyBytes = Encoding.ASCII.GetBytes(key);
			
			using (Aes myAes = Aes.Create())
			{
				myAes.Key = keyBytes;
				myAes.IV = ivBytes;

				decryptedString = DecryptStringFromBytes_Aes(encryptedBytes, myAes.Key, myAes.IV);
			}

		}
		catch (Exception e)
		{
			Debug.LogFormat("Error: {0}", e.Message);
		}

		return decryptedString;
	}

	public byte[] Encrypt(string decryptedString, string key, string iv)
	{
		byte[] encryptedBytes = null;

		try
		{
			byte[] ivBytes = Encoding.ASCII.GetBytes(iv);
			byte[] keyBytes = Encoding.ASCII.GetBytes(key);
			
			using (Aes myAes = Aes.Create())
			{
				myAes.Key = keyBytes;
				myAes.IV = ivBytes;

				encryptedBytes = EncryptStringToBytes_Aes(decryptedString, myAes.Key, myAes.IV);
			}

		}
		catch (Exception e)
		{
			Debug.LogFormat("Error: {0}", e.Message);
		}

		return encryptedBytes;
	}

	static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
	{
		// Check arguments.
		if (plainText == null || plainText.Length <= 0)
			throw new ArgumentNullException("plainText");
		if (Key == null || Key.Length <= 0)
			throw new ArgumentNullException("Key");
		if (IV == null || IV.Length <= 0)
			throw new ArgumentNullException("IV");
		byte[] encrypted;
		
		// Create an Aes object
		// with the specified key and IV.
		using (Aes aesAlg = Aes.Create())
		{
			aesAlg.Key = Key;
			aesAlg.IV = IV;

			// Create an encryptor to perform the stream transform.
			ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

			// Create the streams used for encryption.
			using (MemoryStream msEncrypt = new MemoryStream())
			{
				using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
				{
					using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
					{
						//Write all data to the stream.
						swEncrypt.Write(plainText);
					}
					encrypted = msEncrypt.ToArray();
				}
			}
		}


		// Return the encrypted bytes from the memory stream.
		return encrypted;

	}
	static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
	{
		// Check arguments.
		if (cipherText == null || cipherText.Length <= 0)
			throw new ArgumentNullException("cipherText");
		if (Key == null || Key.Length <= 0)
			throw new ArgumentNullException("Key");
		if (IV == null || IV.Length <= 0)
			throw new ArgumentNullException("IV");

		// Declare the string used to hold
		// the decrypted text.
		string plaintext = null;

		// Create an Aes object
		// with the specified key and IV.
		using (Aes aesAlg = Aes.Create())
		{
			aesAlg.Key = Key;
			aesAlg.IV = IV;

			// Create a decryptor to perform the stream transform.
			ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

			// Create the streams used for decryption.
			using (MemoryStream msDecrypt = new MemoryStream(cipherText))
			{
				using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
				{
					using (StreamReader srDecrypt = new StreamReader(csDecrypt))
					{

						// Read the decrypted bytes from the decrypting stream
						// and place them in a string.
						plaintext = srDecrypt.ReadToEnd();
					}
				}
			}

		}

		return plaintext;
	}
}
