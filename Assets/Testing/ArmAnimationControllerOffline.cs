using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

using Enums;
using Mappings;
using Utility;
using System.Collections.Generic;

public class ArmAnimationControllerOffline : MonoBehaviour {
	private Enums.AnimationState animState = Enums.AnimationState.Stopped;
	[SerializeField] public bool isLeft;

	[Header("Rigs")]
	[SerializeField] private Rig restArmRig;
	[SerializeField] private Rig armRig;
	[SerializeField] private Rig handRig;

	[Header("Arm objects")]
	[SerializeField] private Renderer[] armObjects;
	[SerializeField] private Renderer[] fakeArmObjects;

	[SerializeField] private TargetsHelper targetsHelperObject;

	[SerializeField] private Vector3 armRestOffset;

	[SerializeField] private bool isAnimationRunning;

	[Header("Objects setup from code")]
	[SerializeField] private GameObject targetObject;

	[SerializeField] private AnimationSettingsManagerOffline animSettingsManager;
	[SerializeField] private GameObject armRestHelperObject;
	[SerializeField] private bool isArmResting = false;
	[SerializeField] private PosRotMapping originalArmRestPosRot;
	[SerializeField] private AnimationMapping animationMapping;

	[SerializeField] private AvatarController avatarController;

	[SerializeField] private MeshFilter armRangeMesh;
	[SerializeField] private float armRangeSlack = 0.01f;

	void Start() {
		animSettingsManager = GameObject.Find("AnimSettingsManagerOffline")?.GetComponent<AnimationSettingsManagerOffline>();

		armRestHelperObject = GameObject.Find("ArmRestHelperObject");

		avatarController = gameObject.GetComponent<AvatarController>();

		animationMapping.resizeMappings(avatarController.sizeMultiplier);
	}

	private void LateUpdate() {
		alignArmRestTargetWithTable();

		if (isAnimationRunning) {
			targetsHelperObject.alignTargetTransforms();
		}
	}

	// https://gamedevbeginner.com/coroutines-in-unity-when-and-how-to-use-them/
	private IEnumerator armStartAnimationLerp(bool isFakeAnimation) {
		// Animation control for moving arm and grabbing with hand
		// we set weight to the corresponding part we're moving
		RigLerp[] rigLerps = {new RigLerp(armRig, 0, 1), new RigLerp(restArmRig, 1, 0)};
		yield return StartCoroutine(multiRigLerp(rigLerps, animSettingsManager.armMoveDuration));

		yield return StartCoroutine(simpleRigLerp(handRig, animSettingsManager.handMoveDuration, 0, 1));

		List<PosRotMapping> currentAnimSetup = animSettingsManager.getCurrentAnimationSetup();

		if (animSettingsManager.animType == AnimationType.Cup) {
			PosRotMapping endMapping = currentAnimSetup[0].Clone();
			endMapping.position += new Vector3(0, 0.2f, 0);

			yield return StartCoroutine(lerpTransform(targetObject, currentAnimSetup[0], endMapping, animSettingsManager.moveDuration / 2, true));
			yield return new WaitForSeconds(0.6f);
			yield return StartCoroutine(lerpTransform(targetObject, endMapping, currentAnimSetup[0], animSettingsManager.moveDuration / 2, true));
		}

		for (int i = 1; i < currentAnimSetup.Count; i++) {
			yield return StartCoroutine(lerpTransform(targetObject, currentAnimSetup[i-1], currentAnimSetup[i], animSettingsManager.moveDuration, true));

			if (animSettingsManager.animType == AnimationType.Cup) {
				PosRotMapping tempCupMapping = currentAnimSetup[i].Clone();
				tempCupMapping.position += new Vector3(0, 0.2f, 0);

				yield return StartCoroutine(lerpTransform(targetObject, currentAnimSetup[i], tempCupMapping, animSettingsManager.moveDuration / 2, true));
				yield return new WaitForSeconds(0.6f);
				yield return StartCoroutine(lerpTransform(targetObject, tempCupMapping, currentAnimSetup[i], animSettingsManager.moveDuration / 2, true));
			}
		}

		if (animSettingsManager.animType == AnimationType.Key) {
			yield return new WaitForSeconds(0.5f);
			PosRotMapping tempMapping = currentAnimSetup[currentAnimSetup.Count - 1].Clone();
			// Theoretically both ways should work pretty well
			tempMapping.rotation.x += -90;
			//tempMapping.rotation = (Quaternion.Euler(tempMapping.rotation) * Quaternion.Euler(targetObject.transform.forward * 90)).eulerAngles;

			yield return StartCoroutine(lerpTransform(targetObject, currentAnimSetup[currentAnimSetup.Count - 1], tempMapping, 1f, true));
			yield return new WaitForSeconds(0.5f);
			yield return StartCoroutine(lerpTransform(targetObject, tempMapping, currentAnimSetup[currentAnimSetup.Count - 1], 1f, true));
			yield return new WaitForSeconds(0.5f);
			yield return StartCoroutine(lerpTransform(targetObject, currentAnimSetup[currentAnimSetup.Count - 1], currentAnimSetup[0], animSettingsManager.moveDuration, true));
		}
		
		isAnimationRunning = true;
	}

