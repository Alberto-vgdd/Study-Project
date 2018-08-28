using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringJumpScript : MonoBehaviour 
{
	private string playerTag;
	public float jumpForce = 10;

	// Use this for initialization
	void Start () 
	{
		playerTag = GlobalData.PlayerTag;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            collision.gameObject.GetComponent<Rigidbody>().AddForce(transform.up*jumpForce,ForceMode.VelocityChange);
        }
    }
}
