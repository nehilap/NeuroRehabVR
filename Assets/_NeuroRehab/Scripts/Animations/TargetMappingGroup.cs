using Enums;
using UnityEngine;

namespace Mappings {

	// Contains mapping positioning for hand for certain animation
	[System.Serializable]
	public class TargetMappingGroup {
		public AnimationType animationType;
		public PosRotMapping armMapping;
		public PosRotMapping thumbMapping;
		public PosRotMapping indexMapping;
		public PosRotMapping middleMapping;
		public PosRotMapping ringMapping;
		public PosRotMapping pinkyMapping;
		private Plane mirrorPlane;

		public TargetMappingGroup(AnimationType _animationType, PosRotMapping _armMapping, PosRotMapping _thumbMapping, PosRotMapping _indexMapping,
				PosRotMapping _middleMapping, PosRotMapping _ringMapping, PosRotMapping _pinkyMapping) {
			animationType = _animationType;
			armMapping = _armMapping;
			thumbMapping = _thumbMapping;
			indexMapping = _indexMapping;
			middleMapping = _middleMapping;
			ringMapping = _ringMapping;
			pinkyMapping = _pinkyMapping;
		}

		public void resizeMapping(float multiplier) {
			armMapping.position = armMapping.position * multiplier;
			thumbMapping.position = thumbMapping.position * multiplier;
			indexMapping.position = indexMapping.position * multiplier;
			middleMapping.position = middleMapping.position * multiplier;
			ringMapping.position = ringMapping.position * multiplier;
			pinkyMapping.position = pinkyMapping.position * multiplier;
		}

		public void mirrorMapping(Transform _mirror) {
			mirrorPlane = new Plane(_mirror.forward, _mirror.position);

			armMapping = armMapping.MirrorObject(mirrorPlane);
			thumbMapping = thumbMapping.MirrorObject(mirrorPlane);
			indexMapping = indexMapping.MirrorObject(mirrorPlane);
			middleMapping = middleMapping.MirrorObject(mirrorPlane);
			ringMapping = ringMapping.MirrorObject(mirrorPlane);
			pinkyMapping = pinkyMapping.MirrorObject(mirrorPlane);
		}
	}
}
