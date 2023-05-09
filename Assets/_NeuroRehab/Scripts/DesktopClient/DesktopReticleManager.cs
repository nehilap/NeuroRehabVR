using UnityEngine;

public class DesktopReticleManager : MonoBehaviour {
	private Vector3 initialScale;
	private SpriteRenderer spriteRenderer;

	private void Awake() {
		initialScale = transform.localScale;
		spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
	}

	private void OnEnable() {
		updateReticleScale();
		updateReticleStyle();

		if (SettingsManager.Instance) {
			SettingsManager.Instance.generalSettings.OnReticleChange += updateReticleStyle;
		}
	}

	private void OnDisable() {
		if (SettingsManager.Instance) {
			SettingsManager.Instance.generalSettings.OnReticleChange -= updateReticleStyle;
		}
	}

	public void updateReticleScale() {
		transform.localScale = SettingsManager.Instance.generalSettings.ReticleScale * initialScale;
	}

	private void updateReticleStyle() {
		if (SettingsManager.Instance.generalSettings.ReticleStyle == Enums.ReticleStyle.FILLED) {
			spriteRenderer.sprite = SettingsManager.Instance.reticleSpriteFilled;
		} else if (SettingsManager.Instance.generalSettings.ReticleStyle == Enums.ReticleStyle.EMPTY) {
			spriteRenderer.sprite = SettingsManager.Instance.reticleSpriteEmpty;
		}

		spriteRenderer.color = SettingsManager.Instance.generalSettings.ReticleColor;
	}
}
