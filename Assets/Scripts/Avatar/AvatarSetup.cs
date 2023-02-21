using UnityEngine;
using System.Collections.Generic;

public class AvatarSetup : MonoBehaviour {

	[SerializeField]
	private GameObject modelToUse;

	[SerializeField]
	private List<Transform> currentAvatarObjects = new List<Transform>();

	[SerializeField]
	private Transform rootBone;

	[SerializeField]
	private string[] excludedNames;

	private Dictionary<string, Transform> allBones;

	void Awake() {
		setupAvatarParts();

		allBones = new Dictionary<string, Transform>();
		var childrenBones = rootBone.GetComponentsInChildren<Transform>();
		foreach(Transform b in childrenBones) {
			allBones.Add(b.name, b);
		}
	}

	void OnDisable() {
		if (modelToUse != null) {
			GameObject.Destroy(modelToUse);
		}
	}

	public void setupModel(GameObject model) {
		if(modelToUse != null) {
			GameObject.Destroy(modelToUse);
		}

		modelToUse = GameObject.Instantiate(model) as GameObject;

		Transform newModelTransform = modelToUse.GetComponent<Transform>();

		bool flag;
		foreach(Transform bodyPart in currentAvatarObjects) {
				// Debug.Log("bodypart " + bodyPart.name);
			flag = false; 
			SkinnedMeshRenderer renderer = bodyPart.GetComponent<SkinnedMeshRenderer>();
			if (renderer != null) {
				renderer.enabled = true;
			}
			foreach (Transform item in newModelTransform) {
				// Debug.Log("part " + item.name);
				if(System.Array.IndexOf(excludedNames, item.name) == -1) {
					if(bodyPart.name.Equals(item.name) && item.GetComponent<SkinnedMeshRenderer>() != null) {
						updateMesh(bodyPart.GetComponent<SkinnedMeshRenderer>(), item.GetComponent<SkinnedMeshRenderer>());
						flag = item.gameObject.activeSelf;
						break;
					}
				}
			}
			if (flag) {
				continue;
			}
			if (renderer != null) {
				renderer.enabled = false;
			}
		}
		GameObject.Destroy(modelToUse);

		Debug.Log("Model succsefully changed " + modelToUse.name);
	}

	private void updateMesh(SkinnedMeshRenderer origin, SkinnedMeshRenderer target) {
		origin.sharedMesh = target.sharedMesh;
		origin.sharedMaterials = target.sharedMaterials;

		var originBones = origin.bones;
		var targetBones = new List<Transform>();
		foreach(Transform b in originBones) {
			if(allBones.TryGetValue(b.name, out var foundBone)) {
				targetBones.Add(foundBone);
			}
		}
		target.bones = targetBones.ToArray();
	}

	private void setupAvatarParts() {
		currentAvatarObjects.Clear();
		foreach (Transform item in transform) {
			if(System.Array.IndexOf(excludedNames, item.name) == -1) {
				currentAvatarObjects.Add(item);
			}
		}
	}
}
