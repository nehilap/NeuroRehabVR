using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour {

    private static MenuManager _instance;

    public static MenuManager Instance { get { return _instance; } }

    [SerializeField] private Dictionary<string, GameObject> menuList = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    public GameObject getMenuByName(string menuName) {
        if (menuList.ContainsKey(menuName)) {
            return menuList[menuName];
        } else {
            return null;
        }
    }

    public Dictionary<string, GameObject> getMenuList() {
        return menuList;
    }

    public void addMenuToList(string menuName, GameObject menuToAdd) {
        menuList.Add(menuName, menuToAdd);
    }
}
