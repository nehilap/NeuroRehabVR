using UnityEngine;
using TMPro;
using Enums;
using Mappings;
using UnityEngine.Animations.Rigging;

public class AnimationController : MonoBehaviour
{
	// no need to Serialize public fields, those are serialized automatically
	public GameObject targetObject;
	public bool isFakeArm = false;

	private Enums.AnimationState animState = Enums.AnimationState.Stopped;
	private AnimationPart animPart = AnimationPart.Arm;

	private Rig armRig;
	private Rig handRig;

	private AnimationMapping animationMappings;

	private float currentLerpValue = 0f;
	private float startLerpValue = 0f;
	private float endLerpValue = 1f;
	private float lerpTimeElapsed = 0f;
	private float animationLength = 0f;

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

		// CUBE animation setup - relative values
		// TODO
		animationMappings.cubeMapping.armMapping = new TargetMapping(new Vector3(-1.104002f, 0.07f, -1.648f), new Vector3(0f, 336.925079f, 270f)); // armTarget
		animationMappings.cubeMapping.thumbMapping = new TargetMapping(new Vector3(-0.300006866f, 0.287f, 1.146f), new Vector3(1.90925431f, 8.86787796f, 17.1680984f)); // thumbTarget
		animationMappings.cubeMapping.indexMapping = new TargetMapping(new Vector3(0.200004578f, 0.326000452f, 0.654f), new Vector3(0f, 0f, 0f)); // indexTarget
		animationMappings.cubeMapping.middleMapping = new TargetMapping(new Vector3(0.186004639f, 0.104000568f, 0.68f), new Vector3(0f, 0f, 0f)); // middleTarget
		animationMappings.cubeMapping.ringMapping = new TargetMapping(new Vector3(0.267972946f, -0.161999464f, 0.664f), new Vector3(0f, 0f, 0f)); // ringTarget
		animationMappings.cubeMapping.pinkyMapping = new TargetMapping(new Vector3(0.541992188f, -0.401f, 0.618f), new Vector3(0f, 0f, 0f)); // pinkyTarget
	}

	void PrintVector3(Vector3 message, int type = 1)
	{
		if (type == 1)
			Debug.Log("X: " + message.x + "  Y: " + message.y + "  Z:" + message.z);
		if (type == 2)
			Debug.LogWarning("X: " + message.x + "  Y: " + message.y + "  Z:" + message.z);
		if (type == 3)
			Debug.LogError("X: " + message.x + "  Y: " + message.y + "  Z:" + message.z);
	}

	void Update() {

		// Animation control for moving arm and grabbing with hand
		if (animState == Enums.AnimationState.Playing) { // Linear interpolation for weights, we slowly move arm towards our goal
			// https://gamedevbeginner.com/the-right-way-to-lerp-in-unity-with-examples/
			if (lerpTimeElapsed < animationLength) {
				// despite the fact lerp should be linear, we use calculation to manipulate it, the movement then looks more natural
				float t = lerpTimeElapsed / animationLength;
				t = t* t * t * (t * (6f* t - 15f) + 10f); // https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
				currentLerpValue = Mathf.Lerp(startLerpValue, endLerpValue, t);
				lerpTimeElapsed += Time.deltaTime;
			} else { // lerp never reaches endValue, that is why we have to set it manually
				currentLerpValue = endLerpValue;
			}

			// Debug.Log(currentLerpValue);

			// we set weight to the corresponding part we're moving
			if (animPart == AnimationPart.Arm) {
				armRig.weight = currentLerpValue;
			} else if(animPart == AnimationPart.Hand){
				handRig.weight = currentLerpValue;
			} else if(animPart == AnimationPart.Moving){

			}

			if (currentLerpValue == endLerpValue) {
				// This is the end of animation
				// we check if we're releasing animation (AnimyType.Off), if yes, we first release Hand, then move Arm to relaxed position
				if (animationSettingsManager.animType == AnimationType.Off) {
					if (animPart == AnimationPart.Hand) {
						lerpTimeElapsed = 0f;
						startLerpValue = 1f;
						endLerpValue = 0f;
						animationLength = animationSettingsManager.armMoveDuration;
						animPart = AnimationPart.Arm;
					} else { // Animation is over!! OVER!!!
						animState = Enums.AnimationState.Stopped;

						if(isFakeArm) {
							targetObject.GetComponent<Renderer>().enabled = false;
							figureRenderer.enabled = false;
						} else {
							targetObject.GetComponent<Rigidbody>().useGravity = true;
						}
					}
				} else if(animationSettingsManager.animType == AnimationType.Block) {
					blockAnimHandler();
				} else if(animationSettingsManager.animType == AnimationType.Cube) {
					cubeAnimHandler();
				} else if(animationSettingsManager.animType == AnimationType.Key) {
					keyAnimHandler();
				} else if(animationSettingsManager.animType == AnimationType.Cup) {
					cupAnimHandler();
				}
			}
			
			// Debug.Log(animPart);
			// Debug.Log(animState);
		}
	}

	private void LateUpdate() {
		if(currentLerpValue == endLerpValue) {
			if(animationSettingsManager.animType == AnimationType.Block) {
				if(animPart == AnimationPart.Moving) {
					animationMappings.alignTargetTransforms();
					return; // do nothing else for now
				}
			}
		}
	}

	private void blockAnimHandler() {
		// in other cases (Animating arm to grab an object), we first move Arm, then grip with Hand
		if (animPart == AnimationPart.Arm) {
			lerpTimeElapsed = 0f;
			startLerpValue = 0f;
			endLerpValue = 1f;
			animationLength = animationSettingsManager.handMoveDuration;
			animPart = AnimationPart.Hand;
		} else if(animPart == AnimationPart.Hand) {
			animationLength = animationSettingsManager.moveDuration;
			animPart = AnimationPart.Moving;
		} else if(animPart == AnimationPart.Moving) {
			return; // do nothing for now, refer to LateUpdate for more code
		} else {
			animState = Enums.AnimationState.Stopped;
		}
	}

	private void cubeAnimHandler() {

	}

	private void keyAnimHandler() {

	}

	private void cupAnimHandler() {

	}
	
	public void startAnimation() {
		if(animationSettingsManager.animType == AnimationType.Off) {
			Debug.LogError("No animation type specified");
			return;
		}

		string targetObjectName = ""; // TODO animationSettingsManager.animType.toString();
		switch (animationSettingsManager.animType)
		{
			case AnimationType.Block:
				targetObjectName = "Block";
				break;
			case AnimationType.Cube: 
				targetObjectName = "Cube";
				break;
			case AnimationType.Cup: 
				targetObjectName = "Cup";
				break;
			case AnimationType.Key: 
				targetObjectName = "Key";
				break;
			default: return; // AnimationType.Off or some other unknown state
		}

		// setup targetObject 
		if(isFakeArm) {
			// if it's a fake arm, we also have to set the correct position of our fake object
			GameObject originalTargetObject = GameObject.Find(targetObjectName);
			targetObject = GameObject.Find(targetObjectName + "_fake");

			targetObject.transform.position = originalTargetObject.transform.position;
			targetObject.transform.rotation = originalTargetObject.transform.rotation;

			targetObject.GetComponent<Renderer>().enabled = true;
			figureRenderer.enabled = true;
		}else {
			targetObject = GameObject.Find(targetObjectName);
			targetObject.GetComponent<Rigidbody>().useGravity = false;
		}
		
		// helper target objects, children of our target object
		animationMappings.armTargetFake = targetObject.transform.Find("ArmIK_target_helper").gameObject;
		animationMappings.thumbTargetFake = targetObject.transform.Find("ThumbIK_target_helper").gameObject;
		animationMappings.indexTargetFake = targetObject.transform.Find("IndexChainIK_target_helper").gameObject;
		animationMappings.middleTargetFake = targetObject.transform.Find("MiddleChainIK_target_helper").gameObject;
		animationMappings.ringTargetFake = targetObject.transform.Find("RingChainIK_target_helper").gameObject;
		animationMappings.pinkyTargetFake = targetObject.transform.Find("PinkyChainIK_target_helper").gameObject;

		lerpTimeElapsed = 0f;
		startLerpValue = 0f;
		endLerpValue = 1f;
		animationLength = animationSettingsManager.armMoveDuration;

		// Setting position + rotation
		// TODO calculate for any target position
		animationMappings.setAllTargetMappings(animationSettingsManager.animType);
		animationMappings.alignTargetTransforms();

		animState = Enums.AnimationState.Playing;
		animPart = AnimationPart.Arm;
	}

	public void stopAnimation() {
		if(animState == Enums.AnimationState.Stopped) {
			return;
		}

		lerpTimeElapsed = 0f;
		startLerpValue = 1f;
		endLerpValue = 0f;
		animationLength = animationSettingsManager.handMoveDuration;

		// Setting position + rotation
		// We don't have to set the positions of targets here, because we're simply releasing the hand grip + moving arm to relaxed position

		animState = Enums.AnimationState.Playing;
		animPart = AnimationPart.Hand;

		// here we set it in code, since release of hand is the same state after every type of animation
		animationSettingsManager.setAnimType(AnimationType.Off);
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
