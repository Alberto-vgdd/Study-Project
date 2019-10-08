using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Analytics;

public class GameManagerScript : MonoBehaviour 
{
	// Checkpoint varibales, used to restore the state after a death
	private Transform currentCheckPoint;
	private bool checkPointFreeCameraEnabled;
	private bool checkPointFixedCameraEnabled;


	// Initialization
	void Awake()
	{
		if (GlobalData.GameManager == null)
		{
			GlobalData.GameManager = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else if (GlobalData.GameManager != this)
		{
			Destroy(this.gameObject);
		}

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	void Update()
	{
		if (Input.GetButtonDown("Menu"))
		{
			if (!GlobalData.GamePaused)
			{
				PauseGame();
			}
			else
			{
				ContinueGame();
			}
		}

	}

	public void UpdateCheckPoint(Transform newCheckPoint, bool freeCameraEnabled, bool fixedCameraEnabled)
	{
		currentCheckPoint = newCheckPoint;
		checkPointFreeCameraEnabled = freeCameraEnabled;
		checkPointFixedCameraEnabled = fixedCameraEnabled;
	}

	public void StartGameOver()
	{
		StartCoroutine(GameOver());
	}

	IEnumerator GameOver()
	{
		// "Kill" the character
		GlobalData.PlayerMovement.DisableInput();
		GlobalData.PlayerDeath = true;

		// Fade out the game.
		GlobalData.GameUIScript.StartGameFadeOut();

		// Wait for the game to fade out, and then move the character and the camera to the checkpoint's position.
		yield return new WaitForSeconds(1f);
		GlobalData.PlayerTransform.position = GlobalData.PlayerCameraHorizontalPivot.position = currentCheckPoint.position;
		GlobalData.PlayerTransform.rotation = GlobalData.PlayerCameraHorizontalPivot.rotation = currentCheckPoint.rotation;
		
		// Enable/Disable the camera scripts
		GlobalData.FixedCameraMovementScript.enabled = checkPointFixedCameraEnabled;
		GlobalData.FreeCameraMovementScript.enabled = checkPointFreeCameraEnabled;

		if (checkPointFixedCameraEnabled)
		{
			GlobalData.FixedCameraMovementScript.StartCameraTransition();
		}
		if (checkPointFreeCameraEnabled)
		{
			GlobalData.FreeCameraMovementScript.CenterCamera();
		}
		
		// Wait for the camera to move properly to the character position and then fade in.
		yield return new WaitForSeconds(0.5f);
		GlobalData.GameUIScript.StartGameFadeIn();

		// "Revive" the character and show the health in the UI again.
		GlobalData.PlayerMovement.EnableInput();
		GlobalData.PlayerDeath = false;
	
	}
     
    public void ShakeCamera(float shakeDistance, float shakeDuration)
	{
		GlobalData.CameraShakeScript.ShakeCamera(shakeDistance,shakeDuration);
	}

	public void PauseGame()
	{
		GlobalData.GamePaused = true;
		Time.timeScale = 0f;

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		DisableInput();

		GlobalData.GameUIScript.DisplayPauseMenu();
	}

	public void ContinueGame()
	{
		GlobalData.GamePaused = false;
		Time.timeScale = 1f;

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		EnableInput();	

		GlobalData.GameUIScript.HidePauseMenu();
	}

	public void EnableInput()
	{
		GlobalData.PlayerMovement.EnableInput();
		GlobalData.FreeCameraMovementScript.EnableInput();
		GlobalData.FixedCameraMovementScript.EnableInput();
		GlobalData.CameraEnemyTrackerScript.EnableInput();
	}

	public void DisableInput()
	{
		GlobalData.PlayerMovement.DisableInput();
		GlobalData.FreeCameraMovementScript.DisableInput();
		GlobalData.FixedCameraMovementScript.DisableInput();
		GlobalData.CameraEnemyTrackerScript.DisableInput();
	}

	public void ExitGame()
	{
		Application.Quit();
	}
    
}
