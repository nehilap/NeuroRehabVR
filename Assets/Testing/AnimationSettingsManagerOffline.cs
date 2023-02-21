using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Enums;
using System.Collections.Generic;
using Mappings;
using Utility;

[System.Serializable]
public class AnimationSettingsManagerOffline : MonoBehaviour {
	public float armMoveDuration = 1.5f;
	
	public float handMoveDuration = 1.5f;

	public float waitDuration = 10f;

	public float moveDuration = 4f;

	public int repetitions = 5;

	public AnimationType animType = AnimationType.Block;

	public TMP_Text armMoveTextValue;
	public Slider armMoveSlider;

	public TMP_Text handMoveTextValue;
	public Slider handMoveSlider;

	public TMP_Text waitDurTextValue;
	public Slider waitDurSlider;

	public TMP_Text moveDurTextValue;
	public Slider moveDurSlider;

	public TMP_Text repetitionsTextValue;
	public Slider repetitionsSlider;

	public TMP_Dropdown animTypeDropdown;
	
	// https://mirror-networking.gitbook.io/docs/guides/synchronization/synclists
	public List<PosRotMapping> blockSetup = new List<PosRotMapping>();
	public List<PosRotMapping> cubeSetup = new List<PosRotMapping>();
	public List<PosRotMapping> cupSetup = new List<PosRotMapping>();
	public List<PosRotMapping> keySetup = new List<PosRotMapping>();

	[SerializeField] private Transform markerParent;
	[SerializeField] private List<GameObject> markerPrefabs = new List<GameObject>();
	[SerializeField] private List<GameObject> spawnedMarkers = new List<GameObject>();

	[SerializeField] private GameObject targetObject;

	public void Start() {
		getCurrentAnimationSetup().Add(new PosRotMapping(targetObject.transform));

		if (animType == AnimationType.Key) {
			getCurrentAnimationSetup().Add(new PosRotMapping(ObjectManager.Instance.getFirstObjectByName("Lock").GetComponent<TargetUtility>().customTargetPos.transform));
		}
		
		setupMarkers();
	}

	public void setAnimType(AnimationType _animType) {
		animType =_animType;
		
		changeAnimTypeValue(AnimationType.Off, AnimationType.Off);
	}

	public List<PosRotMapping> getCurrentAnimationSetup() {
		switch (animType) {
			case AnimationType.Block:
				return blockSetup;
			case AnimationType.Cube: 
				return cubeSetup;
			case AnimationType.Cup: 
				return cupSetup;
			case AnimationType.Key: 
				return keySetup;
			default: return null; // AnimType.Off
		}
	}

	public List<PosRotMapping> getAnimationSetupByAnimType(AnimationType _animType) {
		switch (_animType) {
			case AnimationType.Block:
				return blockSetup;
			case AnimationType.Cube: 
				return cubeSetup;
			case AnimationType.Cup: 
				return cupSetup;
			case AnimationType.Key: 
				return keySetup;
			default: return null; // AnimType.Off
		}
	}

	/*
	* FUNCTIONS FOR SETTING ELEMENTS VALUES
	*/

	private void changeArmMoveDurationElements(float _old, float _new) {
		armMoveTextValue.text = (Mathf.Round(armMoveDuration * 10) / 10).ToString("F1") + " s";

		armMoveSlider.value = (int) (armMoveDuration * 2);
	}

	private void changeHandMoveDurationElements(float _old, float _new) {
		handMoveTextValue.text = (Mathf.Round(handMoveDuration * 10) / 10).ToString("F1") + " s";

		handMoveSlider.value = (int) (handMoveDuration * 2);
	}

	private void changeWaitDurationElements(float _old, float _new) {
		waitDurTextValue.text = (Mathf.Round(waitDuration * 10) / 10).ToString("F1") + " s";

		waitDurSlider.value = (int) (waitDuration * 2);
	}
	private void changeMoveDurationElements(float _old, float _new) {
		moveDurTextValue.text = (Mathf.Round(moveDuration * 10) / 10).ToString("F1") + " s";

		moveDurSlider.value = (int) (moveDuration * 2);
	}
	
	private void changeRepetitionsElements(int _old, int _new) {
		repetitionsTextValue.text = repetitions + " x";

		repetitionsSlider.value = repetitions;
	}

	private void changeAnimTypeValue(AnimationType _old, AnimationType _new) {
		animTypeDropdown.value = animTypeDropdown.options.FindIndex(option => option.text == animType.ToString());

		setupMarkers();
	}

	private void setAllElements() {
		armMoveTextValue.text = (Mathf.Round(armMoveDuration * 10) / 10).ToString("F1") + " s";
		armMoveSlider.value = (int) (armMoveDuration * 2);

		handMoveTextValue.text = (Mathf.Round(handMoveDuration * 10) / 10).ToString("F1") + " s";
		handMoveSlider.value = (int) (handMoveDuration * 2);

		waitDurTextValue.text = (Mathf.Round(waitDuration * 10) / 10).ToString("F1") + " s";
		waitDurSlider.value = (int) (waitDuration * 2);

		moveDurTextValue.text = (Mathf.Round(moveDuration * 10) / 10).ToString("F1") + " s";
		moveDurSlider.value = (int) (moveDuration * 2);

		repetitionsTextValue.text = repetitions + " x";
		repetitionsSlider.value = repetitions;

		animTypeDropdown.value = animTypeDropdown.options.FindIndex(option => option.text == animType.ToString());
	}

	private void setupMarkers() {
		spawnedMarkers.Clear();
		List<PosRotMapping> currentMapping = getCurrentAnimationSetup();

		if (currentMapping.Count > 0) {
			GameObject marker = GameObject.Instantiate(markerPrefabs[0], currentMapping[0].position, Quaternion.Euler(currentMapping[0].rotation), markerParent) as GameObject;
			spawnedMarkers.Add(marker);

			for (int i = 1; i < currentMapping.Count; i++) {
				marker = GameObject.Instantiate(markerPrefabs[1], currentMapping[i].position, Quaternion.Euler(currentMapping[i].rotation), markerParent) as GameObject;
				spawnedMarkers.Add(marker);
			}
		}
	}

	/*
	* HANDLERS for assigning them in editor
	*/

	public void armDurationSliderHandler(float value) {
		armMoveDuration = value / 2f;

		changeArmMoveDurationElements(value, value);
	}

	public void handDurationSliderHandler(float value) {
		handMoveDuration = value / 2f;

		changeHandMoveDurationElements(value, value);
	}

	public void waitDurationSliderHandler(float value) {
		waitDuration = value / 2f;

		changeWaitDurationElements(value, value);
	}

	public void moveDurationSliderHandler(float value) {
		moveDuration = value / 2f;

		changeMoveDurationElements(value, value);
	}

	// We have to take in float values, because that's default type that slider works with
	public void repetitionsSliderHandler(float value) {
		repetitions = (int) value;

		changeRepetitionsElements((int) value, (int) value);
	}

	public void animationTypeDropdownHandler(TMP_Dropdown dropdown) {
		AnimationType oldAnimType = animType;

		switch (dropdown.options[dropdown.value].text)
		{
			case "Off": animType = AnimationType.Off; break;
			case "Cube": animType = AnimationType.Cube; break;
			case "Cup": animType = AnimationType.Cup; break;
			case "Key": animType = AnimationType.Key; break;
			case "Block": animType = AnimationType.Block; break;
			default: break;
		}
	}
}
