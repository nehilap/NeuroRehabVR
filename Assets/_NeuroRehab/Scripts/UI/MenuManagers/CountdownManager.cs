using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountdownManager : MonoBehaviour {

	[SerializeField] private Image countdownImage;
	[SerializeField] private TMP_Text textField;

	private Coroutine coroutine;

	private CanvasGroup canvasGroup;

	private void Start() {
		canvasGroup = gameObject.GetComponent<CanvasGroup>();

		canvasGroup.alpha = 0f;
	}

	private IEnumerator countdownCoroutine(float duration) {
		float normalizedTime = 0;

		while(normalizedTime <= 1f) {
			countdownImage.fillAmount = normalizedTime;
			normalizedTime += Time.deltaTime / duration;
			yield return null;
		}
	}

	private IEnumerator countdown(float duration) {
		float timePassed = 0f;
		float lastTime = 0f;

		textField.text = "" + (int) (duration - timePassed);
		while (timePassed < duration) {
			countdownImage.fillAmount = 1f - (timePassed / duration);
			timePassed += Time.deltaTime;

			if (timePassed - lastTime >= 1f) {
				lastTime++;
				textField.text = "" + (int) (duration - lastTime);
			}

			yield return null;
		}

		textField.text = "0";
		countdownImage.fillAmount = 0f;
	}

	IEnumerator fadeAlpha(float startLerpValue, float endLerpValue, float lerpDuration) {
		float lerpTimeElapsed = 0f;

		while (lerpTimeElapsed < lerpDuration) {
			float t = lerpTimeElapsed / lerpDuration;
			t = t * t * t * (t * (6f* t - 15f) + 10f); // https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
			canvasGroup.alpha = Mathf.Lerp(startLerpValue, endLerpValue, t);
			lerpTimeElapsed += Time.deltaTime;
			yield return null;
		}
		// lerp never reaches endValue, that is why we have to set it manually
		canvasGroup.alpha = endLerpValue;
	}

	public void startCountdown(float duration) {
		if (coroutine != null) {
			StopCoroutine(coroutine);
		}
		if (!Mathf.Approximately(canvasGroup.alpha, 1f)) {
			StartCoroutine(fadeAlpha(0f, 1f, 0.5f));
		}
		coroutine = StartCoroutine(countdown(duration));
	}

	public void stopCountdown() {
		if (coroutine != null) {
			StopCoroutine(coroutine);
		}
	}

	public void hideCountdown() {
		StartCoroutine(fadeAlpha(1f, 0f, 0.5f));
	}
}
