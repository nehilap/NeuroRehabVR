using UnityEngine;

public class HeadCollisionManager : MonoBehaviour {

    [SerializeField]
    private GameObject XRRig;

    [SerializeField]
    private CharacterController XRRigCharacterController;

    [SerializeField]
    private float collisionOffset = 0.1f;    

    void Start() {
        int controllerLayer = LayerMask.NameToLayer("XRController");
        int groundLayer = LayerMask.NameToLayer("Ground");
        int avatarLayer = LayerMask.NameToLayer("Avatar");
        Physics.IgnoreLayerCollision(transform.gameObject.layer, transform.gameObject.layer);
        Physics.IgnoreLayerCollision(transform.gameObject.layer, groundLayer);
        Physics.IgnoreLayerCollision(transform.gameObject.layer, controllerLayer);
        Physics.IgnoreLayerCollision(transform.gameObject.layer, avatarLayer);

        XRRigCharacterController = XRRig.GetComponent<CharacterController>();
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log("Trigger detected: " + other.name);
    }

    private void OnCollisionEnter(Collision collision) {
        Debug.Log("Collision detected: " + collision.transform.name);

    
        Vector3 headPosition = transform.TransformPoint(Vector3.zero);
        
        Debug.Log(headPosition);
        foreach (ContactPoint contact in collision.contacts) {
            Debug.Log(contact.point);
            float diffX = Mathf.Round((headPosition.x - contact.point.x) * 100) / 100;
            Debug.Log(diffX);
            if (diffX != 0f) {
                if (Mathf.Abs(diffX) < collisionOffset) {
                    if (diffX < 0f) {
                        diffX = -collisionOffset;
                    } else {
                        diffX = collisionOffset;
                    }
                }
                
                XRRig.transform.position += new Vector3(diffX, 0f, 0f);
                XRRigCharacterController.center += new Vector3(diffX, 0f, 0f);
                break;
            }
            float diffZ = Mathf.Round((headPosition.z - contact.point.z) * 100) / 100;
            Debug.Log(diffZ);
            if (diffZ != 0f) {
                if (Mathf.Abs(diffZ) < collisionOffset) {
                        if (diffZ < 0f) {
                            diffZ = -collisionOffset;
                        } else {
                            diffZ = collisionOffset;
                        }
                }
                
                XRRig.transform.position += new Vector3(0f, 0f, diffZ);
                XRRigCharacterController.center += new Vector3(0f, 0f, diffZ);
                break;
            }
        }
    }
}
