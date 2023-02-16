using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour {

	private static ObjectManager _instance;

	public static ObjectManager Instance {
		get {
			if(_instance == null) {
				_instance = GameObject.FindObjectOfType<ObjectManager>();
			}
			return _instance;
		}
	}

	[SerializeField] private Dictionary<string, GameObject> menuList = new Dictionary<string, GameObject>();
	[SerializeField] private Dictionary<string, GameObject> targetList = new Dictionary<string, GameObject>();

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

	public void removeMenuFromList(string menuName) {
		menuList.Remove(menuName);
	}

	public GameObject getTargetByName(string targetName) {
		if (targetList.ContainsKey(targetName)) {
			return targetList[targetName];
		} else {
			return null;
		}
	}

	public Dictionary<string, GameObject> getTargetList() {
		return targetList;
	}

	public string addTargetToList(string targetName, GameObject targetToAdd) {
		string postfix = "";
		int index = 0;
		while(targetList.ContainsKey(targetName + postfix)) {
			postfix = "" + index;
			index++;
		}

		targetList.Add(targetName + postfix, targetToAdd);
		return targetName + postfix;
	}

	public void removeTargetFromList(string targetName) {
		targetList.Remove(targetName);
	}
}
