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
	}

	public void updateReticleScale() {
		transform.localScale = SettingsManager.Instance.generalSettings.reticleScale * initialScale;
	}

	public void updateReticleStyle() {
		if (SettingsManager.Instance.generalSettings.reticleStyle == Enums.ReticleStyle.FILLED) {
			spriteRenderer.sprite = SettingsManager.Instance.reticleSpriteFilled;
		} else if (SettingsManager.Instance.generalSettings.reticleStyle == Enums.ReticleStyle.EMPTY) {
			spriteRenderer.sprite = SettingsManager.Instance.reticleSpriteEmpty;
		}
	}
}
