using UnityEngine.Animations.Rigging;

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
