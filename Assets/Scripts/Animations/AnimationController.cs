using UnityEngine;
using Enums;
using Mappings;
using UnityEngine.Animations.Rigging;
using Mirror;
using System.Collections;

public class AnimationController : MonoBehaviour
{
	// no need to Serialize public fields, those are serialized automatically
	public GameObject targetObject;
	public bool isFakeArm = false;

	private Enums.AnimationState animState = Enums.AnimationState.Stopped;
	private AnimationPart animPart;

	private Rig armRig;
	private Rig handRig;

	private Renderer figureRenderer;

	[SerializeField]
	private AnimationSettingsManager animSettingsManager;

	private TargetsHelper targetsHelperObject;

	private AnimationMapping animationMapping;

	void Start() {
		if(isFakeArm) {
			figureRenderer = GetComponentInChildren<Renderer>();
			figureRenderer.enabled = false;
		}

		animationMapping = new AnimationMapping();

		animSettingsManager = GameObject.Find("AnimationSettingsObject")?.GetComponent<AnimationSettingsManager>();

		armRig = transform.Find("ArmRig").GetComponent<Rig>();
		handRig = transform.Find("HandRig").GetComponent<Rig>();

		// target objects, these objects will be moved to correspond to where bones should move
		Transform[] children = GetComponentsInChildren<Transform>();
		foreach(Transform child in children) {
			if(child.gameObject.name.Equals("ArmIK_target")) {
				targetsHelperObject.armTarget = child.gameObject;
			} else if(child.gameObject.name.Equals("ThumbIK_target")) {
				targetsHelperObject.thumbTarget = child.gameObject;
			} else if(child.gameObject.name.Equals("IndexChainIK_target")) {
				targetsHelperObject.indexTarget = child.gameObject;
			} else if(child.gameObject.name.Equals("MiddleChainIK_target")) {
				targetsHelperObject.middleTarget = child.gameObject;
			} else if(child.gameObject.name.Equals("RingChainIK_target")) {
				targetsHelperObject.ringTarget = child.gameObject;
			} else if(child.gameObject.name.Equals("PinkyChainIK_target")) {
				targetsHelperObject.pinkyTarget = child.gameObject;
			}
		}

		// BLOCK animation setup - relative values
		// TODO
		animationMapping.blockMapping.armMapping = new PosRotMapping(new Vector3(-1.104002f, 0.07f, -1.648f), new Vector3(0f, 336.925079f, 270f)); // armTarget
		animationMapping.blockMapping.thumbMapping = new PosRotMapping(new Vector3(-0.300006866f, 0.287f, 1.146f), new Vector3(1.90925431f, 8.86787796f, 17.1680984f)); // thumbTarget
		animationMapping.blockMapping.indexMapping = new PosRotMapping(new Vector3(0.200004578f, 0.326000452f, 0.654f), new Vector3(0f, 0f, 0f)); // indexTarget
		animationMapping.blockMapping.middleMapping = new PosRotMapping(new Vector3(0.186004639f, 0.104000568f, 0.68f), new Vector3(0f, 0f, 0f)); // middleTarget
		animationMapping.blockMapping.ringMapping = new PosRotMapping(new Vector3(0.267972946f, -0.161999464f, 0.664f), new Vector3(0f, 0f, 0f)); // ringTarget
		animationMapping.blockMapping.pinkyMapping = new PosRotMapping(new Vector3(0.541992188f, -0.401f, 0.618f), new Vector3(0f, 0f, 0f)); // pinkyTarget
	}

	public static void PrintVector3(Vector3 message, int type = 1) {
		if (type == 1)
			Debug.Log("X: " + message.x + "  Y: " + message.y + "  Z:" + message.z);
		if (type == 2)
			Debug.LogWarning("X: " + message.x + "  Y: " + message.y + "  Z:" + message.z);
		if (type == 3)
			Debug.LogError("X: " + message.x + "  Y: " + message.y + "  Z:" + message.z);
	}