	// Animation control for moving arm and grabbing with hand
	private IEnumerator armStopAnimationLerp() {
		// we set weight to the corresponding part we're moving
		
		yield return StartCoroutine(simpleRigLerp(handRig, animSettingsManager.handMoveDuration, 1, 0));
		
		RigLerp[] rigLerps = {new RigLerp(armRig, 1, 0), new RigLerp(restArmRig, 0, 1)};
		yield return StartCoroutine(multiRigLerp(rigLerps, animSettingsManager.armMoveDuration));
		
		foreach (Renderer item in fakeArmObjects)	{
			item.enabled = false;
		}
		foreach (Renderer item in armObjects)	{
			item.enabled = true;
		}
		if (targetObject.name.Contains("fake")) {
			if (targetObject.TryGetComponent<TargetUtility>(out TargetUtility targetUtility)) {
				foreach (Renderer item in targetUtility.renderers) {
					item.enabled = false;
				}
			}
		} else {
			targetObject.GetComponent<Rigidbody>().useGravity = true;
			targetObject.GetComponent<Collider>().enabled = true;
		}
	}

	private IEnumerator restArmStartAnimation() {
		originalArmRestPosRot = new PosRotMapping(targetsHelperObject.armRestTarget.transform);
		
		if (armRestHelperObject == null) {
			armRestHelperObject = GameObject.Find("ArmRestHelperObject");
		}

		PosRotMapping startMapping = new PosRotMapping(targetsHelperObject.armRestTarget.transform);
		PosRotMapping endMapping = new PosRotMapping(armRestHelperObject.transform);
		endMapping.position += armRestOffset;

		yield return StartCoroutine(lerpTransform(targetsHelperObject.armRestTarget, startMapping, endMapping, animSettingsManager.armMoveDuration));
	}
	
	private IEnumerator restArmStopAnimation() {
		PosRotMapping startMapping = new PosRotMapping(targetsHelperObject.armRestTarget.transform);

		yield return StartCoroutine(lerpTransform(targetsHelperObject.armRestTarget, startMapping, originalArmRestPosRot, animSettingsManager.armMoveDuration));
	}

	public void alignArmRestTargetWithTable() {
		if (isArmResting) {
			Vector3 endPos = armRestHelperObject.transform.position + armRestOffset;

			targetsHelperObject.armRestTarget.transform.position = endPos;
			targetsHelperObject.armRestTarget.transform.rotation = armRestHelperObject.transform.rotation;
		}
	}

	// Linear interpolation for weights, we slowly move arm towards our goal
	// https://gamedevbeginner.com/the-right-way-to-lerp-in-unity-with-examples/
	// currently not used, kept for refference
	private IEnumerator simpleRigLerp(Rig rig, float lerpDuration, float startLerpValue, float endLerpValue) {
		float lerpTimeElapsed = 0f;

		while (lerpTimeElapsed < lerpDuration) {
			// despite the fact lerp should be linear, we use calculation to manipulate it, the movement then looks more natural
			float t = lerpTimeElapsed / lerpDuration;
			t = t * t * t * (t * (6f* t - 15f) + 10f); // https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
			rig.weight = Mathf.Lerp(startLerpValue, endLerpValue, t);
			lerpTimeElapsed += Time.deltaTime;
			yield return null;
		}  
		// lerp never reaches endValue, that is why we have to set it manually
		rig.weight = endLerpValue;
	}

