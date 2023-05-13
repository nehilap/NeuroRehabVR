using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Custom class that we use to make better Reticle, that is scaled depending on distance of your camera from reticle.
/// </summary>
public class CustomXRInteractorLineVisual : XRInteractorLineVisual {

	[Header("Custom reticle")]
	[SerializeField] private bool scaleReticleWithDistance;
	[SerializeField] private float scaleFactor = 10;
	[SerializeField] private float distance;
	[SerializeField] private XRRayInteractor XRRayInteractor;
	[SerializeField] private Transform _camera;

	[SerializeField] private GameObject reticleFilled;
	[SerializeField] private GameObject reticleEmpty;

	new protected void Awake() {
		base.Awake();

		if (base.reticle == null) {
			base.reticle = Instantiate(base.reticle);
		}
	}

	new protected void OnEnable() {
		base.OnEnable();

		updateReticleStyle();
		if (SettingsManager.Instance) {
			SettingsManager.Instance.generalSettings.OnReticleChange += updateReticleStyle;
		}
	}

	protected void OnDestroy() {
		if (reticle != null) {
			Destroy(base.reticle);
		}
		if (SettingsManager.Instance) {
			SettingsManager.Instance.generalSettings.OnReticleChange -= updateReticleStyle;
		}
	}

	private void updateReticleStyle() {
		Destroy(base.reticle);
		if (SettingsManager.Instance.generalSettings.ReticleStyle == NeuroRehab.Enums.ReticleStyle.FILLED) {
			reticle = reticleFilled;
		} else if (SettingsManager.Instance.generalSettings.ReticleStyle == NeuroRehab.Enums.ReticleStyle.EMPTY) {
			reticle = reticleEmpty;
		}
		reticle.GetComponentInChildren<SpriteRenderer>().color = SettingsManager.Instance.generalSettings.ReticleColor;
	}

	// https://forum.unity.com/threads/reticle-crosshair.374076/
	// https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.2/api/UnityEngine.XR.Interaction.Toolkit.XRRayInteractor.html#UnityEngine_XR_Interaction_Toolkit_XRRayInteractor_TryGetCurrent3DRaycastHit_UnityEngine_RaycastHit__
	/// <summary>
	/// We have to implement our own scaling for reticle
	/// </summary>
	private void Update() {
		if (scaleReticleWithDistance && XRRayInteractor != null) {
			Vector3 _position, _normal;
			int _positionInLine;
			bool _isValidTarget;

			bool isHit = XRRayInteractor.TryGetHitInfo(out _position, out _normal, out _positionInLine, out _isValidTarget);
			if (isHit) {
				if (_camera != null) {
					distance = Vector3.Distance(_camera.position, _position);
				} else {
					distance = Vector3.Distance(transform.position, _position);
				}

				base.reticle.transform.localScale = Vector3.one * distance * (scaleFactor * SettingsManager.Instance.generalSettings.ReticleScale);
			}
		}
	}
}