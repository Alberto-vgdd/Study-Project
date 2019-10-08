using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverTriggerScript : MonoBehaviour 
{
	private string playerTag;

	// Use this for initialization
	void Start ()
	{
		playerTag = GlobalData.PlayerTag;
	}
	
	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag(playerTag) && !GlobalData.PlayerDeath)
		{
			GlobalData.GameManager.StartGameOver();
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.CompareTag(playerTag) && !GlobalData.PlayerDeath)
		{
			GlobalData.GameManager.StartGameOver();
		}
	}
}
