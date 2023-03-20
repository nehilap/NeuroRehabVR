using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CustomXRInteractorLineVisual : XRInteractorLineVisual {
	[SerializeField] private bool scaleReticleWithDistance;
	[SerializeField] private float scaleFactor = 10;
	[SerializeField] private float distance;
	[SerializeField] private XRRayInteractor XRRayInteractor;
	[SerializeField] private Transform _camera;

	new protected void Awake() {
		base.Awake();

		if (base.reticle == null) {
			base.reticle = Instantiate(base.reticle);
		}
	}

	protected void OnDestroy() {
		if (reticle != null) {
			Destroy(base.reticle);
		}
	}

	// https://forum.unity.com/threads/reticle-crosshair.374076/
	// https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.2/api/UnityEngine.XR.Interaction.Toolkit.XRRayInteractor.html#UnityEngine_XR_Interaction_Toolkit_XRRayInteractor_TryGetCurrent3DRaycastHit_UnityEngine_RaycastHit__
	/// <summary>
	/// We have to implement our own scaling
	/// </summary>
	private void Update() {
		if (scaleReticleWithDistance && XRRayInteractor != null) {
			Vector3 _position, _normal;
			int _positionInLine;
			bool _isValidTarget;

			bool isHit =  XRRayInteractor.TryGetHitInfo(out _position, out _normal, out _positionInLine, out _isValidTarget);
			if (isHit) {
				if (_camera != null) {
					distance = Vector3.Distance(_camera.position, _position);
				} else {
					distance = Vector3.Distance(transform.position, _position);
				}

				base.reticle.transform.localScale = Vector3.one * distance * scaleFactor;
			}
		}
	}
}