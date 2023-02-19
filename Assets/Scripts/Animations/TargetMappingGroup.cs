using Enums;
using UnityEngine;

namespace Mappings
{
	[System.Serializable]
	public class TargetMappingGroup {
		public AnimationType animationType;
		// order: armTarget; thumbTarget; indexFingerTarget; middleFingerTarget; ringFingerTarget; pinkyFingerTarget;
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

			armMapping = MirrorObject(armMapping);
			thumbMapping = MirrorObject(thumbMapping);
			indexMapping = MirrorObject(indexMapping);
			middleMapping = MirrorObject(middleMapping);
			ringMapping = MirrorObject(ringMapping);
			pinkyMapping = MirrorObject(pinkyMapping);
		}
		
		private PosRotMapping MirrorObject(PosRotMapping mirroredObject) {
			PosRotMapping newMirroredObject = mirroredObject.Clone();

			Vector3 closestPoint;
			float distanceToMirror;
			Vector3 mirrorPos;

			closestPoint = mirrorPlane.ClosestPointOnPlane(mirroredObject.position);
			distanceToMirror = mirrorPlane.GetDistanceToPoint(mirroredObject.position);
	
			mirrorPos = closestPoint - mirrorPlane.normal * distanceToMirror;
	
			newMirroredObject.position = mirrorPos;
			newMirroredObject.rotation = ReflectRotation(Quaternion.Euler(mirroredObject.rotation), mirrorPlane.normal).eulerAngles;

			return newMirroredObject;
		}
	
	
		private Quaternion ReflectRotation(Quaternion source, Vector3 normal) {
			return Quaternion.LookRotation(Vector3.Reflect(source * Vector3.forward, normal), Vector3.Reflect(source * Vector3.up, normal));
		}
	}
}
