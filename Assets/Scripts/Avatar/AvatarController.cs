using Mappings;
using UnityEngine;

[System.Serializable]
public class MapTransforms {
	public bool followVRTarget = true;
	// public bool applyDirectionalOffset;
	public Transform vrTarget;
	public Transform ikTarget;

	public Vector3 positionOffset;
	// public Vector3 directionalOffset;
	private Vector3 actualPosOffset;

	public Vector3 rotationOffset;
	private Vector3 actualRotOffset;

	public void mapTransforms(){
		if (!followVRTarget || vrTarget == null || ikTarget == null) {
			return;
		}

		if (vrTarget.hasChanged) {
			ikTarget.position = vrTarget.TransformPoint(actualPosOffset);
		}

		ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(actualRotOffset);
	}

	 public void mapTransformsDebug(Transform transform){
		if (!followVRTarget) {
			return;
		}
		ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(rotationOffset);

		// not working sadly
		/*if (applyDirectionalOffset) {
			Vector3 offset = positionOffset;
			ikTarget.position = vrTarget.TransformPoint(positionOffset);

			float upwardOffset = Mathf.Abs(transform.forward.y - (1f - Mathf.Abs(ikTarget.forward.y))) * directionalOffset.y;
			float forwardOffset = Mathf.Abs(transform.forward.z - (1f - Mathf.Abs(ikTarget.forward.z))) * directionalOffset.z;
			float sidewayOffset = Mathf.Abs(transform.forward.x - (1f - Mathf.Abs(ikTarget.forward.x))) * directionalOffset.x;

			offset = new Vector3(sidewayOffset, upwardOffset, forwardOffset);
			Debug.Log("model " + transform.forward);
			Debug.Log(ikTarget.forward);
			ikTarget.localPosition += offset;
		} else {*/
			ikTarget.position = vrTarget.TransformPoint(positionOffset);
		//}

		//ikTarget.position = vrTarget.TransformPoint(positionOffset);
	} 

	public void setMulti(float multiplier) {
		actualPosOffset = positionOffset * multiplier;
		actualRotOffset = rotationOffset * multiplier;
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
	[SerializeField] public Vector3 headOffset;
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
		originHeadOffset = headOffset;

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

		SettingsManager.Instance.avatarSettings.sizeMultiplier = sizeMultiplier;
		transform.localScale = new Vector3(sizeMultiplier, sizeMultiplier, sizeMultiplier);
		headOffset = originHeadOffset * sizeMultiplier;
/*
		Debug.Log(gameObject.name);
		Debug.Log("head offset " +headOffset);
		Debug.Log("size multi " + sizeMultiplier);
*/
		head.setMulti(sizeMultiplier);
		headSpine.setMulti(sizeMultiplier);
		leftHand.setMulti(sizeMultiplier);
		rightHand.setMulti(sizeMultiplier);

		transform.position = headTarget.position + new Vector3(0, headOffset.y, 0);
		transform.position += new Vector3(transform.forward.x * headOffset.x, 0, transform.forward.z * headOffset.z);

		transform.forward = headTarget.forward;

		gameObject.GetComponent<AvatarLowerBodyAnimationController>().enabled = true;
	}

	public void resetHeight() {
		sizeMultiplier = calculateSizeMultiplier();

		SettingsManager.Instance.avatarSettings.sizeMultiplier = sizeMultiplier;
		transform.localScale = new Vector3(sizeMultiplier, sizeMultiplier, sizeMultiplier);
		headOffset = originHeadOffset * sizeMultiplier;

		/* Debug.Log("head offset " + headOffset);
		Debug.Log("size multi " + sizeMultiplier); */
		head.setMulti(sizeMultiplier);
		headSpine.setMulti(sizeMultiplier);
		leftHand.setMulti(sizeMultiplier);
		rightHand.setMulti(sizeMultiplier);
	}

	void LateUpdate() {
		transform.position = headTarget.position + new Vector3(0, headOffset.y, 0);
		transform.position += new Vector3(transform.forward.x * headOffset.x, 0, transform.forward.z * headOffset.z);

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

	public float calculateStandardizedSizeMultiplier() {
		return Mathf.Round((standardizedReferenceHeight / referenceHeight) * 1000) / 1000;
	}

	public void resetHandIKTargets() {
		if (leftHand.ikTarget != null) {
			leftHand.ikTarget.localRotation = Quaternion.Euler(initialLeftPosRot.rotation);
			leftHand.ikTarget.localPosition = initialLeftPosRot.position;
		}
		if (rightHand.ikTarget != null) {
			rightHand.ikTarget.localRotation = Quaternion.Euler(initialRightPosRot.rotation);
			rightHand.ikTarget.localPosition = initialRightPosRot.position;
		}
	}
}
