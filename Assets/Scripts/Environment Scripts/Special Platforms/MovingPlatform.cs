using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour 
{
	private Rigidbody movingPlatform;
	private Transform[] waypoints;

	[Header("Moving Platform Properties")]
	public Transform decorationInScene;
	public float speed;
	public bool cyclic;

	private int increment = 1;
	private int nextPlatform = 1;
	private Vector3 movementDirection;


	void Awake () 
	{
		Transform waypointsParent = transform.GetChild(1);

		movingPlatform = transform.GetChild(0).GetComponent<Rigidbody>();
		waypoints = new Transform[waypointsParent.childCount];

		for(int i = 0; i < waypoints.Length; i++)
		{
			waypoints[i] = waypointsParent.GetChild(i);
		}

		movementDirection = waypoints[nextPlatform].position - movingPlatform.position;
	}

	void Start()
	{
		if (decorationInScene != null)
		{
			decorationInScene.parent = movingPlatform.transform;
		}
	}
	
	void FixedUpdate () 
	{
		movingPlatform.MovePosition(movingPlatform.position + movementDirection*speed*Time.fixedDeltaTime);

		if (Vector3.Distance(movingPlatform.position,waypoints[nextPlatform].position) < 0.05f)
		{
			if (cyclic)
			{

				if (nextPlatform >= waypoints.Length - 1)
				{
					nextPlatform = 0;
				}
				else
				{
					nextPlatform += increment;
				}
			}
			else
			{

				if (nextPlatform >= waypoints.Length -1 || nextPlatform <= 0 )
				{
					increment *= -1;
				}

				nextPlatform += increment;
			}

			movementDirection = waypoints[nextPlatform].position - movingPlatform.position;
		}

		movingPlatform.MoveRotation(movingPlatform.rotation*Quaternion.Euler(Vector3.up*90f*Time.fixedDeltaTime));
	}

	public void PlayerOnTop()
	{
		GlobalData.PlayerTransform.parent = movingPlatform.transform;
	}

	public void PlayerLeftTop()
	{
		GlobalData.PlayerTransform.parent = null;
	}

}
