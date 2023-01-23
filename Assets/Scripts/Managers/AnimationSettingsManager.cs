using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using Enums;
using Mappings;

[System.Serializable]
public class AnimationSettingsManager : NetworkBehaviour {
	[SyncVar(hook = nameof(changeArmMoveDurationElements))] [Range(0.5f, 5f)]
	public float armMoveDuration = 1.5f;
	
	[SyncVar(hook = nameof(changeHandMoveDurationElements))] [Range(0.5f, 5f)]
	public float handMoveDuration = 1.5f;

	[SyncVar(hook = nameof(changeWaitDurationElements))] [Range(0.5f, 20f)]
	public float waitDuration = 10f;

	[SyncVar(hook = nameof(changeMoveDurationElements))] [Range(0.5f, 10f)]
	public float moveDuration = 4f;

	[SyncVar(hook = nameof(changeRepetitionsElements))] [Range(1, 20)]
	public int repetitions = 5;

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

	public TMP_Text repetitionsTextValue;
	public Slider repetitionsSlider;

	public TMP_Dropdown animTypeDropdown;

	public readonly SyncList<PosRotMapping> blockSetup = new SyncList<PosRotMapping>();
	public readonly SyncList<PosRotMapping> cubeSetup = new SyncList<PosRotMapping>();
	public readonly SyncList<PosRotMapping> cupSetup = new SyncList<PosRotMapping>();
	public readonly SyncList<PosRotMapping> keySetup = new SyncList<PosRotMapping>();

	public void Start() {
		if (isClientOnly) {
			setAllElements();
			
			blockSetup.Callback += onAnimationSetupUpdated;
			cubeSetup.Callback += onAnimationSetupUpdated;
			cupSetup.Callback += onAnimationSetupUpdated;
			keySetup.Callback += onAnimationSetupUpdated;
		}
	}

	public void setAnimType(AnimationType _animType) {
		animType =_animType;
		NetworkCharacterManager.localNetworkClient.CMDUpdateAnimType(_animType);
		
		changeAnimTypeValue(AnimationType.Off, AnimationType.Off);
	}

	public SyncList<PosRotMapping> getCurrentAnimationSetup() {
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

	public SyncList<PosRotMapping> getAnimationSetupByAnimType(AnimationType _animType) {
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

    void onAnimationSetupUpdated(SyncList<PosRotMapping>.Operation op, int index, PosRotMapping oldItem, PosRotMapping newItem) {
        switch (op) {
            case SyncList<PosRotMapping>.Operation.OP_ADD:
                // index is where it was added into the list
                // newItem is the new item
				Debug.Log("Item added");
                break;
            case SyncList<PosRotMapping>.Operation.OP_INSERT:
                // index is where it was inserted into the list
                // newItem is the new item
                break;
            case SyncList<PosRotMapping>.Operation.OP_REMOVEAT:
                // index is where it was removed from the list
                // oldItem is the item that was removed
                break;
            case SyncList<PosRotMapping>.Operation.OP_SET:
                // index is of the item that was changed
                // oldItem is the previous value for the item at the index
                // newItem is the new value for the item at the index
				Debug.Log("Item changed");
                break;
            case SyncList<PosRotMapping>.Operation.OP_CLEAR:
                // list got cleared
				Debug.Log("List cleared");
                break;
        }
    }

	/*
	* HANDLERS for assigning them in editor
	*/

	public void armDurationSliderHandler(float value) {
		if (isServer) {
			return;
		}

		armMoveDuration = value / 2f;

		changeArmMoveDurationElements(value, value);
		NetworkCharacterManager.localNetworkClient.CMDUpdateArmDuration(armMoveDuration);
	}

	public void handDurationSliderHandler(float value) {
		if (isServer) {
			return;
		}

		handMoveDuration = value / 2f;

		changeHandMoveDurationElements(value, value);
		NetworkCharacterManager.localNetworkClient.CMDUpdateHandDuration(handMoveDuration);
	}

	public void waitDurationSliderHandler(float value) {
		if (isServer) {
			return;
		}

		waitDuration = value / 2f;

		changeWaitDurationElements(value, value);
		NetworkCharacterManager.localNetworkClient.CMDUpdateWaitDuration(waitDuration);
	}

	public void moveDurationSliderHandler(float value) {
		if (isServer) {
			return;
		}

		moveDuration = value / 2f;

		changeMoveDurationElements(value, value);
		NetworkCharacterManager.localNetworkClient.CMDUpdateMoveDuration(moveDuration);
	}

	// We have to take in float values, because that's default type that slider works with
	public void repetitionsSliderHandler(float value) {
		if (isServer) {
			return;
		}

		repetitions = (int) value;

		changeRepetitionsElements((int) value, (int) value);
		NetworkCharacterManager.localNetworkClient.CMDUpdateRepetitions(repetitions);
	}

	public void animationTypeDropdownHandler(TMP_Dropdown dropdown) {
		if (isServer) {
			return;
		}

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
		NetworkCharacterManager.localNetworkClient.CMDUpdateAnimType(animType);

		
		if (CharacterManager.localClient != null) {
			NetworkCharacterManager.localNetworkClient.CMDSpawnCorrectTarget(oldAnimType, animType);
		}
		
	}
}