using System.Collections.Generic;
using UnityEngine;

public class MiniMenuVisibilityManager : MonoBehaviour {

	private List<MiniMenuManager> miniMenuManagers = new List<MiniMenuManager>();
	[SerializeField] private List<bool> isMenuShowingList = new List<bool>();

	public void registerMiniMenuManager(MiniMenuManager _miniMenuManager) {
		miniMenuManagers.Add(_miniMenuManager);

		isMenuShowingList.Add(false);
	}

	// checks if IS menu showing AND was triggered by other MiniMenuManager
	public bool isMenuShowing(MiniMenuManager _miniMenuManager) {
		int index = miniMenuManagers.IndexOf(_miniMenuManager);

		for (int i = 0; i < miniMenuManagers.Count; i++) {
			if (i == index) {
				continue;
			}
			if (isMenuShowingList[i]) {
				return true;
			}
		}

		return false;
	}

	// triggers menu for specific MenuManager
	public bool triggerMenu(MiniMenuManager _miniMenuManager) {
		int index = miniMenuManagers.IndexOf(_miniMenuManager);

		isMenuShowingList[index] = !isMenuShowingList[index];

		return isMenuShowingList[index];
	}

	public void setMenuStatus(MiniMenuManager _miniMenuManager, bool _value) {
		int index = miniMenuManagers.IndexOf(_miniMenuManager);

		isMenuShowingList[index] = _value;
	}

	public bool getMenuStatus(MiniMenuManager _miniMenuManager) {
		return isMenuShowingList[miniMenuManagers.IndexOf(_miniMenuManager)];
	}
}
