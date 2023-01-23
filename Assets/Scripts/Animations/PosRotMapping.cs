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
    }
}
