using NeuroRehab.Mappings;
using UnityEngine;

[System.Serializable]
public class MapTransforms {
	public bool followVRTarget = true;
	public Transform vrTarget;
	public Transform ikTarget;

	public Vector3 positionOffset;
	private Vector3 actualPosOffset;

	public Vector3 rotationOffset;
	private Vector3 actualRotOffset;

	/// <summary>
	///
	/// </summary>
	public void mapTransforms(){
		if (!followVRTarget || vrTarget == null || ikTarget == null) {
			return;
		}
		ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(actualRotOffset);

		if (vrTarget.hasChanged) {
			ikTarget.position = vrTarget.TransformPoint(actualPosOffset); // transformPoint from somewhere acts as if it was offset
		}
	}

	 public void mapTransformsDebug(Transform transform){
		if (!followVRTarget) {
			return;
		}
		ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(rotationOffset);
		ikTarget.position = vrTarget.TransformPoint(positionOffset);
	}

	/// <summary>
	/// Setting multiplier of offsets.
	/// We use extra variable so that we don't have to recalculate scale every update, just once at the beginning
	/// </summary>
	/// <param name="multiplier"></param>
	/// <param name="ignoreMulti"></param>
	public void setMulti(float multiplier, bool ignoreMulti = false) {
		if (!ignoreMulti) {
			actualPosOffset = positionOffset;
		} else {
			actualPosOffset = positionOffset * multiplier;
		}
		actualRotOffset = rotationOffset;
	}
}

public class AvatarController : MonoBehaviour {

	[SerializeField] public GameObject rightArmRangeMarker;
	[SerializeField] public GameObject leftArmRangeMarker;

	[SerializeField] public MapTransforms head;
	[SerializeField] public MapTransforms headSpine;
	[SerializeField] public MapTransforms leftHand;
	[SerializeField] public MapTransforms rightHand;

	[SerializeField] public bool applyTurn = true;
	[SerializeField] private float turnSmoothness;
	[SerializeField] private Transform headTarget;
	private Vector3 headOffset;
	[SerializeField] private Vector3 originHeadOffset;

	[SerializeField] private float referenceHeight = 1.725f;
	private float standardizedReferenceHeight = 1.725f;

	[SerializeField] private bool debug;

	private PosRotMapping initialLeftPosRot;
	private PosRotMapping initialRightPosRot;

	private bool sizeInitialized = false;
	public float sizeMultiplier;
	public bool sizePreset;

	private void Awake() {
		if (leftHand.ikTarget != null) {
			initialLeftPosRot = new PosRotMapping(leftHand.ikTarget.transform.localPosition, leftHand.ikTarget.transform.localRotation.eulerAngles);
		}

		if (rightHand.ikTarget != null) {
			initialRightPosRot = new PosRotMapping(rightHand.ikTarget.transform.localPosition, rightHand.ikTarget.transform.localRotation.eulerAngles);
		}
	}

	private void OnEnable() {
		if (!sizePreset && !sizeInitialized) {
			sizeMultiplier = calculateSizeMultiplier();

			sizeInitialized = true;
		}

		initializeValues();

		transform.position = headTarget.position + new Vector3(0, headOffset.y, 0);
		transform.position += new Vector3(transform.forward.x * headOffset.x, 0, transform.forward.z * headOffset.z);

		transform.forward = headTarget.forward;

		gameObject.GetComponent<AvatarLowerBodyAnimationController>().enabled = true;
	}

	public void resetHeight() {
		sizeMultiplier = calculateSizeMultiplier();

		initializeValues();
	}

	private void initializeValues() {
		if (gameObject.activeInHierarchy) {
			SettingsManager.Instance.avatarSettings.sizeMultiplier = sizeMultiplier;
		}
		transform.localScale = new Vector3(sizeMultiplier, sizeMultiplier, sizeMultiplier);
		headOffset = originHeadOffset * sizeMultiplier;

		/*
		Debug.Log(gameObject.name);
		Debug.Log("head offset " +headOffset);
		Debug.Log("size multi " + sizeMultiplier);
		*/
		head.setMulti(sizeMultiplier);
		headSpine.setMulti(sizeMultiplier);
		leftHand.setMulti(sizeMultiplier, true);
		rightHand.setMulti(sizeMultiplier, true);
	}

	void Update() {
		// move avatar based on head position + offset
		transform.position = headTarget.position + new Vector3(0, headOffset.y, 0);
		transform.position += new Vector3(transform.forward.x * headOffset.x, 0, transform.forward.z * headOffset.z);

		// rotate whole avatar based on heads rotation (slowly)
		if (applyTurn) {
			transform.forward = Vector3.Lerp(transform.forward, Vector3.ProjectOnPlane(headTarget.forward, Vector3.up).normalized, Time.deltaTime * turnSmoothness);
		}

		if (debug) {
			head.mapTransformsDebug(transform);
			headSpine.mapTransformsDebug(transform);
			leftHand.mapTransformsDebug(transform);
			rightHand.mapTransformsDebug(transform);
		} else {
			head.mapTransforms();
			headSpine.mapTransforms();
			leftHand.mapTransforms();
			rightHand.mapTransforms();
		}
	}

	public float calculateSizeMultiplier() {
		return Mathf.Round((head.vrTarget.TransformPoint(Vector3.zero).y / referenceHeight) * 1000) / 1000;
	}

	/// <summary>
	/// we use this to calculate how much should avatar be scaled in "multiplayer" settings
	/// </summary>
	/// <returns></returns>
	public float calculateStandardizedSizeMultiplier() {
		return Mathf.Round((standardizedReferenceHeight / referenceHeight) * 1000) / 1000;
	}

	/// <summary>
	/// Initial ikTarget position, which is used when arm is not resting. This method is used after changing arms, so that non resting position is not where arm was before it was changed into active animated arm, but next to 'body'
	/// </summary>
	public void resetHandIKTargets() {
		if (leftHand.ikTarget != null) {
			leftHand.ikTarget.localRotation = Quaternion.Euler(initialLeftPosRot.rotation);
			leftHand.ikTarget.localPosition = initialLeftPosRot.position;
			// Debug.Log("Left hand reset position " + leftHand.followVRTarget);
		}
		if (rightHand.ikTarget != null) {
			rightHand.ikTarget.localRotation = Quaternion.Euler(initialRightPosRot.rotation);
			rightHand.ikTarget.localPosition = initialRightPosRot.position;
			// Debug.Log("Right hand reset position "  + rightHand.followVRTarget);
		}
	}
}
