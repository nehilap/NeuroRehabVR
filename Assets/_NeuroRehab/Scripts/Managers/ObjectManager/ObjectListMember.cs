using UnityEngine;

/// <summary>
/// Component used to register to ObjectManager. Uses specific name or GameObject name if not specified.
/// </summary>
public class ObjectListMember : MonoBehaviour {
	[SerializeField] private string objectName;

	void Awake() {
		if (objectName == null || objectName.Trim().Length != 0) {
			ObjectManager.Instance.addObjectToList(objectName.Trim(), transform.gameObject);
		} else {
			ObjectManager.Instance.addObjectToList(gameObject.name.Trim(), transform.gameObject);
		}
	}

	private void OnDestroy() {
		if (ObjectManager.Instance != null) {
			if (objectName == null || objectName.Trim().Length != 0) {
				ObjectManager.Instance.removeObjectFromList(objectName.Trim(), transform.gameObject);
			} else {
				ObjectManager.Instance.removeObjectFromList(gameObject.name.Trim(), transform.gameObject);
			}
		}
	}
}
