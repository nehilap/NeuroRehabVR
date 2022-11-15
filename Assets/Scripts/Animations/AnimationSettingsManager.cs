using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using Enums;

public class AnimationSettingsManager : NetworkBehaviour
{
	[SyncVar(hook = nameof(changeArmMoveDurationElements))] [Range(0.5f, 5f)]
	public float armMoveDuration = 1.5f;
	
	[SyncVar(hook = nameof(changeHandMoveDurationElements))] [Range(0.5f, 5f)]
	public float handMoveDuration = 1.5f;

	[SyncVar(hook = nameof(changeWaitDurationElements))] [Range(0.5f, 20f)]
	public float waitDuration = 10f;

	[SyncVar(hook = nameof(changeMoveDurationElements))] [Range(0.5f, 10f)]
	public float moveDuration = 4f;

	[SyncVar(hook = nameof(changeAnimTypeValue))]
	public AnimationType animType = AnimationType.Block;

	public TMP_Text armMoveTextValue;
	public Slider armMoveSlider;

	public TMP_Text handMoveTextValue;
	public Slider handMoveSlider;

	public TMP_Text waitDurTextValue;
	public Slider waitDurSlider;

	public TMP_Text moveDurTextValue;
	public Slider moveDurSlider;

	public TMP_Dropdown animTypeDropdown;

	void Start() {
		setAllElements();
	}

	public void setAnimType(AnimationType _animType) {
		animType =_animType;
		CMDUpdateAnimType(_animType);
		
		changeAnimTypeValue(AnimationType.Off, AnimationType.Off);
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

	private void changeAnimTypeValue(AnimationType _old, AnimationType _new) {
		animTypeDropdown.value = animTypeDropdown.options.FindIndex(option => option.text == animType.ToString());
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

		animTypeDropdown.value = animTypeDropdown.options.FindIndex(option => option.text == animType.ToString());
	}

	/*
	* HANDLERS for assigning them in editor
	*/

	public void armDurationSliderHandler(float value) {
		armMoveDuration = value / 2f;

		changeArmMoveDurationElements(0, 0);
		CMDUpdateArmDuration(armMoveDuration);
	}

	public void handDurationSliderHandler(float value) {
		handMoveDuration = value / 2f;

		changeHandMoveDurationElements(0, 0);
		CMDUpdateHandDuration(handMoveDuration);
	}

	public void waitDurationSliderHandler(float value) {
		waitDuration = value / 2f;

		changeWaitDurationElements(0, 0);
		CMDUpdateWaitDuration(waitDuration);
	}

	public void moveDurationSliderHandler(float value) {
		moveDuration = value / 2f;

		changeMoveDurationElements(0, 0);
		CMDUpdateMoveDuration(moveDuration);
	}

	public void animationTypeDropdownHandler(TMP_Dropdown dropdown) {
		switch (dropdown.options[dropdown.value].text)
		{
			case "Off": animType = AnimationType.Off; break;
			case "Cube": animType = AnimationType.Cube; break;
			case "Cup": animType = AnimationType.Cup; break;
			case "Key": animType = AnimationType.Key; break;
			case "Block": animType = AnimationType.Block; break;
			default: break;
		}
		CMDUpdateAnimType(animType);
	}

	/*
	* COMMANDS FOR SERVER
	* requires authority is off because we are firing off these commands from object, not 'player'
	*/

	[Command(requiresAuthority = false)]
	public void CMDUpdateArmDuration(float value) {
		if (armMoveDuration == value) return;

		armMoveDuration = value;
	}

	[Command(requiresAuthority = false)]
	public void CMDUpdateHandDuration(float value) {
		if (handMoveDuration == value) return;

		handMoveDuration = value;
	}

	[Command(requiresAuthority = false)]
	public void CMDUpdateWaitDuration(float value) {
		if (waitDuration == value) return;
		
		waitDuration = value;
	}
	
	[Command(requiresAuthority = false)]
	public void CMDUpdateMoveDuration(float value) {
		if (moveDuration == value) return;
		
		moveDuration = value;
	}

	[Command(requiresAuthority = false)]
	public void CMDUpdateAnimType(AnimationType _animType) {
		if (animType == _animType) return;
		
		animType = _animType;
	}
}
