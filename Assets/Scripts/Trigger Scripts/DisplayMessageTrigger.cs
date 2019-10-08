using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayMessageTrigger : MonoBehaviour 
{
	[Header("Parameters")]
	public string messageToDisplay;
	public bool disableAfterUse;
	private string playerTag;

	void Start()
	{
		playerTag = GlobalData.PlayerTag;
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag(playerTag))
		{
			GlobalData.GameUIScript.DisplayMessage(messageToDisplay);
			
			if (disableAfterUse)
			{
				this.gameObject.SetActive(false);
			}
		}
	}
}
