using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCameraTrigger : MonoBehaviour 
{
	private FixedCameraMovementScript camera2D;
	private FreeCameraMovementScript camera3D;
	private string playerTag;

	// Use this for initialization
	void Start () 
	{
		camera2D = GlobalData.FixedCameraMovementScript;
		camera3D = GlobalData.FreeCameraMovementScript;
		playerTag = GlobalData.PlayerTag;
	}

	void OnTriggerEnter(Collider other)
	{
		if ( !camera3D.enabled && other.CompareTag(playerTag))
		{
			camera2D.enabled = false;

			camera3D.enabled = true;
			camera3D.StartCameraTransition();
		}
	}
}
