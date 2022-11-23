using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
    public bool followPosition = true;

    public bool followPositionX = true;
    public bool followPositionY = true;
    public bool followPositionZ = true;

    public bool followRotation = false;
    public bool followRotationX = false;
    public bool followRotationY = false;
    public bool followRotationZ = false;

    public GameObject objectToFollow;

    public Vector3 positionDiff = new Vector3();

    public Vector3 rotationDiff = new Vector3();

    // Update is called once per frame
    void LateUpdate() {
        if (followRotation) {
            float rotX = (objectToFollow.transform.rotation.eulerAngles.x + rotationDiff.x) * (followRotationX ? 1 : 0);
            float rotY = (objectToFollow.transform.rotation.eulerAngles.y + rotationDiff.y) * (followRotationY ? 1 : 0);
            float rotZ = (objectToFollow.transform.rotation.eulerAngles.z + rotationDiff.z) * (followRotationZ ? 1 : 0);
            transform.rotation = Quaternion.Euler(rotX, rotY, rotZ);
        }
        if (followPosition) {
            float posX = (objectToFollow.transform.position.x + positionDiff.x) * (followPositionX ? 1 : 0);
            float posY = (objectToFollow.transform.position.y + positionDiff.y) * (followPositionY ? 1 : 0);
            float posZ = (objectToFollow.transform.position.z + positionDiff.z) * (followPositionZ ? 1 : 0);
            transform.position = new Vector3(posX, posY, posZ);
        }
    }
}
