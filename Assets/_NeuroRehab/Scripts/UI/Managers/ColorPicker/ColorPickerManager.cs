using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorPickerManager : MonoBehaviour, IPointerClickHandler {
	[SerializeField] private RectTransform colorPickerRectTransform;
	[SerializeField] private Texture2D colorSprite;
	[SerializeField] private List<Image> targetImages = new List<Image>();
	[SerializeField] List<GameObject> colorPaletteObjects = new List<GameObject>();
	private bool isShowing = false;

	private Color chosenColor = Color.white;
	public Color ChosenColor { get => chosenColor; set {
			chosenColor = value;
			if (OnColorChange != null)
				OnColorChange();
		}
	}

	public delegate void OnColorChangeDelegate();
	public event OnColorChangeDelegate OnColorChange;

	void Start() {
		changeElementsColor();
		setColorPaletteVisibility();
	}

	public void OnPointerClick(PointerEventData eventData) {
		Vector2 clickPosition;
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out clickPosition)) {
			return;
		}
		//Debug.Log(clickPosition);
		//Debug.Log($"{clickPosition.x * (colorSprite.width / colorPickerRectTransform.sizeDelta.x)}, {clickPosition.y * (colorSprite.height / colorPickerRectTransform.sizeDelta.y)}");

		Vector2 scaledPosition = new Vector2(clickPosition.x * (colorSprite.width / colorPickerRectTransform.sizeDelta.x), clickPosition.y * (colorSprite.height / colorPickerRectTransform.sizeDelta.y));

		ChosenColor = colorSprite.GetPixel((int) scaledPosition.x, (int) scaledPosition.y);
		//Debug.Log(color);
		changeElementsColor();
	}

	private void changeElementsColor() {
		foreach (Image targetImage in targetImages) {
			targetImage.color = ChosenColor;
		}
	}

	public void triggerColorPaletter() {
		isShowing = !isShowing;

		setColorPaletteVisibility();
	}

	private void setColorPaletteVisibility() {
		if (isShowing) {
			foreach (GameObject colorPaletteObject in colorPaletteObjects) {
				colorPaletteObject.SetActive(true);
			}
		} else {
			foreach (GameObject colorPaletteObject in colorPaletteObjects) {
				colorPaletteObject.SetActive(false);
			}
		}
	}

}
