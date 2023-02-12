using UnityEngine;

[System.Serializable]
public class MapTransforms {
	public bool applyIk = true;
	public Transform vrTarget;
	public Transform ikTarget;

	public Vector3 positionOffset;
	public Vector3 rotationOffset;

	public void mapTransforms(float multiplier){
		if (!applyIk) {
			return;
		}

		ikTarget.position = vrTarget.TransformPoint(positionOffset * multiplier);
		ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(rotationOffset);
	}
}

public class AvatarController : MonoBehaviour {

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
	public float sizeMultiplier;
	public bool sizePreset;

	private void Awake() {
		originHeadOffset = headOffset;
	}

	private void OnEnable() {
		if (!sizePreset) {
			sizeMultiplier = calculateSizeMultiplier();

			SettingsManager.Instance.avatarSettings.sizeMultiplier = sizeMultiplier;
		}

		transform.localScale = new Vector3(sizeMultiplier, sizeMultiplier, sizeMultiplier);
		headOffset = originHeadOffset * sizeMultiplier;
	}

	void LateUpdate() {
		transform.position = headTarget.position + new Vector3(0, headOffset.y, 0);
		transform.position += new Vector3(transform.forward.x * headOffset.x, 0, transform.forward.z * headOffset.z);

		if (applyTurn) {
			transform.forward = Vector3.Lerp(transform.forward, Vector3.ProjectOnPlane(headTarget.forward, Vector3.up).normalized, Time.deltaTime * turnSmoothness);
		}

		head.mapTransforms(sizeMultiplier);
		headSpine.mapTransforms(sizeMultiplier);
		leftHand.mapTransforms(sizeMultiplier);
		rightHand.mapTransforms(sizeMultiplier);
	}

	public float calculateSizeMultiplier() {
		return head.vrTarget.TransformPoint(Vector3.zero).y / referenceHeight;
	}
}
