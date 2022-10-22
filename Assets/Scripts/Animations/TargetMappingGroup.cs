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

        public TargetMappingGroup(TargetMapping _armMapping, TargetMapping _thumbMapping, TargetMapping _indexMapping, 
                TargetMapping _middleMapping, TargetMapping _ringMapping, TargetMapping _pinkyMapping)
        {
            armMapping = _armMapping;
            thumbMapping = _thumbMapping;
            indexMapping = _indexMapping;
            middleMapping = _middleMapping;
            ringMapping = _ringMapping;
            pinkyMapping = _pinkyMapping;
        }

        public TargetMappingGroup() {}
    }
}
