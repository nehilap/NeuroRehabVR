using System.Collections.Generic;
using UnityEngine;
using Enums;

/// <summary>
/// Hides object if not in the list of allowed roles. Does not deactivate object, only hides it.
/// </summary>
public class RoleBasedVisibility : MonoBehaviour {
	public List<UserRole> allowedRoles = new List<UserRole>();

	void Start() {
		if (!allowedRoles.Contains(SettingsManager.Instance.roleSettings.characterRole)) {
			if (gameObject.TryGetComponent<Canvas>(out Canvas canvas)) {
				canvas.enabled = false;
			}
			if (gameObject.TryGetComponent<CanvasGroup>(out CanvasGroup canvasGroup)) {
				canvasGroup.interactable = false;
			}
			if (gameObject.TryGetComponent<Renderer>(out Renderer renderer)) {
				renderer.enabled = false;
			}
		}
	}
}
