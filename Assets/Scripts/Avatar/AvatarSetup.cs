using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Animations.Rigging;
using System;

public class AvatarSetup : MonoBehaviour {

    [SerializeField]
    private GameObject modelToUse;

    [SerializeField]
    private List<Transform> currentAvatarObjects = new List<Transform>();

    [SerializeField]
    private Transform rootBone;

    [SerializeField]
    private string[] excludedNames;

    void Awake() {
        /*
        if(modelToUse != null) {
            setupModel(modelToUse);
        }
        */

        setupAvatarParts();
    }

    public void setupModel(GameObject model) {
        if(modelToUse != null) {
            GameObject.Destroy(modelToUse);
        }

        modelToUse = GameObject.Instantiate(model) as GameObject;

        /*
        currentAvatarObjects.Clear();
        foreach (Transform item in transform) {
            if(System.Array.IndexOf(excludedNames, item.name) == -1) {
                currentAvatarObjects.Add(item);
            }
        }
        */
        Transform newModelTransform = modelToUse.GetComponent<Transform>();

        bool flag;
        foreach(Transform bodyPart in currentAvatarObjects) {
                Debug.Log("bodypart " + bodyPart.name);
            flag = false;
            bodyPart.gameObject.SetActive(true);
            foreach (Transform item in newModelTransform) {
                Debug.Log("part " + item.name);
                if(System.Array.IndexOf(excludedNames, item.name) == -1) {
                    if(bodyPart.name.Equals(item.name) && item.GetComponent<SkinnedMeshRenderer>() != null) {
                        updateMesh(bodyPart.GetComponent<SkinnedMeshRenderer>(), item.GetComponent<SkinnedMeshRenderer>(), rootBone);
                        flag = true;
                        break;
                    }
                }
            }
            if (flag) {
                continue;
            }
            bodyPart.gameObject.SetActive(false);
        }
        modelToUse.SetActive(false);

        Debug.Log("Model succsefully changed " + modelToUse.name);
    }

    private void updateMesh(SkinnedMeshRenderer origin, SkinnedMeshRenderer target, Transform skeletonRoot) {
        origin.sharedMesh = target.sharedMesh;
        origin.sharedMaterials = target.sharedMaterials;

        Dictionary<string, Transform> allBones = new Dictionary<string, Transform>(); // you can just cache this (and consequently the foreach below) or pass through parameter if in a static context. Leaving here for simplicity
        var childrenBones = skeletonRoot.GetComponentsInChildren<Transform>();
        foreach(Transform b in childrenBones) {
            allBones.Add(b.name, b);
        }

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
