using UnityEngine.Animations.Rigging;

/// <summary>
/// Helper class for lerping Rigs. We can use this when we want to lerp multiple rigs at the same time.
/// </summary>
public class RigLerp {
	public Rig rig;
	public float startValue;
	public float endValue;

	public RigLerp(Rig _rig, float _startValue, float _endValue) {
		rig = _rig;
		startValue = _startValue;
		endValue = _endValue;
	}
}
