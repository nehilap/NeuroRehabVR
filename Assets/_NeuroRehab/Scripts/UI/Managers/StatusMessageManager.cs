using System.Collections;
using Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusMessageManager : MonoBehaviour {

	[SerializeField] private TMP_Text textField;
	[Header("Backgrounds")]
	[SerializeField] private Image okBackground;
	[SerializeField] private Image warningBackground;
	[SerializeField] private Image normalBackground;

	[SerializeField] private float fadeDuration = 0.5f;
	[SerializeField] private float messageDuration = 3f;

	private Coroutine coroutine;
	private Fadeable fadeable;

	void Start() {
		fadeable = gameObject.GetComponent<Fadeable>();

		hideBackgrounds();
	}

	private IEnumerator statusMessageCoroutine(string message) {
		textField.text = message;
		if (!Mathf.Approximately(fadeable.canvasGroup.alpha, 1f)) {
			yield return StartCoroutine(fadeable.fadeAlpha(0f, 1f, fadeDuration));
		}

		yield return new WaitForSecondsRealtime(messageDuration);
		yield return StartCoroutine(fadeable.fadeAlpha(1f, 0f, fadeDuration));
	}

	public void showMessage(string message, MessageType messageType) {
		if (coroutine != null) {
			StopCoroutine(coroutine);
		}
		hideBackgrounds();
		switch (messageType) {
			case MessageType.OK:
				okBackground.enabled = true;
				break;
			case MessageType.WARNING:
				warningBackground.enabled = true;
				break;
			case MessageType.NORMAL:
				normalBackground.enabled = true;
				break;
			default:
				break;
		}
		coroutine = StartCoroutine(statusMessageCoroutine(message));
	}

	public void hideMessage() {
		StartCoroutine(fadeable.fadeAlpha(1f, 0f, fadeDuration));
		textField.text = "";
	}

	private void hideBackgrounds() {
		okBackground.enabled = false;
		warningBackground.enabled = false;
		normalBackground.enabled = false;
	}
}
