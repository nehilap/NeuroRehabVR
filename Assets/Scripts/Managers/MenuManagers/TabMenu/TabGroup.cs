using System.Collections.Generic;
using UnityEngine;

public class TabGroup : MonoBehaviour {

	[SerializeField] private List<TabGroupButton> tabButtons = new List<TabGroupButton>();
	[SerializeField] private Sprite tabIdle;
	[SerializeField] private Sprite tabHover;
	[SerializeField] private Sprite tabActive;

	[SerializeField] private List<GameObject> tabObjects = new List<GameObject>();

	private TabGroupButton selectedTabButton;

	public void registerButton(TabGroupButton tabButton, bool isActive) {
		tabButtons.Add(tabButton);
		tabObjects.Add(tabButton.tabMenu);

		if (isActive) {
			OnTabSelected(tabButton);
		} else {
			if (tabButton.tabMenu.TryGetComponent<Canvas>(out Canvas canvas)) {
				canvas.enabled = false;
			} else {
				tabButton.tabMenu.SetActive(false);
			}
		}
		resetTabs();
	}

	public void OnTabEnter(TabGroupButton tabButton) {
		resetTabs();
		if (selectedTabButton != null && !selectedTabButton.Equals(tabButton)) {
			tabButton.background.sprite = tabHover;
		}
	}

	public void OnTabExit(TabGroupButton tabButton) {
		resetTabs();
	}

	public void OnTabSelected(TabGroupButton tabButton) {
		selectedTabButton = tabButton;

		resetTabs();
		tabButton.background.sprite = tabActive;

		int index = tabObjects.IndexOf(tabButton.tabMenu);

		for (int i = 0; i < tabObjects.Count; i++) {
			if (i == index) {
				if (tabObjects[i].TryGetComponent<Canvas>(out Canvas canvas)) {
					canvas.enabled = true;
				} else {
					tabObjects[i].SetActive(true);
				}
			} else {
				if (tabObjects[i].TryGetComponent<Canvas>(out Canvas canvas)) {
					canvas.enabled = false;
				} else {
					tabObjects[i].SetActive(false);
				}
			}
		}
	}

	private void resetTabs() {
		foreach (var item in tabButtons) {
			if (selectedTabButton != null && item.Equals(selectedTabButton)) {
				continue;
			}
			item.background.sprite = tabIdle;	
		}
	}
}