	// https://gamedevbeginner.com/coroutines-in-unity-when-and-how-to-use-them/
	IEnumerator armStartAnimationLerp() {
		// Animation control for moving arm and grabbing with hand
		// we set weight to the corresponding part we're moving
		yield return StartCoroutine(simpleRigLerp(armRig, animSettingsManager.armMoveDuration, 0, 1));

		animPart = AnimationPart.Hand;
		
		yield return StartCoroutine(simpleRigLerp(handRig, animSettingsManager.handMoveDuration, 0, 1));
		
		animPart = AnimationPart.Moving;

		// Debug.Log(CharacterManager.localClient.GetInstanceID() + ",,," + CharacterManager.activePatient.GetInstanceID());		
		if (!isFakeArm && !(CharacterManager.localClient.GetInstanceID() == CharacterManager.activePatient.GetInstanceID())) {
			Debug.Log("Not original patient, aligning transform");
			yield return StartCoroutine(alignTransformWrapper(animSettingsManager.moveDuration));
		} else {
			Debug.Log("Original patient or FakeArm, moving object");
			// if Fake Arm, we don't need to gain authority, because it is not using networked object
			if (!isFakeArm && (CharacterManager.localClient.GetInstanceID() == CharacterManager.activePatient.GetInstanceID())) {
				// we ask server to grant us authority over target object
				CharacterManager.localClient.CmdSetItemAuthority(targetObject.GetComponent<NetworkIdentity>(), CharacterManager.localClient.GetComponent<NetworkIdentity>());
			}

			SyncList<PosRotMapping> currentAnimSetup = animSettingsManager.getCurrentAnimationSetup();
			for (int i = 1; i < currentAnimSetup.Count; i++)	{
				Vector3 startPos = currentAnimSetup[i-1].position;
				Vector3 endPos = currentAnimSetup[i].position;
				yield return StartCoroutine(lerpVector3(targetObject, startPos, endPos, animSettingsManager.moveDuration));
			}
		}

		// do nothing else
		stopAnimation();
	}

	IEnumerator armStopAnimationLerp() {
		
		// Animation control for moving arm and grabbing with hand
		// we set weight to the corresponding part we're moving
		animPart = AnimationPart.Hand;

		yield return StartCoroutine(simpleRigLerp(handRig, animSettingsManager.armMoveDuration, 1, 0));

		animPart = AnimationPart.Arm;
		
		yield return StartCoroutine(simpleRigLerp(armRig, animSettingsManager.handMoveDuration, 1, 0));

		if(isFakeArm) {
			targetObject.GetComponent<Renderer>().enabled = false;
			figureRenderer.enabled = false;
		} else {
			targetObject.GetComponent<Rigidbody>().useGravity = true;
		}
	}

	// Linear interpolation for weights, we slowly move arm towards our goal
	// https://gamedevbeginner.com/the-right-way-to-lerp-in-unity-with-examples/
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

	IEnumerator lerpVector3(GameObject target, Vector3 startPosition, Vector3 targetPosition, float duration) {
        float time = 0;
        while (time < duration) {
            target.transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
			targetsHelperObject.alignTargetTransforms();
            time += Time.deltaTime;
            yield return null;
        }
        target.transform.position = targetPosition;
		targetsHelperObject.alignTargetTransforms();
    }

	IEnumerator alignTransformWrapper(float duration) {
        float time = 0;
        while (time < duration) {
            targetsHelperObject.alignTargetTransforms();
			time += Time.deltaTime;
            yield return null;
        }
        targetsHelperObject.alignTargetTransforms();
    }

	public void startAnimation() {
		if(animSettingsManager.animType == AnimationType.Off) {
			Debug.LogError("No animation type specified");
			return;
		}
		if(animState == Enums.AnimationState.Playing) {
			Debug.LogError("There is an animation running already");
			return;
		}

		string targetObjectName = animSettingsManager.animType.ToString(); // TODO animationSettingsManager.animType.toString();

		// setup targetObject 
		if(isFakeArm) {
			// if it's a fake arm, we also have to set the correct position of our fake object
			// GameObject originalTargetObject = GameObject.Find(targetObjectName);
			targetObject = GameObject.Find(targetObjectName + "_fake");

			targetObject.GetComponent<Renderer>().enabled = true;
			figureRenderer.enabled = true;
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
		Debug.Log(currentAnimationSetup[0].position + " _ " + (currentAnimationSetup.Count - 1));

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
		}catch(System.Exception ex) {
			Debug.LogError(ex);
			return;
		}

		animState = Enums.AnimationState.Playing;
		animPart = AnimationPart.Arm;

		// https://gamedevbeginner.com/coroutines-in-unity-when-and-how-to-use-them/
		StartCoroutine(armStartAnimationLerp());
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

	private GameObject findChildByName(string name) {
		Transform[] children = GetComponentsInChildren<Transform>();
			foreach(Transform child in children) {
				if(child.gameObject.name.Equals(name)) {
					return child.gameObject;
			}
		}

		return null;
	}
}
