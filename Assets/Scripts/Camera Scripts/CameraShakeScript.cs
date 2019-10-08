using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShakeScript : MonoBehaviour
{
	private Transform cameraShakePivot;
	private IEnumerator shakeCoroutine;
	

	void Awake()
	{
		cameraShakePivot = transform;
		GlobalData.PlayerCamera = transform.GetComponent<Camera>();
		GlobalData.CameraShakeScript = this;
	}

	void Start () 
	{
		shakeCoroutine = null;
	}
	


	public void ShakeCamera(float shakeDistance, float shakeDuration)
	{
		if (shakeDistance <= 0 || shakeDuration <= 0 || shakeCoroutine != null )
		{
			return;
		}

		shakeCoroutine = Shake(shakeDistance,shakeDuration);
		StartCoroutine(shakeCoroutine);
	}
	
	IEnumerator Shake(float shakeDistance, float shakeDuration)
	{
		int shakeBounces = (int) (shakeDuration/0.05f);
		int currentBounce = 0;

		while (currentBounce <= shakeBounces)
		{
			Vector3 start = cameraShakePivot.localPosition;
			Vector3 target;

			if (currentBounce == shakeBounces)
			{
				target = Vector3.zero;
			}
			else
			{
				float angle = Random.value*Mathf.PI*2f;
				target = new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0f) * shakeDistance;
			}

			float timer = 0f;

			while (timer <= 0.05f)
			{
				timer += Time.deltaTime;
				cameraShakePivot.localPosition = Vector3.Lerp(start, target, timer/0.05f);

				yield return new WaitForEndOfFrame();
			} 

			currentBounce ++;
		}
		
		StopCoroutine(shakeCoroutine);
		shakeCoroutine = null;
	}
}
