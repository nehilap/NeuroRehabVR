using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fadeable : MonoBehaviour {

	public CanvasGroup canvasGroup {get; private set;}

	private void Start() {
		canvasGroup = gameObject.GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
	}

	public IEnumerator fadeAlpha(float startLerpValue, float endLerpValue, float lerpDuration) {
		float lerpTimeElapsed = 0f;

		while (lerpTimeElapsed < lerpDuration) {
			float t = lerpTimeElapsed / lerpDuration;
			canvasGroup.alpha = Mathf.Lerp(startLerpValue, endLerpValue, t);
			lerpTimeElapsed += Time.deltaTime;
			yield return null;
		}
		// lerp never reaches endValue, that is why we have to set it manually
		canvasGroup.alpha = endLerpValue;
	}
}
