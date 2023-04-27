using UnityEngine;
using UnityEngine.EventSystems;

public class HoverTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	[SerializeField] private GameObject hoverInterfaceObject;

	private HoverInterface hoverInterface;

	private void Start() {
		hoverInterface = hoverInterfaceObject.GetComponent<HoverInterface>();
	}

	public void OnPointerEnter(PointerEventData eventData) {
		hoverInterface.onHoverStart();
	}

	public void OnPointerExit(PointerEventData eventData) {
		hoverInterface.onHoverStop();
	}

}
