using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SimpleObjectListMember {
	public string name;
	public GameObject gameObject;
}

public class SimpleObjectListManager : MonoBehaviour {
	[NonReorderable] [SerializeField] public List<SimpleObjectListMember> objectList = new List<SimpleObjectListMember>();
}
