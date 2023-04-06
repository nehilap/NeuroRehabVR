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

	// we use Dictionary thanks to it's O(1) speed in accessing items
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

	/// <summary>
	/// Method to get the first object in list of objects for specific name.
	/// </summary>
	/// <param name="targetName"></param>
	/// <returns></returns>
	public GameObject getFirstObjectByName(string targetName) {
		if (objectList.ContainsKey(targetName)) {
			if (objectList[targetName].Count <= 0) {
				objectList.Remove(targetName);
				return null;
			}
			return objectList[targetName][0];
		} else {
			return null;
		}
	}

	/// <summary>
	/// Method that returns whole Dictionary
	/// </summary>
	/// <returns></returns>
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