	private IEnumerator reverseDualRigLerp(Rig rigToStart, Rig rigToStop, float lerpDuration, float startLerpValue, float endLerpValue) {
		float lerpTimeElapsed = 0f;

		while (lerpTimeElapsed < lerpDuration) {
			// despite the fact lerp should be linear, we use calculation to manipulate it, the movement then looks more natural
			float t = lerpTimeElapsed / lerpDuration;
			t = t * t * t * (t * (6f* t - 15f) + 10f); // https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
			float lerpValue = Mathf.Lerp(startLerpValue, endLerpValue, t);
			rigToStart.weight = lerpValue;
			rigToStop.weight = Mathf.Abs(endLerpValue - lerpValue);

			lerpTimeElapsed += Time.deltaTime;
			yield return null;
		}  
		// lerp never reaches endValue, that is why we have to set it manually
		rigToStart.weight = endLerpValue;
		rigToStop.weight = startLerpValue;
	}

	private IEnumerator multiRigLerp(RigLerp[] rigLerps, float lerpDuration) {
		float lerpTimeElapsed = 0f;

		while (lerpTimeElapsed < lerpDuration) {
			// despite the fact lerp should be linear, we use calculation to manipulate it, the movement then looks more natural
			float t = lerpTimeElapsed / lerpDuration;
			t = t * t * t * (t * (6f* t - 15f) + 10f); // https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
			foreach (RigLerp rigLerp in rigLerps) {
				float lerpValue = Mathf.Lerp(rigLerp.startValue, rigLerp.endValue, t);
				rigLerp.rig.weight = lerpValue;
			}

			lerpTimeElapsed += Time.deltaTime;
			yield return null;
		}  
		// lerp never reaches endValue, that is why we have to set it manually
		foreach (RigLerp rigLerp in rigLerps) {
			rigLerp.rig.weight = rigLerp.endValue;
		}
	}

	// No need to use currently
	/*private IEnumerator lerpAllTargets(GameObject target, Vector3 startPosition, Vector3 targetPosition, float duration) {
		float time = 0;
		while (time < duration) {
			target.transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
			targetsHelperObject.alignTargetTransforms();
			time += Time.deltaTime;
			yield return null;
		}
		// lerp never reaches endValue, that is why we have to set it manually
		target.transform.position = targetPosition;
		targetsHelperObject.alignTargetTransforms();
	}*/

	private IEnumerator lerpTransform(GameObject startTarget, PosRotMapping startMapping, PosRotMapping endMapping, float duration, bool alignTransforms = false) {
		float time = 0;
		while (time < duration) {
			float t = time / duration;
			t = t * t * t * (t * (6f* t - 15f) + 10f); // https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
			startTarget.transform.position = Vector3.Lerp(startMapping.position, endMapping.position, t);
			startTarget.transform.rotation = Quaternion.Lerp(Quaternion.Euler(startMapping.rotation), Quaternion.Euler(endMapping.rotation), t);
			if (alignTransforms) {
				targetsHelperObject.alignTargetTransforms();
			}
			time += Time.deltaTime;
			yield return null;
		}
		// lerp never reaches endValue, that is why we have to set it manually
		startTarget.transform.position = endMapping.position;
		startTarget.transform.rotation = Quaternion.Euler(endMapping.rotation);
		if (alignTransforms) {
			targetsHelperObject.alignTargetTransforms();
		}
	}

	private IEnumerator alignTransformWrapper(float duration) {
		float time = 0;
		while (time < duration) {
			targetsHelperObject.alignTargetTransforms();
			time += Time.deltaTime;
			yield return null;
		}
		targetsHelperObject.alignTargetTransforms();
	}

