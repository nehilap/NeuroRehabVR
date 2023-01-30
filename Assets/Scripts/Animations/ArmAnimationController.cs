using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.Animations.Rigging;

using Enums;
using Mappings;

public class ArmAnimationController : MonoBehaviour {
	private Enums.AnimationState animState = Enums.AnimationState.Stopped;
	private AnimationPart animPart;
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

	[Header("Objects setup from code")]
	[SerializeField] private GameObject targetObject;

	[SerializeField] private AnimationSettingsManager animSettingsManager;
	[SerializeField] private GameObject armRestHelperObject;
	[SerializeField] private bool isArmResting = false;
	[SerializeField] private PosRotMapping originalArmRestPosRot;
	

	private AnimationMapping animationMapping;

	void Start() {
		animationMapping = new AnimationMapping();

		animSettingsManager = GameObject.Find("AnimationSettingsObject")?.GetComponent<AnimationSettingsManager>();

		armRestHelperObject = GameObject.Find("ArmRestHelperObject");

		// BLOCK animation setup - relative values
		// TODO
		animationMapping.blockMapping.armMapping = new PosRotMapping(new Vector3(-1.104002f, 0.07f, -1.648f), new Vector3(0f, 336.925079f, 270f)); // armTarget
		animationMapping.blockMapping.thumbMapping = new PosRotMapping(new Vector3(-0.300006866f, 0.287f, 1.146f), new Vector3(1.90925431f, 8.86787796f, 17.1680984f)); // thumbTarget
		animationMapping.blockMapping.indexMapping = new PosRotMapping(new Vector3(0.200004578f, 0.326000452f, 0.654f), new Vector3(0f, 0f, 0f)); // indexTarget
		animationMapping.blockMapping.middleMapping = new PosRotMapping(new Vector3(0.186004639f, 0.104000568f, 0.68f), new Vector3(0f, 0f, 0f)); // middleTarget
		animationMapping.blockMapping.ringMapping = new PosRotMapping(new Vector3(0.267972946f, -0.161999464f, 0.664f), new Vector3(0f, 0f, 0f)); // ringTarget
		animationMapping.blockMapping.pinkyMapping = new PosRotMapping(new Vector3(0.541992188f, -0.401f, 0.618f), new Vector3(0f, 0f, 0f)); // pinkyTarget
	}

	private void LateUpdate() {
		alignArmRestTargetWithTable();
	}

	// https://gamedevbeginner.com/coroutines-in-unity-when-and-how-to-use-them/
	private IEnumerator armStartAnimationLerp(bool isFakeAnimation) {
		// Animation control for moving arm and grabbing with hand
		// we set weight to the corresponding part we're moving
		yield return StartCoroutine(dualRigLerp(armRig, restArmRig, animSettingsManager.armMoveDuration, 0, 1));

		animPart = AnimationPart.Hand;
		
		yield return StartCoroutine(simpleRigLerp(handRig, animSettingsManager.handMoveDuration, 0, 1));
		
		animPart = AnimationPart.Moving;

		// Debug.Log(CharacterManager.localClient.GetInstanceID() + ",,," + CharacterManager.activePatient.GetInstanceID());		
		if (!isFakeAnimation && !(CharacterManager.localClient.GetInstanceID() == CharacterManager.activePatient.GetInstanceID())) {
			Debug.Log("Not original patient, aligning transform");
			yield return StartCoroutine(alignTransformWrapper(animSettingsManager.moveDuration));
		} else {
			Debug.Log("Original patient or FakeArm, moving object");
			// if Fake Arm, we don't need to gain authority, because it is not using networked object
			if (!isFakeAnimation && (CharacterManager.localClient.GetInstanceID() == CharacterManager.activePatient.GetInstanceID())) {
				// we ask server to grant us authority over target object
				NetworkCharacterManager.localNetworkClient.CmdSetItemAuthority(targetObject.GetComponent<NetworkIdentity>(), CharacterManager.localClient.GetComponent<NetworkIdentity>());
			}

			SyncList<PosRotMapping> currentAnimSetup = animSettingsManager.getCurrentAnimationSetup();
			for (int i = 1; i < currentAnimSetup.Count; i++)	{
				Vector3 startPos = currentAnimSetup[i-1].position;
				Vector3 endPos = currentAnimSetup[i].position;
				yield return StartCoroutine(lerpAllTargets(targetObject, startPos, endPos, animSettingsManager.moveDuration));
			}
		}

		// do nothing else
		stopAnimation();
	}

