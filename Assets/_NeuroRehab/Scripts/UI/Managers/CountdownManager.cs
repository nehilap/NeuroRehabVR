using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountdownManager : MonoBehaviour {

	[SerializeField] private Image countdownImage;
	[SerializeField] private TMP_Text textField;
	[SerializeField] private TMP_Text extraTextField;

	private Fadeable fadeable;

	private void Start() {
		fadeable = gameObject.GetComponent<Fadeable>();
	}

	private IEnumerator countdownCoroutine(float duration) {
		float timePassed = 0f;
		float lastTime = 0f;

		textField.text = $"{(int) (duration - lastTime)}";
		while (timePassed < duration) {
			countdownImage.fillAmount = 1f - (timePassed / duration);
			timePassed += Time.deltaTime;

			if (timePassed - lastTime >= 1f) {
				lastTime++;
				textField.text = $"{(int) (duration - lastTime)}";
			}

			yield return null;
		}

		textField.text = "0";
		countdownImage.fillAmount = 0f;
	}

	public void startCountdown(float duration, string extraText) {
		StopAllCoroutines();

		if (!Mathf.Approximately(fadeable.canvasGroup.alpha, 1f)) {
			StartCoroutine(fadeable.fadeAlpha(0f, 1f, 0.5f));
		}
		extraTextField.text = extraText;
		StartCoroutine(countdownCoroutine(duration));
	}

	public void stopCountdown(string extraText) {
		extraTextField.text = extraText;
		stopCountdown();
	}

	public void stopCountdown() {
		StopAllCoroutines();
	}

	public void hideCountdown() {
		if (Mathf.Approximately(fadeable.canvasGroup.alpha, 0f)) {
			return;
		}
		StartCoroutine(fadeable.fadeAlpha(1f, 0f, 0.5f));
	}
}
