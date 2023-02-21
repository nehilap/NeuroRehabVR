using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.Animations.Rigging;

using Enums;
using Mappings;
using Utility;
using System.Collections.Generic;

public class ArmAnimationController : MonoBehaviour {
	private Enums.AnimationState animState = Enums.AnimationState.Stopped;
	// private AnimationPart animPart;
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
	
	[SerializeField] private AnimationMapping animationMapping;

	[Header("Arm range and mirroring mappings objects")]
	[SerializeField] private AvatarController avatarController;
	[SerializeField] private MeshFilter armRangeMesh;
	[SerializeField] private float armRangeSlack = 0.01f;
 
	[SerializeField] private Transform _mirror;

	void Start() {
		animSettingsManager = ObjectManager.Instance.getFirstObjectByName("AnimationSettingsManager")?.GetComponent<AnimationSettingsManager>();
		if (animSettingsManager == null) {
			Debug.LogError("Failed to initialize ArmAnimationController - 'AnimationSettingsManager' not found");
			return;
		}

		armRestHelperObject = ObjectManager.Instance.getFirstObjectByName("ArmRestHelperObject" + (isLeft ? "Left" : "Right"));
		if (armRestHelperObject == null) {
			Debug.LogError("Failed to initialize ArmAnimationController - 'ArmRestHelperObject' not found");
			return;
		}
		_mirror = ObjectManager.Instance.getFirstObjectByName("MirrorPlane")?.transform;
		if (_mirror == null) {
			Debug.LogError("Failed to initialize ArmAnimationController - 'MirrorPlane' not found");
			return;
		}

		avatarController = gameObject.GetComponent<AvatarController>();
		animationMapping.resizeMappings(avatarController.sizeMultiplier);

		if (isLeft) {
			animationMapping.mirrorMappings(_mirror);
		}
	}

	private void LateUpdate() {
		alignArmRestTargetWithTable();
	}

	// https://gamedevbeginner.com/coroutines-in-unity-when-and-how-to-use-them/
	private IEnumerator armStartAnimationLerp(bool isFakeAnimation) {
		float waitDuration = 0.5f;
		float keyTurnDuration = 1f;

		// Animation control for moving arm and grabbing with hand
		// we set weight to the corresponding part we're moving
		RigLerp[] rigLerps = {new RigLerp(armRig, 0, 1), new RigLerp(restArmRig, 1, 0)};
		yield return StartCoroutine(multiRigLerp(rigLerps, animSettingsManager.armMoveDuration));

		//animPart = AnimationPart.Hand;
		
		yield return StartCoroutine(simpleRigLerp(handRig, animSettingsManager.handMoveDuration, 0, 1));
		
		//animPart = AnimationPart.Moving;

		SyncList<PosRotMapping> currentAnimSetup = animSettingsManager.getCurrentAnimationSetup();
		// Debug.Log(CharacterManager.localClient.GetInstanceID() + ",,," + CharacterManager.activePatient.GetInstanceID());		
		if (!isFakeAnimation && !(CharacterManager.localClientInstance.GetInstanceID() == CharacterManager.activePatientInstance.GetInstanceID())) {
			Debug.Log("Not original patient, aligning transform");
			yield return StartCoroutine(alignTransformWrapper(calculateAnimationDuration(animSettingsManager, waitDuration, keyTurnDuration)));
		} else {
			Debug.Log("Original patient or FakeArm, moving object");
			// if Fake Arm, we don't need to gain authority, because it is not using networked object
			if (!isFakeAnimation && (CharacterManager.localClientInstance.GetInstanceID() == CharacterManager.activePatientInstance.GetInstanceID())) {
				// we ask server to grant us authority over target object
				NetworkCharacterManager.localNetworkClientInstance.CmdSetItemAuthority(targetObject.GetComponent<NetworkIdentity>(), CharacterManager.localClientInstance.GetComponent<NetworkIdentity>());
			}

			if (animSettingsManager.animType == AnimationType.Cup) {
				yield return StartCoroutine(moveCupUpAndDown(currentAnimSetup[0], waitDuration));
			}

			for (int i = 1; i < currentAnimSetup.Count; i++) {
				yield return StartCoroutine(lerpTransform(targetObject, currentAnimSetup[i-1], currentAnimSetup[i], animSettingsManager.moveDuration, true));

				if (animSettingsManager.animType == AnimationType.Cup) {
					yield return StartCoroutine(moveCupUpAndDown(currentAnimSetup[i], waitDuration));
				}
			}

			if (animSettingsManager.animType == AnimationType.Key) {
				PosRotMapping tempMapping = currentAnimSetup[currentAnimSetup.Count - 1].Clone();
				// Theoretically both ways should work pretty well
				tempMapping.rotation.x += -90;
				//tempMapping.rotation = (Quaternion.Euler(tempMapping.rotation) * Quaternion.Euler(targetObject.transform.forward * 90)).eulerAngles;

				yield return new WaitForSeconds(waitDuration);
				yield return StartCoroutine(lerpTransform(targetObject, currentAnimSetup[currentAnimSetup.Count - 1], tempMapping, keyTurnDuration, true));
				yield return new WaitForSeconds(waitDuration);
				yield return StartCoroutine(lerpTransform(targetObject, tempMapping, currentAnimSetup[currentAnimSetup.Count - 1], keyTurnDuration, true));
				yield return new WaitForSeconds(waitDuration);
				yield return StartCoroutine(lerpTransform(targetObject, currentAnimSetup[currentAnimSetup.Count - 1], currentAnimSetup[0], animSettingsManager.moveDuration, true));
			}
		}

		// do nothing else
		stopAnimation();
	}