	// Animation control for moving arm and grabbing with hand
	private IEnumerator armStopAnimationLerp() {
		// we set weight to the corresponding part we're moving
		animPart = AnimationPart.Hand;

		yield return StartCoroutine(simpleRigLerp(handRig, animSettingsManager.armMoveDuration, 1, 0));

		animPart = AnimationPart.Arm;
		
		yield return StartCoroutine(dualRigLerp(armRig, restArmRig, animSettingsManager.handMoveDuration, 1, 0));
		
		foreach (Renderer item in fakeArmObjects)	{
			item.enabled = false;
		}
		foreach (Renderer item in armObjects)	{
			item.enabled = true;
		}
		if (targetObject.name.Contains("fake")) {
			targetObject.GetComponent<Renderer>().enabled = false;
		} else {
			targetObject.GetComponent<Rigidbody>().useGravity = true;
		}
	}

	private IEnumerator restArmStartAnimation() {
		originalArmRestPosRot = new PosRotMapping(targetsHelperObject.armRestTarget.transform);

		Vector3 startPos = targetsHelperObject.armRestTarget.transform.position;
		Vector3 endPos = armRestHelperObject.transform.position + armRestOffset;

		PosRotMapping endMapping = new PosRotMapping(endPos, armRestHelperObject.transform.rotation.eulerAngles);

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

	private IEnumerator dualRigLerp(Rig rigToStart, Rig rigToStop, float lerpDuration, float startLerpValue, float endLerpValue) {
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
			targetObject.GetComponent<Renderer>().enabled = true;

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
		SyncList<PosRotMapping> currentAnimationSetup = animSettingsManager.getCurrentAnimationSetup();

		if (currentAnimationSetup.Count <= 1) {
			Debug.LogError("Start or End animation position not set");
			return;
		}
		// Debug.Log(currentAnimationSetup[0].position + " _ " + (currentAnimationSetup.Count - 1));

		targetObject.transform.position = currentAnimationSetup[0].position;
		targetObject.transform.rotation = Quaternion.Euler(currentAnimationSetup[0].rotation);
		
		// helper target objects, children of our target object
		targetsHelperObject.armTargetTemplate = targetObject.transform.Find("ArmIK_target_helper").gameObject;
		targetsHelperObject.thumbTargetTemplate = targetObject.transform.Find("ThumbIK_target_helper").gameObject;
		targetsHelperObject.indexTargetTemplate = targetObject.transform.Find("IndexChainIK_target_helper").gameObject;
		targetsHelperObject.middleTargetTemplate = targetObject.transform.Find("MiddleChainIK_target_helper").gameObject;
		targetsHelperObject.ringTargetTemplate = targetObject.transform.Find("RingChainIK_target_helper").gameObject;
		targetsHelperObject.pinkyTargetTemplate = targetObject.transform.Find("PinkyChainIK_target_helper").gameObject;

		try {
			// Setting position + rotation
			// TODO calculate for any target position
			targetsHelperObject.setAllTargetMappings(animationMapping.getTargetMappingByType(animSettingsManager.animType));
			targetsHelperObject.alignTargetTransforms();
		} catch(System.Exception ex) {
			Debug.LogError(ex);
			return;
		}

		animState = Enums.AnimationState.Playing;
		animPart = AnimationPart.Arm;

		// https://gamedevbeginner.com/coroutines-in-unity-when-and-how-to-use-them/
		StartCoroutine(armStartAnimationLerp(isFakeAnimation));
	}

	public void stopAnimation() {
		if(animState == Enums.AnimationState.Stopped) {
			return;
		}

		// We don't have to set the positions of targets here, because we're simply releasing the hand grip + moving arm to relaxed position
		animPart = AnimationPart.Hand;

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

	public static void PrintVector3(Vector3 message, int type = 1) {
		if (type == 1)
			Debug.Log("X: " + message.x + "  Y: " + message.y + "  Z:" + message.z);
		if (type == 2)
			Debug.LogWarning("X: " + message.x + "  Y: " + message.y + "  Z:" + message.z);
		if (type == 3)
			Debug.LogError("X: " + message.x + "  Y: " + message.y + "  Z:" + message.z);
	}
}
