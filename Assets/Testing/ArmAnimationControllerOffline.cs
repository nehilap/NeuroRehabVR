using System.Collections;
using Mirror;
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

	void Start() {
		animSettingsManager = GameObject.Find("AnimSettingsManagerOffline")?.GetComponent<AnimationSettingsManagerOffline>();

		armRestHelperObject = GameObject.Find("ArmRestHelperObject");

		avatarController = gameObject.GetComponent<AvatarController>();

		// BLOCK animation setup - relative values
		// TODO

		/* 	
		animationMapping.blockMapping.armMapping = new PosRotMapping(new Vector3(-1.104002f, 0.07f, -1.648f), new Vector3(0f, 336.925079f, 270f)); // armTarget
		animationMapping.blockMapping.thumbMapping = new PosRotMapping(new Vector3(-0.300006866f, 0.287f, 1.146f), new Vector3(1.90925431f, 8.86787796f, 17.1680984f)); // thumbTarget
		animationMapping.blockMapping.indexMapping = new PosRotMapping(new Vector3(0.200004578f, 0.326000452f, 0.654f), new Vector3(0f, 0f, 0f)); // indexTarget
		animationMapping.blockMapping.middleMapping = new PosRotMapping(new Vector3(0.186004639f, 0.104000568f, 0.68f), new Vector3(0f, 0f, 0f)); // middleTarget
		animationMapping.blockMapping.ringMapping = new PosRotMapping(new Vector3(0.267972946f, -0.161999464f, 0.664f), new Vector3(0f, 0f, 0f)); // ringTarget
		animationMapping.blockMapping.pinkyMapping = new PosRotMapping(new Vector3(0.541992188f, -0.401f, 0.618f), new Vector3(0f, 0f, 0f)); // pinkyTarget
		 */
		/*
		X: -0,08240002  Y: -0,006999969  Z:0,0552001 (0.00, 246.93, 270.00)
		X: 0,05730001  Y: -0,02869999  Z:0,01500034 (1.91, 278.87, 17.17)
		X: 0,03270001  Y: -0,03260005  Z:-0,01000023 (0.00, 270.00, 0.00)
		X: 0,03400001  Y: -0,01040006  Z:-0,009300232 (0.00, 270.00, 0.00)
		X: 0,03320001  Y: 0,01619995  Z:-0,01339865 (0.00, 270.00, 0.00)
		X: 0,03090001  Y: 0,04009998  Z:-0,02709961 (0.00, 270.00, 0.00)
		*/

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
		
		// Debug.Log(CharacterManager.localClient.GetInstanceID() + ",,," + CharacterManager.activePatient.GetInstanceID());		
		if (!isFakeAnimation) {
			yield return StartCoroutine(alignTransformWrapper(animSettingsManager.moveDuration));
		} else {
			List<PosRotMapping> currentAnimSetup = animSettingsManager.getCurrentAnimationSetup();
			for (int i = 1; i < currentAnimSetup.Count; i++)	{
				Vector3 startPos = currentAnimSetup[i-1].position;
				Vector3 endPos = currentAnimSetup[i].position;
				yield return StartCoroutine(lerpAllTargets(targetObject, startPos, endPos, animSettingsManager.moveDuration));
			}
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
		}
	}

	private IEnumerator restArmStartAnimation() {
		originalArmRestPosRot = new PosRotMapping(targetsHelperObject.armRestTarget.transform);
		
		if (armRestHelperObject == null) {
			armRestHelperObject = GameObject.Find("ArmRestHelperObject");
		}

		Vector3 endPos = armRestHelperObject.transform.position + armRestOffset;
		Vector3 endRot = armRestHelperObject.transform.rotation.eulerAngles;

		PosRotMapping endMapping = new PosRotMapping(endPos, endRot);
		

		yield return StartCoroutine(lerpTransform(targetsHelperObject.armRestTarget, endMapping, animSettingsManager.armMoveDuration));
	}
	
	private IEnumerator restArmStopAnimation() {
		Vector3 startPos = targetsHelperObject.armRestTarget.transform.position;

		yield return StartCoroutine(lerpTransform(targetsHelperObject.armRestTarget, originalArmRestPosRot, animSettingsManager.armMoveDuration));
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

	private IEnumerator lerpAllTargets(GameObject target, Vector3 startPosition, Vector3 targetPosition, float duration) {
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
	}

	private IEnumerator lerpTransform(GameObject startTarget, PosRotMapping endMapping, float duration) {
		PosRotMapping startMapping = new PosRotMapping(startTarget.transform);

		float time = 0;
		while (time < duration) {
			startTarget.transform.position = Vector3.Lerp(startMapping.position, endMapping.position, time / duration);
			startTarget.transform.rotation = Quaternion.Lerp(Quaternion.Euler(startMapping.rotation), Quaternion.Euler(endMapping.rotation), time / duration);
			time += Time.deltaTime;
			yield return null;
		}
		// lerp never reaches endValue, that is why we have to set it manually
		startTarget.transform.position = endMapping.position;
		startTarget.transform.rotation = Quaternion.Euler(endMapping.rotation);
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
			PosRotMapping currentTransform = new PosRotMapping(targetObject.transform);

			targetObject.transform.position = targetUtility.zeroTransform.position;
			targetObject.transform.rotation = Quaternion.Euler(targetUtility.zeroTransform.rotation);

			targetsHelperObject.setAllTargetMappings(animationMapping.getTargetMappingByType(animSettingsManager.animType), targetObject);

			targetObject.transform.position = currentTransform.position;
			targetObject.transform.rotation = Quaternion.Euler(currentTransform.rotation);

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

		animState = Enums.AnimationState.Stopped;
		StartCoroutine(armStopAnimationLerp());
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
