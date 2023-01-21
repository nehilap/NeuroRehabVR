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

    [SerializeField]
    private MapTransforms head;
    [SerializeField]
    private MapTransforms headSpine;
    [SerializeField]
    private MapTransforms leftHand;
    [SerializeField]
    private MapTransforms rightHand;
    
    [SerializeField]
    private float turnSmoothness;
    [SerializeField]
    private Transform headTarget;
    [SerializeField]
    public Vector3 headOffset;

    [SerializeField]
    private float referenceHeight = 1.725f;
    public float sizeMultiplier { get; private set; }

    void Start() {
        sizeMultiplier = head.vrTarget.TransformPoint(Vector3.zero).y / referenceHeight;

        transform.localScale = new Vector3(sizeMultiplier, sizeMultiplier, sizeMultiplier);
        headOffset *= sizeMultiplier;
    }

    void LateUpdate() {
        transform.position = headTarget.position + new Vector3(0, headOffset.y, 0);
        transform.position += new Vector3(transform.forward.x * headOffset.x, 0, transform.forward.z * headOffset.z);

        transform.forward = Vector3.Lerp(transform.forward, Vector3.ProjectOnPlane(headTarget.forward, Vector3.up).normalized, Time.deltaTime * turnSmoothness);

        head.mapTransforms(sizeMultiplier);
        headSpine.mapTransforms(sizeMultiplier);
        leftHand.mapTransforms(sizeMultiplier);
        rightHand.mapTransforms(sizeMultiplier);
    }
}
