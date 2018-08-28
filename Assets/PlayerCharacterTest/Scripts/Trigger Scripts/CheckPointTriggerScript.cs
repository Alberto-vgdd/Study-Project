using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointTriggerScript : MonoBehaviour
{
    [Header("Checkpoint Parameters")]
    private string playerTag;
    
    void Start()
    {
        playerTag = GlobalData.PlayerTag;
    }
	
	void OnTriggerEnter(Collider other)
	{
		if ( other.tag.Equals(playerTag) )
		{
			GlobalData.GameManager.UpdateCheckPoint(this.transform, GlobalData.FreeCameraMovementScript.enabled,GlobalData.FixedCameraMovementScript.enabled);
		}
    }

}
