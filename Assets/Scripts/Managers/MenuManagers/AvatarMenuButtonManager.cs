using UnityEngine;
using UnityEngine.UI;

public class AvatarMenuButtonManager : MonoBehaviour {
	[SerializeField]
	private AvatarMenuManager avatarMenuManager;

	[SerializeField]
	private GameObject modelToUse;

	[SerializeField]
	private bool isFemale;

	[SerializeField]
	private int avatarNumber;

	private void Start() {
		transform.GetComponent<Button>().onClick.AddListener(setAvatar);
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