	public void startAnimation(bool isFakeAnimation) {
		if(animSettingsManager.animType == AnimationType.Off) {
			Debug.LogError("No animation type specified");
			return;
		}
		if(animState == Enums.AnimationState.Playing) {
			Debug.LogError("There is an animation running already");
			return;
		}

		string targetObjectName = animSettingsManager.animType.ToString();

		// setup targetObject 
		if(isFakeAnimation) {
			// if it's a fake animation, we also have to set the correct position of our fake object
			// GameObject originalTargetObject = GameObject.Find(targetObjectName);
			targetObject = GameObject.Find(targetObjectName + "_fake");
			if (targetObject.TryGetComponent<TargetUtility>(out TargetUtility tu)) {
				foreach (Renderer item in tu.renderers) {
					item.enabled = true;
				}
			}

			foreach (Renderer item in armObjects) {
				item.enabled = false;
			}
			foreach (Renderer item in fakeArmObjects) {
				item.enabled = true;
			}
		}else {
			targetObject = GameObject.Find(targetObjectName);
			if (targetObject == null) {
				targetObject = GameObject.Find(targetObjectName + "(Clone)");
			}
			targetObject.GetComponent<Rigidbody>().useGravity = false;
			targetObject.GetComponent<Collider>().enabled = false;
		}

		float armLength = Mathf.Max(armRangeMesh.transform.lossyScale.x, armRangeMesh.transform.lossyScale.x, armRangeMesh.transform.lossyScale.x) * armRangeMesh.sharedMesh.bounds.extents.x;
		float targetDistance = Vector3.Distance(targetObject.transform.position, armRangeMesh.transform.position);
		if ((targetDistance - armRangeSlack) > armLength) {
			Debug.LogError("Arm cannot grab object, too far away: " + targetDistance + "m > " + armLength + "m");
			return;
		}

		TargetMappingGroup currentMapping = animationMapping.getTargetMappingByType(animSettingsManager.animType);
		List<PosRotMapping> currentAnimationSetup = animSettingsManager.getCurrentAnimationSetup();

		if (currentAnimationSetup.Count < 1) {
			Debug.LogError("Start or End animation position not set");
			return;
		}
		// Debug.Log(currentAnimationSetup[0].position + " _ " + (currentAnimationSetup.Count - 1));

		targetObject.transform.position = currentAnimationSetup[0].position;
		targetObject.transform.rotation = Quaternion.Euler(currentAnimationSetup[0].rotation);

		if (targetObject.TryGetComponent<TargetUtility>(out TargetUtility targetUtility)) {
			// helper target objects, children of our target object
			targetsHelperObject.armTargetTemplate = targetUtility.ArmIK_target_helper;
			targetsHelperObject.thumbTargetTemplate = targetUtility.ThumbIK_target_helper;
			targetsHelperObject.indexTargetTemplate = targetUtility.IndexChainIK_target_helper;
			targetsHelperObject.middleTargetTemplate = targetUtility.MiddleChainIK_target_helper;
			targetsHelperObject.ringTargetTemplate = targetUtility.RingChainIK_target_helper;
			targetsHelperObject.pinkyTargetTemplate = targetUtility.PinkyChainIK_target_helper;
		} else {
			Debug.LogError("Failed to retrieve target helper objects from Target - " + targetObjectName);
			return;
		}

		try {
			// Setting position + rotation
			// TODO calculate for any target position
			/* PosRotMapping originalTransform = new PosRotMapping(targetObject.transform);

			if (targetObject.TryGetComponent<NetworkTransform>(out NetworkTransform networkTransform)) {
				networkTransform.enabled = false;
			}

			Vector3 newRotation = targetObject.transform.localRotation.eulerAngles;
			ArmAnimationController.PrintVector3(newRotation);

			if (!targetUtility.ignoreZeroRotX) {
				newRotation.x = 0;
			} else {
				newRotation.x = Mathf.Clamp(newRotation.x, targetUtility.zeroTransformClampMin.x, targetUtility.zeroTransformClampMax.x);
			}
			if (!targetUtility.ignoreZeroRotY) {
				newRotation.y = 0;
			} else {
				newRotation.y = Mathf.Clamp(newRotation.y, targetUtility.zeroTransformClampMin.y, targetUtility.zeroTransformClampMax.y);
			}
			if (!targetUtility.ignoreZeroRotZ) {
				newRotation.z = 0;
			} else {
				newRotation.z = Mathf.Clamp(newRotation.z, targetUtility.zeroTransformClampMin.z, targetUtility.zeroTransformClampMax.z);
			}

			targetObject.transform.position = targetUtility.zeroTransformPosition;
			targetObject.transform.localRotation = Quaternion.Euler(newRotation); */

			targetsHelperObject.setAllTargetMappings(animationMapping.getTargetMappingByType(animSettingsManager.animType), targetObject);

			/* targetObject.transform.position = originalTransform.position;
			targetObject.transform.rotation = Quaternion.Euler(originalTransform.rotation);

			if (networkTransform != null) {
				networkTransform.enabled = true;
			} */

			targetsHelperObject.alignTargetTransforms();

			#if UNITY_EDITOR
				// printOffsets();
			#endif
		} catch(System.Exception ex) {
			Debug.LogError(ex);
			return;
		}

		animState = Enums.AnimationState.Playing;

		// https://gamedevbeginner.com/coroutines-in-unity-when-and-how-to-use-them/
		StartCoroutine(armStartAnimationLerp(isFakeAnimation));
	}

