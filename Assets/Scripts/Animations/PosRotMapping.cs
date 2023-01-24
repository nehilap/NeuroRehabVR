using Mirror;
using UnityEngine;

namespace Mappings
{
    [System.Serializable]
	public class PosRotMapping {
		public Vector3 position {get; set;}
		public Vector3 rotation {get; set;}

		public PosRotMapping() {
		}

		public PosRotMapping(Vector3 _position, Vector3 _rotation) {
			position = _position;
			rotation = _rotation;
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