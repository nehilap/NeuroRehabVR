using Mirror;
using UnityEngine;

namespace NeuroRehab.Mappings {

	/// <summary>
	/// Custom class used to transfer two vectors together. NOT nullable when sent over network, due to CustomReadWriteFunctions restrictions. If it is needed then Serialization functions need to be changed. Can be null when using only locally.
	/// </summary>
	[System.Serializable]
	public class PosRotMapping {
		public Vector3 position;
		public Vector3 rotation;

		public PosRotMapping() {
		}

		public PosRotMapping(Vector3 _position, Vector3 _rotation) {
			position = _position;
			rotation = _rotation;
		}

		public PosRotMapping(Transform _object) {
			position = _object.position;
			rotation = _object.rotation.eulerAngles;
		}

		public override string ToString() {
			return position + " _ " + rotation;
		}

		public PosRotMapping Clone() {
			return new PosRotMapping(position, rotation);
		}

		/// <summary>
		/// Mirrors object using plane object. Refer to https://forum.unity.com/threads/mirror-reflections-in-vr.416728/#post-6067572 for more details.
		/// </summary>
		/// <param name="mirrorPlane"></param>
		/// <returns></returns>
		public PosRotMapping MirrorObject(Plane mirrorPlane) {
			PosRotMapping newMirroredObject = this.Clone();

			Vector3 closestPoint;
			float distanceToMirror;
			Vector3 mirrorPos;

			closestPoint = mirrorPlane.ClosestPointOnPlane(this.position);
			distanceToMirror = mirrorPlane.GetDistanceToPoint(this.position);

			mirrorPos = closestPoint - mirrorPlane.normal * distanceToMirror;

			newMirroredObject.position = mirrorPos;
			newMirroredObject.rotation = ReflectRotation(Quaternion.Euler(this.rotation), mirrorPlane.normal).eulerAngles;

			return newMirroredObject;
		}

		private Quaternion ReflectRotation(Quaternion source, Vector3 normal) {
			return Quaternion.LookRotation(Vector3.Reflect(source * Vector3.forward, normal), Vector3.Reflect(source * Vector3.up, normal));
		}
	}

	/// <summary>
	/// Custom Serialization methods for more complex objects, needed because we use it in SyncList and Mirror doesn't know how to do it by itself. This makes PosRotMapping objects NOT nullable, at least when they are being sent over network. Refer to https://mirror-networking.gitbook.io/docs/guides/serialization for more details.
	/// </summary>
	public static class CustomReadWriteFunctions {
		public static void WritePosRotMapping(this NetworkWriter writer, PosRotMapping value) {
			writer.WriteVector3(value.position);
			writer.WriteVector3(value.rotation);
		}

		public static PosRotMapping ReadPosRotMapping(this NetworkReader reader) {
			return new PosRotMapping(reader.ReadVector3(), reader.ReadVector3());
		}
	}
}