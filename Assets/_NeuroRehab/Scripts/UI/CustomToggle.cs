using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class CustomToggle : MonoBehaviour {

	[SerializeField] private Sprite selectedSprite;
	[SerializeField] private Sprite deselectedSprite;

	[SerializeField] private Image background;

	private Toggle toggle;

	void Awake() {
		toggle = gameObject.GetComponent<Toggle>();

		if (!selectedSprite || !deselectedSprite || !background) {
			return;
		}
		toggle.onValueChanged.AddListener(changeGraphic);
		changeGraphic(false);
	}

	private void changeGraphic(bool value) {
		if (toggle.isOn) {
			background.sprite = selectedSprite;
		} else {
			background.sprite = deselectedSprite;
		}
	}
}
