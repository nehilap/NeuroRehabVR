using UnityEngine;

public class AvatarMenuManager : MonoBehaviour
{
	[SerializeField]
	public AvatarModelManager[] avatarModelManagers;

	public void resetHeight() {
		foreach (AvatarModelManager manager in avatarModelManagers) {
			manager.resetHeight();
		}
	}
}
