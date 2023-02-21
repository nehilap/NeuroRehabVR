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

	[SerializeField] private Dictionary<string, List<GameObject>> objectList = new Dictionary<string, List<GameObject>>();

	/*
	 * OBJECT LIST
	 */

	public List<GameObject> getObjectsByName(string targetName) {
		if (objectList.ContainsKey(targetName)) {
			return objectList[targetName];
		} else {
			return new List<GameObject>();
		}
	}

	public GameObject getFirstObjectByName(string targetName) {
		if (objectList.ContainsKey(targetName)) {
			return objectList[targetName][0];
		} else {
			return null;
		}
	}

	public Dictionary<string, List<GameObject>> getObjectList() {
		return objectList;
	}

	public void addObjectToList(string objectName, GameObject objectToAdd) {
		if (objectList.ContainsKey(objectName)) {
			objectList[objectName].Add(objectToAdd);
		} else {
			List<GameObject> objects = new List<GameObject>();
			objects.Add(objectToAdd);
			objectList.Add(objectName, objects);
		}
	}

	public void removeObjectFromList(string targetName, GameObject objectToRemove) {
		objectList[targetName].Remove(objectToRemove);
	}
}
