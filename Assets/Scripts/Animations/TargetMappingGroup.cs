using System.Collections.Generic;
using UnityEngine;

namespace Mappings
{
    public class TargetMappingGroup
    {
        // order: armTarget; thumbTarget; indexFingerTarget; middleFingerTarget; ringFingerTarget; pinkyFingerTarget;
        public TargetMapping armMapping;
        public TargetMapping thumbMapping;
        public TargetMapping indexMapping;
        public TargetMapping middleMapping;
        public TargetMapping ringMapping;
        public TargetMapping pinkyMapping;

        public TargetMapping startPositionRotation;
        public List<TargetMapping> movePositions; // last one will be endPosition

        public TargetMappingGroup(TargetMapping _armMapping, TargetMapping _thumbMapping, TargetMapping _indexMapping, 
                TargetMapping _middleMapping, TargetMapping _ringMapping, TargetMapping _pinkyMapping, TargetMapping _startPositionRotation) {
            armMapping = _armMapping;
            thumbMapping = _thumbMapping;
            indexMapping = _indexMapping;
            middleMapping = _middleMapping;
            ringMapping = _ringMapping;
            pinkyMapping = _pinkyMapping;

            startPositionRotation = _startPositionRotation;
            movePositions = new List<TargetMapping>();
        }

        public TargetMappingGroup() {
            movePositions = new List<TargetMapping>();
        }
    }
}
