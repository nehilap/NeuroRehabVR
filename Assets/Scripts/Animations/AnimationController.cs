using UnityEngine;
using TMPro;
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

	private AnimationMapping animationMappings;

	private Renderer figureRenderer;

	public AnimationSettingsManager animationSettingsManager;

	void Start() {
		if(isFakeArm) {
			figureRenderer = GetComponentInChildren<Renderer>();
			figureRenderer.enabled = false;
		}

		animationMappings = new AnimationMapping();

		animationSettingsManager = GameObject.Find("AnimationSettingsObject")?.GetComponent<AnimationSettingsManager>();

		armRig = transform.Find("ArmRig").GetComponent<Rig>();
		handRig = transform.Find("HandRig").GetComponent<Rig>();

		// target objects, these objects will be moved to correspond to where bones should move
		Transform[] children = GetComponentsInChildren<Transform>();
		foreach(Transform child in children) {
			if(child.gameObject.name.Equals("ArmIK_target")) {
				animationMappings.armTarget = child.gameObject;
			} else if(child.gameObject.name.Equals("ThumbIK_target")) {
				animationMappings.thumbTarget = child.gameObject;
			} else if(child.gameObject.name.Equals("IndexChainIK_target")) {
				animationMappings.indexTarget = child.gameObject;
			} else if(child.gameObject.name.Equals("MiddleChainIK_target")) {
				animationMappings.middleTarget = child.gameObject;
			} else if(child.gameObject.name.Equals("RingChainIK_target")) {
				animationMappings.ringTarget = child.gameObject;
			} else if(child.gameObject.name.Equals("PinkyChainIK_target")) {
				animationMappings.pinkyTarget = child.gameObject;
			}
		}

		/*
		animationMappings.armTarget = transform.Find("ArmIK_target")?.gameObject;
		animationMappings.thumbTarget = transform.Find("ThumbIK_target")?.gameObject;
		animationMappings.indexTarget = transform.Find("IndexChainIK_target")?.gameObject;
		animationMappings.middleTarget = transform.Find("MiddleChainIK_target")?.gameObject;
		animationMappings.ringTarget = transform.Find("RingChainIK_target")?.gameObject;
		animationMappings.pinkyTarget = transform.Find("PinkyChainIK_target")?.gameObject;
		*/

		// BLOCK animation setup - relative values
		// TODO
		animationMappings.blockMapping.armMapping = new TargetMapping(new Vector3(-1.104002f, 0.07f, -1.648f), new Vector3(0f, 336.925079f, 270f)); // armTarget
		animationMappings.blockMapping.thumbMapping = new TargetMapping(new Vector3(-0.300006866f, 0.287f, 1.146f), new Vector3(1.90925431f, 8.86787796f, 17.1680984f)); // thumbTarget
		animationMappings.blockMapping.indexMapping = new TargetMapping(new Vector3(0.200004578f, 0.326000452f, 0.654f), new Vector3(0f, 0f, 0f)); // indexTarget
		animationMappings.blockMapping.middleMapping = new TargetMapping(new Vector3(0.186004639f, 0.104000568f, 0.68f), new Vector3(0f, 0f, 0f)); // middleTarget
		animationMappings.blockMapping.ringMapping = new TargetMapping(new Vector3(0.267972946f, -0.161999464f, 0.664f), new Vector3(0f, 0f, 0f)); // ringTarget
		animationMappings.blockMapping.pinkyMapping = new TargetMapping(new Vector3(0.541992188f, -0.401f, 0.618f), new Vector3(0f, 0f, 0f)); // pinkyTarget
		setAnimationStartPosition();
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
		yield return StartCoroutine(simpleRigLerp(armRig, animationSettingsManager.armMoveDuration, 0, 1));

		animPart = AnimationPart.Hand;
		
		yield return StartCoroutine(simpleRigLerp(handRig, animationSettingsManager.handMoveDuration, 0, 1));
		
		animPart = AnimationPart.Moving;

		// Debug.Log(CharacterManager.localClient.GetInstanceID() + ",,," + CharacterManager.activePatient.GetInstanceID());		
		if (!isFakeArm && !(CharacterManager.localClient.GetInstanceID() == CharacterManager.activePatient.GetInstanceID())) {
			Debug.Log("Not original patient, aligning transform");
			yield return StartCoroutine(alignTransformWrapper(animationSettingsManager.moveDuration));
		} else {
			Debug.Log("Original patient or FakeArm, moving object");
			// if Fake Arm, we don't need to gain authority, because it is not using networked object
			if (!isFakeArm && (CharacterManager.localClient.GetInstanceID() == CharacterManager.activePatient.GetInstanceID())) {
				// we ask server to grant us authority over target object
				CharacterManager.localClient.CmdSetItemAuthority(targetObject.GetComponent<NetworkIdentity>(), CharacterManager.localClient.GetComponent<NetworkIdentity>());
			}

			TargetMappingGroup currentTargetMappingGroup = animationMappings.getTargetMappingByType(animationSettingsManager.animType);
			for (int i = 0; i < currentTargetMappingGroup.movePositions.Count; i++)	{
				Vector3 startPos = currentTargetMappingGroup.startPositionRotation.position;
				Vector3 endPos = currentTargetMappingGroup.movePositions[i].position;
				yield return StartCoroutine(lerpVector3(targetObject, startPos, endPos, animationSettingsManager.moveDuration));
			}
		}

		// do nothing else
		stopAnimation();
	}

	IEnumerator armStopAnimationLerp() {
		
		// Animation control for moving arm and grabbing with hand
		// we set weight to the corresponding part we're moving
		animPart = AnimationPart.Hand;

		yield return StartCoroutine(simpleRigLerp(handRig, animationSettingsManager.armMoveDuration, 1, 0));

		animPart = AnimationPart.Arm;
		
		yield return StartCoroutine(simpleRigLerp(armRig, animationSettingsManager.handMoveDuration, 1, 0));

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
			animationMappings.alignTargetTransforms();
            time += Time.deltaTime;
            yield return null;
        }
        target.transform.position = targetPosition;
		animationMappings.alignTargetTransforms();
    }

	IEnumerator alignTransformWrapper(float duration) {
        float time = 0;
        while (time < duration) {
            animationMappings.alignTargetTransforms();
			time += Time.deltaTime;
            yield return null;
        }
        animationMappings.alignTargetTransforms();
    }

	public void startAnimation() {
		if(animationSettingsManager.animType == AnimationType.Off) {
			Debug.LogError("No animation type specified");
			return;
		}
		if(animState == Enums.AnimationState.Playing) {
			Debug.LogError("There is an animation running already");
			return;
		}

		string targetObjectName = animationSettingsManager.animType.ToString(); // TODO animationSettingsManager.animType.toString();

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

		TargetMappingGroup currentMapping = animationMappings.getTargetMappingByType(animationSettingsManager.animType);

		targetObject.transform.position = currentMapping.startPositionRotation.position;
		targetObject.transform.rotation = Quaternion.Euler(currentMapping.startPositionRotation.rotation);
		
		// helper target objects, children of our target object
		animationMappings.armTargetFake = targetObject.transform.Find("ArmIK_target_helper").gameObject;
		animationMappings.thumbTargetFake = targetObject.transform.Find("ThumbIK_target_helper").gameObject;
		animationMappings.indexTargetFake = targetObject.transform.Find("IndexChainIK_target_helper").gameObject;
		animationMappings.middleTargetFake = targetObject.transform.Find("MiddleChainIK_target_helper").gameObject;
		animationMappings.ringTargetFake = targetObject.transform.Find("RingChainIK_target_helper").gameObject;
		animationMappings.pinkyTargetFake = targetObject.transform.Find("PinkyChainIK_target_helper").gameObject;

		try {
			// Setting position + rotation
			// TODO calculate for any target position
			animationMappings.setAllTargetMappings(animationSettingsManager.animType);
			animationMappings.alignTargetTransforms();
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

	public void setAnimationStartPosition() {
		GameObject originalTargetObject = GameObject.Find(animationSettingsManager.animType.ToString());
		if (!originalTargetObject) {
			 originalTargetObject = GameObject.Find(animationSettingsManager.animType.ToString() + "(Clone)");
		}

		TargetMapping _startPositionRotation = new TargetMapping(originalTargetObject.transform.position, originalTargetObject.transform.rotation.eulerAngles);
		animationMappings.getTargetMappingByType(animationSettingsManager.animType).startPositionRotation = _startPositionRotation;
	}

	public void setAnimationEndPosition() {
		GameObject originalTargetObject = GameObject.Find(animationSettingsManager.animType.ToString());
		if (!originalTargetObject) {
			 originalTargetObject = GameObject.Find(animationSettingsManager.animType.ToString() + "(Clone)");
		}

		TargetMapping _endPositionRotation = new TargetMapping(originalTargetObject.transform.position, originalTargetObject.transform.rotation.eulerAngles);
		animationMappings.getTargetMappingByType(animationSettingsManager.animType).movePositions.Add(_endPositionRotation);
	}

	public void clearAnimationEndPosition() {
		animationMappings.getTargetMappingByType(animationSettingsManager.animType).movePositions.Clear();
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
