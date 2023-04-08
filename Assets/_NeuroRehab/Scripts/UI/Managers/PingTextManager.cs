using Mirror;
using TMPro;
using UnityEngine;

public class PingTextManager : MonoBehaviour {

	private TMP_Text textField;

	void Start() {
		textField = GetComponent<TMP_Text>();
	}

	// Update is called once per frame
	void FixedUpdate() {
		textField.text = "RTT: " + Mathf.Round((float)(NetworkTime.rtt * 1000)) + "ms";
	}
}
