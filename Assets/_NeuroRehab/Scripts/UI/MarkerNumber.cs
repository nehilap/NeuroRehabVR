using UnityEngine;
using TMPro;

/// <summary>
/// Class used for markers and their numbers
/// </summary>
public class MarkerNumber : MonoBehaviour {
	[SerializeField] private TMP_Text textField;
	public string orderString;

	private void Start() {
		textField.text = orderString;
	}

	private void LateUpdate() {
		if (CharacterManager.localClientInstance) {
			textField.transform.rotation = Quaternion.LookRotation(textField.transform.position - CharacterManager.localClientInstance.cameraObject.transform.position);
		}
	}
}
