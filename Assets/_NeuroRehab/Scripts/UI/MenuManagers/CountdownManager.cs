using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountdownManager : MonoBehaviour {

	[SerializeField] private Image countdownImage;
	[SerializeField] private TMP_Text textField;

	private Coroutine coroutine;

	private Canvas canvas;

	private void Start() {
		canvas = gameObject.GetComponent<Canvas>();

		canvas.enabled = false;
	}

	private IEnumerator ImageCountdown(float duration) {
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

	public void startCountdown(float duration) {
		canvas.enabled = true;

		coroutine = StartCoroutine(countdown(duration));
	}

	public void pauseCountdown() {
		if (coroutine != null) {
			StopCoroutine(coroutine);
		}
	}

	public void hideCountdown() {
		canvas.enabled = false;
	}
}
