using Enums;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SettingsMenuManager : MonoBehaviour
{
    public Canvas generalMenu;

    void Start() {
    }

    public void closeSettingsMenu() {
        gameObject.GetComponent<Canvas>().enabled = false;
        generalMenu.enabled = true;
    }

    public void setControllerPrefabs(int controller) {
        GameObject controllerL = null;
        GameObject controllerR = null;

        foreach (GameObject item in HMDInfoManager.instance.controllerPrefabs) {
            if (item.name.Contains(((ControllerType) controller).ToString())) {
                if (item.name.Contains("Left")) {
                    controllerL = item;
                }else if (item.name.Contains("Right")) {
                    controllerR = item;
                }
            }
        }

        HMDInfoManager.instance.controllerType = (ControllerType) controller;

        XRBaseController rightC =  GameObject.Find("RightHand Controller").GetComponent<XRBaseController>();
        XRBaseController leftC =  GameObject.Find("LeftHand Controller").GetComponent<XRBaseController>();

        leftC.modelPrefab = controllerL.transform;
        rightC.modelPrefab = controllerR.transform;

        rightC.model.gameObject.SetActive(false);
        leftC.model.gameObject.SetActive(false);

        rightC.model = Instantiate(rightC.modelPrefab, rightC.modelParent.transform.position, rightC.modelParent.transform.rotation, rightC.modelParent.transform);
        leftC.model = Instantiate(leftC.modelPrefab, leftC.modelParent.transform.position, leftC.modelParent.transform.rotation, leftC.modelParent.transform);
    }
}
