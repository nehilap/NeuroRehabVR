using UnityEngine;

public class MenuListMember : MonoBehaviour {

    [SerializeField] private string menuName;

    void Start() {
        if (menuName.Trim().Length != 0) {
            MenuManager.Instance.addMenuToList(menuName.Trim(), transform.gameObject);
        } else {
            MenuManager.Instance.addMenuToList(transform.name.Trim(), transform.gameObject);
        }
    }
}
