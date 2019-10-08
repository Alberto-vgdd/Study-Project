using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManagerScript : MonoBehaviour 
{
	// Variables used to determine the device in use.
	[Header("Combined Input Parameters")]
	public int DeviceRefreshPerSecond = 5;
	private float refreshFrequency;
	private Vector3 previousMousePosition;

	// Variables the other scripts are going to use
	[Header("Debug Variables")]
	public bool joystickInUse;

	void Start () 
	{
		GlobalData.InputManagerScript = this;
		refreshFrequency = 1f/DeviceRefreshPerSecond;
		InvokeRepeating("IsJoystickInUse",0,refreshFrequency);
	}

	void  IsJoystickInUse()
	{
		if (joystickInUse)
		{
			if (Input.mousePosition != previousMousePosition || (Input.anyKey && !AnyControllerButtonPressed()) )
			{
				joystickInUse =  false;
			}
			
		}
		else
		{ 
			if(Mathf.Abs(Input.GetAxis(InputAxis.ControllerAxes)) > 0.0f || AnyControllerButtonPressed())
			{
				previousMousePosition = Input.mousePosition;
				joystickInUse =  true;
			}
			
		}
	}

	bool AnyControllerButtonPressed()
	{
		return 
		Input.GetKey(KeyCode.JoystickButton0)  ||
		Input.GetKey(KeyCode.JoystickButton1)  ||
		Input.GetKey(KeyCode.JoystickButton2)  ||
		Input.GetKey(KeyCode.JoystickButton3)  ||
		Input.GetKey(KeyCode.JoystickButton4)  ||
		Input.GetKey(KeyCode.JoystickButton5)  ||
		Input.GetKey(KeyCode.JoystickButton6)  ||
		Input.GetKey(KeyCode.JoystickButton7)  ||
		Input.GetKey(KeyCode.JoystickButton8)  ||
		Input.GetKey(KeyCode.JoystickButton9)  ||
		Input.GetKey(KeyCode.JoystickButton10) ||
		Input.GetKey(KeyCode.JoystickButton11) ||
		Input.GetKey(KeyCode.JoystickButton12) ||
		Input.GetKey(KeyCode.JoystickButton13) ||
		Input.GetKey(KeyCode.JoystickButton14) ||
		Input.GetKey(KeyCode.JoystickButton15) ||
		Input.GetKey(KeyCode.JoystickButton16) ||
		Input.GetKey(KeyCode.JoystickButton17) ||
		Input.GetKey(KeyCode.JoystickButton18) ||
		Input.GetKey(KeyCode.JoystickButton19);
	}
}

public static class InputAxis
{
    public const string ControllerAxes = "Controller Axes";
    public const string HorizontalMovement = "Horizontal Movement";
    public const string VerticalMovement = "Vertical Movement";
    public const string HorizontalCamera = "Horizontal Camera";
    public const string VerticalCamera = "Vertical Camera";
    public const string LockOn = "Lock On";
    public const string ChangeTarget = "Change Target";
    public const string Jump = "Jump";
    public const string Run = "Run";
    public const string Menu = "Menu";
    public const string Submit = "Submit";
    public const string Cancel = "Cancel";
}
