using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Components used to change meshes and armature to fit current object.
/// </summary>
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

	/// <summary>
	/// Method to change actual Avatar model. We replace mesh instead of changing whole object, otherwise it would break bunch of stuff + we would have to make perfect setup for every single Avatar variation (ineffective).
	/// </summary>
	/// <param name="model">Model that will be used</param>
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
				if(System.Array.IndexOf(excludedNames, item.name) != -1) {
					continue;
				}
				if(bodyPart.name.Equals(item.name) && item.GetComponent<SkinnedMeshRenderer>() != null) {
					updateMesh(bodyPart.GetComponent<SkinnedMeshRenderer>(), item.GetComponent<SkinnedMeshRenderer>());
					flag = item.gameObject.activeSelf;
					break;
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

		Debug.Log($"{transform.root.name} - Model succesfully changed {modelToUse.name}");
	}

	/// <summary>
	/// We update mesh, bones and materials
	/// </summary>
	/// <param name="origin"></param>
	/// <param name="target"></param>
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

	/// <summary>
	/// Load all objects into variable, so that we don't have to do it every time
	/// </summary>
	private void setupAvatarParts() {
		currentAvatarObjects.Clear();
		foreach (Transform item in transform) {
			if(System.Array.IndexOf(excludedNames, item.name) == -1) {
				currentAvatarObjects.Add(item);
			}
		}
	}
}
