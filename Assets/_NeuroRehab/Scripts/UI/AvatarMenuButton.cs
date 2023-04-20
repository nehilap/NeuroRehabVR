using UnityEngine;
using UnityEngine.UI;

public class AvatarMenuButton : MonoBehaviour {
	[SerializeField]
	private AvatarMenuManager avatarMenuManager;

	[SerializeField]
	private GameObject modelToUse;

	[SerializeField]
	private bool isFemale;

	[SerializeField]
	private int avatarNumber;

	[SerializeField]
	private Image activeBar;

	private void Start() {
		transform.GetComponent<Button>().onClick.AddListener(setAvatar);

		if (activeBar) {
			activeBar.enabled = false;
		}

		if (SettingsManager.Instance.avatarSettings.avatarNumber == avatarNumber && SettingsManager.Instance.avatarSettings.isFemale == isFemale) {
			setAvatar();
			if (activeBar) {
				activeBar.enabled = true;
			}
		}
	}

	public void setAvatar() {
		foreach (AvatarModelManager manager in avatarMenuManager.avatarModelManagers) {
			SettingsManager.Instance.avatarSettings.avatarNumber = avatarNumber;
			SettingsManager.Instance.avatarSettings.isFemale = isFemale;
			SettingsManager.Instance.avatarSettings.currentModel = modelToUse;

			manager.changeModel(isFemale, modelToUse);
		}
	}
}
