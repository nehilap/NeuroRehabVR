using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TabGroupButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler {

	[SerializeField] private TabGroup tabGroup;

	[SerializeField] public Image background;

	[SerializeField] public GameObject tabMenu;

	[SerializeField] public bool isActive;

	void Start() {
		background = gameObject.GetComponent<Image>();
		tabGroup.registerButton(this, isActive);
	}

	public void OnPointerEnter(PointerEventData eventData) {
		tabGroup.OnTabEnter(this);
	}

	public void OnPointerClick(PointerEventData eventData) {
		tabGroup.OnTabSelected(this);
	}

	public void OnPointerExit(PointerEventData eventData) {
		tabGroup.OnTabExit(this);
	}
}
