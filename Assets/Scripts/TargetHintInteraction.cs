using UnityEngine;

public class TargetHintInteraction : MonoBehaviour
{
    
    public GameObject target;
    public GameObject hint;

    public float breakpoint = 0.0f;

    private bool belowBreak = true;
    void Update()
    {
        if(belowBreak && target.transform.localPosition.z > breakpoint) {
            hint.transform.localPosition = new Vector3(Mathf.Abs(hint.transform.localPosition.x), hint.transform.localPosition.y, hint.transform.localPosition.z);
            belowBreak = false;
        } else if (!belowBreak && target.transform.localPosition.z < breakpoint) {
            hint.transform.localPosition = new Vector3(-hint.transform.localPosition.x, hint.transform.localPosition.y, hint.transform.localPosition.z);
            belowBreak = true;
        }
    }
}
