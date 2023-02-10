using Mirror;
using UnityEngine;

namespace Mappings
{
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
	}

	// https://mirror-networking.gitbook.io/docs/guides/serialization
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