	public void stopAnimation() {
		if(animState == Enums.AnimationState.Stopped) {
			return;
		}

		// We don't have to set the positions of targets here, because we're simply releasing the hand grip + moving arm to relaxed position

		StartCoroutine(armStopAnimationLerp());
		animState = Enums.AnimationState.Stopped;
	}

	public void setArmRestPosition() {
		isArmResting = !isArmResting;
		

		if (isArmResting) {
			StartCoroutine(restArmStartAnimation());
		} else {
			StartCoroutine(restArmStopAnimation());
		}
	}

	private GameObject findChildByName(string name) {
		Transform[] children = GetComponentsInChildren<Transform>();
			foreach(Transform child in children) {
				if(child.gameObject.name.Equals(name)) {
					return child.gameObject;
			}
		}

		return null;
	}

	public void printOffsets() {
		if (targetObject == null) {
			return;
		}
		Debug.Log("_____________________________________");
		ArmAnimationController.PrintVector3(targetObject.transform.position - targetsHelperObject.armTargetTemplate.transform.position);
		Debug.Log(targetsHelperObject.armTargetTemplate.transform.rotation.eulerAngles);
		ArmAnimationController.PrintVector3(targetObject.transform.position - targetsHelperObject.thumbTargetTemplate.transform.position);
		Debug.Log(targetsHelperObject.thumbTargetTemplate.transform.rotation.eulerAngles);
		ArmAnimationController.PrintVector3(targetObject.transform.position - targetsHelperObject.indexTargetTemplate.transform.position);
		Debug.Log(targetsHelperObject.indexTargetTemplate.transform.rotation.eulerAngles);
		ArmAnimationController.PrintVector3(targetObject.transform.position - targetsHelperObject.middleTargetTemplate.transform.position);
		Debug.Log(targetsHelperObject.middleTargetTemplate.transform.rotation.eulerAngles);
		ArmAnimationController.PrintVector3(targetObject.transform.position - targetsHelperObject.ringTargetTemplate.transform.position);
		Debug.Log(targetsHelperObject.ringTargetTemplate.transform.rotation.eulerAngles);
		ArmAnimationController.PrintVector3(targetObject.transform.position - targetsHelperObject.pinkyTargetTemplate.transform.position);
		Debug.Log(targetsHelperObject.pinkyTargetTemplate.transform.rotation.eulerAngles);
	}

	public void saveOffsets() {
		if (targetObject == null) {
			return;
		}
		TargetMappingGroup currentMapping = animationMapping.getTargetMappingByType(animSettingsManager.animType);
		
		currentMapping.armMapping = new PosRotMapping(targetObject.transform.position - targetsHelperObject.armTargetTemplate.transform.position, targetsHelperObject.armTargetTemplate.transform.rotation.eulerAngles);
		currentMapping.thumbMapping = new PosRotMapping(targetObject.transform.position - targetsHelperObject.thumbTargetTemplate.transform.position, targetsHelperObject.thumbTargetTemplate.transform.rotation.eulerAngles);
		currentMapping.indexMapping = new PosRotMapping(targetObject.transform.position - targetsHelperObject.indexTargetTemplate.transform.position, targetsHelperObject.indexTargetTemplate.transform.rotation.eulerAngles);
		currentMapping.middleMapping = new PosRotMapping(targetObject.transform.position - targetsHelperObject.middleTargetTemplate.transform.position, targetsHelperObject.middleTargetTemplate.transform.rotation.eulerAngles);
		currentMapping.ringMapping = new PosRotMapping(targetObject.transform.position - targetsHelperObject.ringTargetTemplate.transform.position, targetsHelperObject.ringTargetTemplate.transform.rotation.eulerAngles);
		currentMapping.pinkyMapping = new PosRotMapping(targetObject.transform.position - targetsHelperObject.pinkyTargetTemplate.transform.position, targetsHelperObject.pinkyTargetTemplate.transform.rotation.eulerAngles);
	}
}
