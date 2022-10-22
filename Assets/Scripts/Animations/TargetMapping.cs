using UnityEngine;

namespace Mappings
{
public class TargetMapping
    {
        public Vector3 position {get; set;}
        public Vector3 rotation {get; set;}

        public TargetMapping(Vector3 _position, Vector3 _rotation) {
            position = _position;
            rotation = _rotation;
        }
    }
}
