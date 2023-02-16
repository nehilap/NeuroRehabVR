using UnityEngine;

public class MenuListMember : MonoBehaviour {

	[SerializeField] private string menuName;

	void Start() {
		if (menuName.Trim().Length != 0) {
			ObjectManager.Instance.addMenuToList(menuName.Trim(), transform.gameObject);
		} else {
			ObjectManager.Instance.addMenuToList(transform.name.Trim(), transform.gameObject);
		}
	}
}
