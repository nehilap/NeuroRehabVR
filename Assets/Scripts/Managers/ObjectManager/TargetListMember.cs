using UnityEngine;

public class TargetListMember : MonoBehaviour
{
	[SerializeField] private string targetName;

	void Awake() {
		string newName = gameObject.name.Trim();
		if (targetName.Trim().Length != 0) {
			newName = ObjectManager.Instance.addTargetToList(targetName.Trim(), transform.gameObject);
		} else {
			newName = ObjectManager.Instance.addTargetToList(gameObject.name.Trim(), transform.gameObject);
		}

		gameObject.name = newName;
	}

	private void OnDestroy() {
		if (ObjectManager.Instance != null) {
			ObjectManager.Instance.removeTargetFromList(gameObject.name.Trim());
		}
	}
}
