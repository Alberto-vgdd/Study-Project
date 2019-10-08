using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIScript : MonoBehaviour
{
	private Animator animator;
	public Text messageText;


	private const string gameFadeOut = "GameFadeOut";
	private const string gameFadeIn = "GameFadeIn";
	private const string displayMessage = "DisplayMessage";
	private const string displayPauseMenu = "DisplayPauseMenu";
	private const string hidePauseMenu = "HidePauseMenu";

	// Variables to display messages.
	private Coroutine displayMessageCoroutine;
	private float displayMessageTime = 3.5f;
	private Queue<string> messages;


	// Use this for initialization
	void Awake ()
	{
		GlobalData.GameUIScript = this;
		animator = GetComponent<Animator>();
		messages = new Queue<string>();
	}


	public void StartGameFadeOut()
	{
		animator.SetTrigger(gameFadeOut);
	}

	public void StartGameFadeIn()
	{
		animator.SetTrigger(gameFadeIn);
	}

	IEnumerator DisplayMessage()
	{
		while (messages.Count > 0)
		{
			string message = messages.Dequeue();
			messageText.text = message;
			animator.SetTrigger(displayMessage);

			yield return new WaitForSeconds(displayMessageTime);
		}
		
		StopCoroutine(displayMessageCoroutine);
		displayMessageCoroutine = null;

		yield return null;
	}


	public void DisplayMessage(string messageText)
	{
		messages.Enqueue(messageText);
		
		if (displayMessageCoroutine == null)
		{
			displayMessageCoroutine = StartCoroutine(DisplayMessage());
		}
	}

	
	public void DisplayPauseMenu()
	{
		animator.SetTrigger(displayPauseMenu);
	}

	public void HidePauseMenu()
	{
		animator.SetTrigger(hidePauseMenu);
	}


	// Methods for the Pause Menu Buttons
	public void ContinueGame()
	{
		GlobalData.GameManager.ContinueGame();
	}

	public void ExitGame()
	{
		GlobalData.GameManager.ExitGame();
	}

}
