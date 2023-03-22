using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.Animations.Rigging;

using Enums;
using NeuroRehab.Mappings;
using NeuroRehab.Utility;

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
	private float armLength;

	void Start() {
		initElements();

		if (animSettingsManager == null) {
			Debug.LogError("Failed to initialize ArmAnimationController - 'AnimationSettingsManager' not found");
			return;
		}
		if (armRestHelperObject == null) {
			Debug.LogError("Failed to initialize ArmAnimationController - 'ArmRestHelperObject' not found");
			return;
		}
		if (_mirror == null) {
			Debug.LogError("Failed to initialize ArmAnimationController - 'MirrorPlane' not found");
			return;
		}

		animationMapping.resizeMappings(avatarController.sizeMultiplier);

		if (isLeft) {
			animationMapping.mirrorMappings(_mirror);
		}
		armLength = calculateArmLength();
		// Debug.Log(armRangeMesh.transform.lossyScale);
		// Debug.Log(armLength);
	}

	private void OnEnable() {
		armRig.gameObject.SetActive(true);
		handRig.gameObject.SetActive(true);
	}

	private void OnDisable() {
		armRig.gameObject.SetActive(false);
		handRig.gameObject.SetActive(false);
	}

	private void LateUpdate() {
		alignArmRestTargetWithTable();
	}

	public void alignArmRestTargetWithTable() {
		if (isArmResting) {
			Vector3 endPos = armRestHelperObject.transform.position + armRestOffset;

			targetsHelperObject.armRestTarget.transform.position = endPos;
			targetsHelperObject.armRestTarget.transform.rotation = armRestHelperObject.transform.rotation;
		}
	}

	private void initElements() {
		if (!animSettingsManager)
			animSettingsManager = ObjectManager.Instance.getFirstObjectByName("AnimationSettingsManager")?.GetComponent<AnimationSettingsManager>();
		if (!armRestHelperObject)
			armRestHelperObject = ObjectManager.Instance.getFirstObjectByName("ArmRestHelperObject" + (isLeft ? "Left" : "Right"));
		if (!_mirror)
			_mirror = ObjectManager.Instance.getFirstObjectByName("MirrorPlane")?.transform;
		if (!avatarController)
			avatarController = gameObject.GetComponent<AvatarController>();
	}

	/// <summary>
	/// Coroutine for animation control. Refer to https://gamedevbeginner.com/coroutines-in-unity-when-and-how-to-use-them/ for more details
	/// </summary>
	/// <param name="isFakeAnimation"></param>
	/// <returns></returns>
	private IEnumerator armStartAnimationLerp(bool isFakeAnimation) {
		float waitDuration = 0.5f;
		float keyTurnDuration = 1f;
		float cupMoveDuration = animSettingsManager.moveDuration / 2;

		SyncList<PosRotMapping> currentAnimSetup = animSettingsManager.getCurrentAnimationSetup();

		// First we move the object to it's starting position, instead of it just warping there suddenly
		yield return StartCoroutine(lerpTransform(targetObject, new PosRotMapping(targetObject.transform), currentAnimSetup[0], 1f, false));
		targetsHelperObject.alignTargetTransforms();

		// Animation control for moving arm and grabbing with hand
		// we set weight to the corresponding part we're moving
		RigLerp[] rigLerps = {new RigLerp(armRig, 0, 1), new RigLerp(restArmRig, 1, 0)};
		yield return StartCoroutine(multiRigLerp(rigLerps, animSettingsManager.armMoveDuration));

		yield return StartCoroutine(simpleRigLerp(handRig, animSettingsManager.handMoveDuration, 0, 1));

		// if it's not fake animation AND we are not an active patient, we just follow object being moved
		if (!isFakeAnimation && !(CharacterManager.localClientInstance.GetInstanceID() == CharacterManager.activePatientInstance.GetInstanceID())) {
			Debug.Log("Not original patient, aligning transform");
			yield return StartCoroutine(alignTransformWrapper(calculateAnimationDuration(animSettingsManager, waitDuration, keyTurnDuration)));
		} else {
			Debug.Log("Original patient or FakeArm, moving object");
			// if Fake Arm, we don't need to gain authority, because it is not using networked object
			if (!isFakeAnimation && (CharacterManager.localClientInstance.GetInstanceID() == CharacterManager.activePatientInstance.GetInstanceID())) {
				// we ask server to grant us authority over target object
				NetworkIdentity targetObjectIdentity = targetObject.GetComponent<NetworkIdentity>();
				if (!targetObjectIdentity.isOwned) {
					NetworkCharacterManager.localNetworkClientInstance.CmdSetItemAuthority(targetObjectIdentity, CharacterManager.localClientInstance.GetComponent<NetworkIdentity>());
				}
			}

			// Cup animation also moves object up by 0.20m
			// we do this for starting position as well
			if (animSettingsManager.animType == AnimationType.Cup) {
				yield return StartCoroutine(moveCupUpAndDown(currentAnimSetup[0], waitDuration, cupMoveDuration));
			}

			// we use one extra variable to store previous mapping, because in case we skip some steps (out of range)
			PosRotMapping previousMapping = currentAnimSetup[0].Clone();
			for (int i = 1; i < currentAnimSetup.Count; i++) {
				if (!isTargetInRange(currentAnimSetup[i].position)) {
					continue;
				}

				yield return StartCoroutine(lerpTransform(targetObject, previousMapping, currentAnimSetup[i], animSettingsManager.moveDuration, true));

				// Cup animation also moves object up by 0.20m
				if (animSettingsManager.animType == AnimationType.Cup) {
					yield return StartCoroutine(moveCupUpAndDown(currentAnimSetup[i], waitDuration, cupMoveDuration));
				}
				previousMapping = currentAnimSetup[i].Clone(); // we clone previos animation so that we make changes only to cloned versions of it
			}

			if (animSettingsManager.animType == AnimationType.Key) {
				yield return StartCoroutine(handleKeyAnimation(currentAnimSetup[currentAnimSetup.Count - 1], currentAnimSetup[0], waitDuration, keyTurnDuration));
			}
		}

		// finish animation and inform server about end of animation
		// we inform server about end of animation only if it's not fake animation playing
		stopAnimation(!isFakeAnimation);
	}

	/// <summary>
	/// Coroutine - Animation control for stopping animation
	/// </summary>
	/// <returns></returns>
	private IEnumerator armStopAnimationLerp(bool informServer) {

		// simply release hand by changing weights
		yield return StartCoroutine(simpleRigLerp(handRig, animSettingsManager.handMoveDuration, 1, 0));
		// moving hand to relaxed position by changing weights on multiple rigs at once
		RigLerp[] rigLerps = {new RigLerp(armRig, 1, 0), new RigLerp(restArmRig, 0, 1)};
		yield return StartCoroutine(multiRigLerp(rigLerps, animSettingsManager.armMoveDuration));

		foreach (Renderer item in fakeArmObjects) {
			item.enabled = false;
		}
		foreach (Renderer item in armObjects) {
			item.enabled = true;
		}

		// if we are patient, we inform Server to progress animation step by one
		// alternative check - (CharacterManager.localClientInstance.GetInstanceID() == CharacterManager.activePatientInstance.GetInstanceID())
		if (informServer && RoleManager.Instance.characterRole == UserRole.Patient) {
			NetworkCharacterManager.localNetworkClientInstance.CmdProgressAnimationStep();
		}

		// if this is a fake animation, we have to hide the fake object
		if (targetObject.name.Contains("fake")) {
			if (targetObject.TryGetComponent<TargetUtility>(out TargetUtility targetUtility)) {
				foreach (Renderer item in targetUtility.renderers) {
					item.enabled = false;
				}
			}
		} else {
			if (targetObject.TryGetComponent<Collider>(out Collider col))
				col.enabled = true;
			if (targetObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
				rb.useGravity = true;
		}
	}

	private IEnumerator restArmStartAnimation() {
		// we use variable to save old rest position - this is used either when holding arm next to your body OR when therapist changes animated arm (and rest position is active)
		originalArmRestPosRot = new PosRotMapping(targetsHelperObject.armRestTarget.transform);

		if (armRestHelperObject == null) {
			armRestHelperObject = ObjectManager.Instance.getFirstObjectByName("ArmRestHelperObject" + (isLeft ? "Left" : "Right"));
		}

		PosRotMapping startMapping = new PosRotMapping(targetsHelperObject.armRestTarget.transform);
		PosRotMapping endMapping = new PosRotMapping(armRestHelperObject.transform);
		endMapping.position += armRestOffset;

		// we simply move arm rest target to it's position
		// just to make it clear, armRestTarget is the same object as Right/LeftHandTarget, we simply have it named differently for purpose of animation
		// the reason is so that we don't need another Rig just for rest animations
		yield return StartCoroutine(lerpTransform(targetsHelperObject.armRestTarget, startMapping, endMapping, animSettingsManager.armMoveDuration));
	}

	private IEnumerator restArmStopAnimation() {
		PosRotMapping startMapping = new PosRotMapping(targetsHelperObject.armRestTarget.transform);

		// we simply move arm rest target to it's original position
		// just to make it clear, armRestTarget is the same object as Right/LeftHandTarget, we simply have it named differently for purpose of animation
		// the reason is so that we don't need another Rig just for rest animations
		yield return StartCoroutine(lerpTransform(targetsHelperObject.armRestTarget, startMapping, originalArmRestPosRot, animSettingsManager.armMoveDuration));
	}

	/// <summary>
	/// Coroutine for Linear interpolation for weights, we slowly move arm towards our goal. Refer to https://gamedevbeginner.com/the-right-way-to-lerp-in-unity-with-examples/ for more details
	/// </summary>
	/// <param name="rig"></param>
	/// <param name="lerpDuration"></param>
	/// <param name="startLerpValue"></param>
	/// <param name="endLerpValue"></param>
	/// <returns></returns>
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

	// not used anymore, replaced with superior multiRigLerp; kept just for reference
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

	/// <summary>
	/// Coroutine that is the same as simpleRigLerp but for as many rigs as needed
	/// </summary>
	/// <param name="rigLerps"></param>
	/// <param name="lerpDuration"></param>
	/// <returns></returns>
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

	/// <summary>
	/// Coroutine for smooth transform movements.
	/// </summary>
	/// <param name="startTarget">The object might not be where we want to start animation</param>
	/// <param name="startMapping"></param>
	/// <param name="endMapping"></param>
	/// <param name="duration"></param>
	/// <param name="alignTransforms"></param>
	/// <returns></returns>
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

	/// <summary>
	/// Coroutine to align Wrapper used for smooth aligning of objects
	/// </summary>
	/// <param name="duration"></param>
	/// <returns></returns>
	private IEnumerator alignTransformWrapper(float duration) {
		float time = 0;
		while (time < duration) {
			targetsHelperObject.alignTargetTransforms();
			time += Time.deltaTime;
			yield return null;
		}
		targetsHelperObject.alignTargetTransforms();
	}

	/// <summary>
	/// Coroutine for smooth cup movement, currently we move 0.20m up; Could be changed in the future via Therapist, but it seems to be good amount of movement up and down for now.
	/// </summary>
	/// <param name="currentAnimSetup"></param>
	/// <param name="waitDuration"></param>
	/// <param name="cupMoveDuration"></param>
	/// <returns></returns>
	private IEnumerator moveCupUpAndDown(PosRotMapping currentAnimSetup, float waitDuration, float cupMoveDuration) {
		PosRotMapping tempCupMapping = currentAnimSetup.Clone();
		tempCupMapping.position += new Vector3(0, 0.2f, 0);

		yield return StartCoroutine(lerpTransform(targetObject, currentAnimSetup, tempCupMapping, cupMoveDuration, true));
		yield return new WaitForSeconds(waitDuration);
		yield return StartCoroutine(lerpTransform(targetObject, tempCupMapping, currentAnimSetup, cupMoveDuration, true));
	}

	/// <summary>
	/// Coroutine to rotate the key and move key to it's initial position
	/// </summary>
	/// <param name="lastMapping"></param>
	/// <param name="initialMapping"></param>
	/// <param name="waitDuration"></param>
	/// <param name="keyTurnDuration"></param>
	/// <returns></returns>
	private IEnumerator handleKeyAnimation(PosRotMapping lastMapping, PosRotMapping initialMapping, float waitDuration, float keyTurnDuration) {
		PosRotMapping tempMapping = lastMapping.Clone();
		// Theoretically both ways should work pretty well
		tempMapping.rotation.x += -90;
		//tempMapping.rotation = (Quaternion.Euler(tempMapping.rotation) * Quaternion.Euler(targetObject.transform.forward * 90)).eulerAngles;

		yield return new WaitForSeconds(waitDuration);
		yield return StartCoroutine(lerpTransform(targetObject, lastMapping, tempMapping, keyTurnDuration, true));
		yield return new WaitForSeconds(waitDuration);
		yield return StartCoroutine(lerpTransform(targetObject, tempMapping, lastMapping, keyTurnDuration, true));
		yield return new WaitForSeconds(waitDuration);
		yield return StartCoroutine(lerpTransform(targetObject, lastMapping, initialMapping, animSettingsManager.moveDuration, true));
	}

	/// <summary>
	/// Algorithm used to calculate FULL duration of animation with all it's wait times etc., so that main client knows how long to align transforms. We take into account that some positions are not in range
	/// </summary>
	/// <param name="_animationSettingsManager"></param>
	/// <param name="_waitDuration"></param>
	/// <param name="_keyTurnDuration"></param>
	/// <returns></returns>
	private float calculateAnimationDuration(AnimationSettingsManager _animationSettingsManager, float _waitDuration, float _keyTurnDuration) {
		/*
			+ (currentAnimSetup.Count - 1) * _animationSettingsManager.moveDuration
			+ if (cup) (currentAnimSetup.Count * _animationSettingsManager.moveDuration) + currentAnimSetup.Count * _waitDuration // Up + Down movement
			+ if (key) (_waitDuration * 3) + (2 * _keyTurnDuration) + _animationSettingsManager.moveDuration
		 */
		SyncList<PosRotMapping> currentAnimSetup = _animationSettingsManager.getCurrentAnimationSetup();
		int animCount = currentAnimSetup.Count;
		for (int i = 0; i < currentAnimSetup.Count; i++) {
			if (!isTargetInRange(currentAnimSetup[i].position)) {
				animCount--;
			}
		}
		if(animCount == 0) {
			return 0f;
		}

		float duration = 0f;
		duration += (animCount - 1) * _animationSettingsManager.moveDuration;
		if (_animationSettingsManager.animType == AnimationType.Cup) {
			duration += (animCount * _animationSettingsManager.moveDuration) + animCount * _waitDuration;// Up + Down movement
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

		SyncList<PosRotMapping> currentAnimationSetup = animSettingsManager.getCurrentAnimationSetup();

		if (currentAnimationSetup.Count < 1) {
			Debug.LogError("Too few animation positions set: '" + currentAnimationSetup.Count + "'");
			return;
		}

		// Initial starting position HAS to be in arm range
		if (!isTargetInRange(currentAnimationSetup[0].position)) {
			float targetDistance = Vector3.Distance(currentAnimationSetup[0].position, armRangeMesh.transform.position);
			Debug.LogWarning("Arm cannot grab object, too far away: " + targetDistance + "m > " + armLength + "m");
			return;
		}

		targetObject = ObjectManager.Instance.getFirstObjectByName(targetObjectName + (isFakeAnimation ? "_fake" : ""));
		if (targetObject == null) {
			Debug.LogError("Failed to find object: '" + targetObjectName + (isFakeAnimation ? "_fake" : "") + "'");
			return;
		}

		// setup targetObject
		if(isFakeAnimation) {
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
		}

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

		// We have to disable Gravity and Collider for the objects, so that they don't collide with stuff (for example key + lock)
		if (targetObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
			rb.useGravity = false;
		if (targetObject.TryGetComponent<Collider>(out Collider col))
			col.enabled = false;

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

	public void stopAnimation(bool informServer = false) {
		if(animState == Enums.AnimationState.Stopped) {
			return;
		}

		// We don't have to set the positions of targets here, because we're simply releasing the hand grip + moving arm to relaxed position
		// all the movements are done using rig weights

		animState = Enums.AnimationState.Stopped;
		StartCoroutine(armStopAnimationLerp(informServer));
	}

	public void setArmRestPosition(bool _isArmResting) {
		initElements();

		isArmResting = _isArmResting;

		if (isArmResting) {
			StartCoroutine(restArmStartAnimation());

			avatarController.applyTurn = false; // when arm is resting, we disable torso and whole body rotation based on Head rotation
		} else {
			StartCoroutine(restArmStopAnimation());

			avatarController.applyTurn = true;
		}
	}

	public bool isTargetInRange(Vector3 targetPosition) {
		float targetDistance = Vector3.Distance(targetPosition, armRangeMesh.transform.position);
		return armLength > (targetDistance - armRangeSlack);
	}

	public float calculateArmLength() {
		return armRangeMesh.transform.lossyScale.x * armRangeMesh.sharedMesh.bounds.extents.x;
	}

	public float getArmLength() {
		return armLength;
	}

	public float getArmRangeSlack() {
		return armRangeSlack;
	}

	public Vector3 getArmRangePosition() {
		return armRangeMesh.transform.position;
	}

	/// <summary>
	/// Helper method for printing vectors with more decimal places
	/// </summary>
	/// <param name="message">Vector printed</param>
	/// <param name="type">Not mandatory: 1 - Log; 2 - Warning; 3 - Error</param>
	public static void PrintVector3(Vector3 message, int type = 1) {
		if (type == 1)
			Debug.Log("X: " + message.x + " Y: " + message.y + " Z:" + message.z);
		if (type == 2)
			Debug.LogWarning("X: " + message.x + " Y: " + message.y + " Z:" + message.z);
		if (type == 3)
			Debug.LogError("X: " + message.x + " Y: " + message.y + " Z:" + message.z);
	}
}