	// Animation control for moving arm and grabbing with hand
	private IEnumerator armStopAnimationLerp() {
		// we set weight to the corresponding part we're moving
		//animPart = AnimationPart.Hand;

		yield return StartCoroutine(simpleRigLerp(handRig, animSettingsManager.handMoveDuration, 1, 0));

		//animPart = AnimationPart.Arm;
		
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
			armRestHelperObject = GameObject.Find("ArmRestHelperObject" + (isLeft ? "Left" : "Right"));
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

	private IEnumerator moveCupUpAndDown(PosRotMapping currentAnimSetup, float waitDuration) {
		PosRotMapping tempCupMapping = currentAnimSetup.Clone();
		tempCupMapping.position += new Vector3(0, 0.2f, 0);

		yield return StartCoroutine(lerpTransform(targetObject, currentAnimSetup, tempCupMapping, animSettingsManager.moveDuration / 2, true));
		yield return new WaitForSeconds(waitDuration);
		yield return StartCoroutine(lerpTransform(targetObject, tempCupMapping, currentAnimSetup, animSettingsManager.moveDuration / 2, true));
	}

	private float calculateAnimationDuration(AnimationSettingsManager _animationSettingsManager, float _waitDuration, float _keyTurnDuration) {
		/*
			+ (currentAnimSetup.Count - 1) * _animationSettingsManager.moveDuration
			+ if (cup) (currentAnimSetup.Count * _animationSettingsManager.moveDuration) + currentAnimSetup.Count * _waitDuration// Up + Down movement
			+ if (key) (_waitDuration * 3) + (2 * _keyTurnDuration) + _animationSettingsManager.moveDuration
		 */
		SyncList<PosRotMapping> currentAnimSetup = _animationSettingsManager.getCurrentAnimationSetup();
		float duration = 0f;
		duration += (currentAnimSetup.Count - 1) * _animationSettingsManager.moveDuration;
		if (_animationSettingsManager.animType == AnimationType.Cup) {
			duration += (currentAnimSetup.Count * _animationSettingsManager.moveDuration) + currentAnimSetup.Count * _waitDuration;// Up + Down movement
		}
		if (_animationSettingsManager.animType == AnimationType.Key) {
			duration += (_waitDuration * 3) + (2 * _keyTurnDuration) + _animationSettingsManager.moveDuration;
		}

		return duration;
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
			targetObject = ObjectManager.Instance.getFirstObjectByName(targetObjectName);
			
			if (targetObject == null) {
				Debug.LogError("Failed to find object: '" + targetObjectName + "'");
				return;
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

		SyncList<PosRotMapping> currentAnimationSetup = animSettingsManager.getCurrentAnimationSetup();

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
			// Setting initial position + rotation
			targetsHelperObject.setAllTargetMappings(animationMapping.getTargetMappingByType(animSettingsManager.animType), targetObject);
			targetsHelperObject.alignTargetTransforms();
		} catch(System.Exception ex) {
			Debug.LogError(ex);
			return;
		}

		animState = Enums.AnimationState.Playing;
		//animPart = AnimationPart.Arm;

		// https://gamedevbeginner.com/coroutines-in-unity-when-and-how-to-use-them/
		StartCoroutine(armStartAnimationLerp(isFakeAnimation));
	}

	public void stopAnimation() {
		if(animState == Enums.AnimationState.Stopped) {
			return;
		}

		// We don't have to set the positions of targets here, because we're simply releasing the hand grip + moving arm to relaxed position
		//animPart = AnimationPart.Hand;

		animState = Enums.AnimationState.Stopped;
		StartCoroutine(armStopAnimationLerp());
	}

	public void setArmRestPosition() {
		isArmResting = !isArmResting;
		
		if (avatarController == null) {
			gameObject.GetComponent<AvatarController>();
		}

		if (isArmResting) {
			StartCoroutine(restArmStartAnimation());

			avatarController.applyTurn = false;
		} else {
			StartCoroutine(restArmStopAnimation());

			avatarController.applyTurn = true;
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
