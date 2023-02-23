using Enums;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SettingsMenuManager : MonoBehaviour {

	public void setControllerPrefabs(int controller) {
		if (!System.Enum.IsDefined(typeof(ControllerType), controller)) {
			Debug.LogError("Wrong enum value argument - ControllerType");
			return;
		}

		GameObject controllerL = null;
		GameObject controllerR = null;

		foreach (GameObject item in XRStatusManager.Instance.controllerPrefabs) {
			if (item.name.Contains(((ControllerType) controller).ToString())) {
				if (item.name.Contains("Left")) {
					controllerL = item;
				}else if (item.name.Contains("Right")) {
					controllerR = item;
				}
			}
		}

		XRStatusManager.Instance.controllerType = (ControllerType) controller;

		XRBaseController rightC = GameObject.Find("RightHand Controller").GetComponent<XRBaseController>();
		XRBaseController leftC = GameObject.Find("LeftHand Controller").GetComponent<XRBaseController>();

		leftC.modelPrefab = controllerL.transform;
		rightC.modelPrefab = controllerR.transform;

		if (rightC.model != null && leftC.model != null) {
			rightC.model.gameObject.SetActive(false);
			leftC.model.gameObject.SetActive(false);
		}

		if (leftC.modelParent != null) {
			leftC.model = Instantiate(leftC.modelPrefab, leftC.modelParent.transform.position, leftC.modelParent.transform.rotation, leftC.modelParent.transform);
			Outline outline = leftC.model.gameObject.AddComponent(typeof(Outline)) as Outline;
			outline.OutlineMode = Outline.Mode.OutlineHidden;
		}
		if (rightC.modelParent != null) {
			rightC.model = Instantiate(rightC.modelPrefab, rightC.modelParent.transform.position, rightC.modelParent.transform.rotation, rightC.modelParent.transform);
			Outline outline = rightC.model.gameObject.AddComponent(typeof(Outline)) as Outline;
			outline.OutlineMode = Outline.Mode.OutlineHidden;
		}
	}

	public void startXR() {
		StartCoroutine(XRStatusManager.Instance.startXR());
	}

	public void stopXR() {
		XRStatusManager.Instance.stopXR();
	}
}
