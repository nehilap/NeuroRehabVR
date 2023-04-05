using UnityEngine;

public class AvatarMenuManager : MonoBehaviour
{
	[SerializeField]
	public AvatarModelManager[] avatarModelManagers;

	public void resetHeight() {
		foreach (AvatarModelManager manager in avatarModelManagers) {
			if (manager.gameObject.activeInHierarchy) {
				manager.resetHeight();
			}
		}
	}
}
