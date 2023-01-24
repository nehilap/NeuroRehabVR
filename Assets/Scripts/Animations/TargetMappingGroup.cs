namespace Mappings
{
    [System.Serializable]
    public class TargetMappingGroup {
        // order: armTarget; thumbTarget; indexFingerTarget; middleFingerTarget; ringFingerTarget; pinkyFingerTarget;
        public PosRotMapping armMapping;
        public PosRotMapping thumbMapping;
        public PosRotMapping indexMapping;
        public PosRotMapping middleMapping;
        public PosRotMapping ringMapping;
        public PosRotMapping pinkyMapping;

        public TargetMappingGroup(PosRotMapping _armMapping, PosRotMapping _thumbMapping, PosRotMapping _indexMapping, 
                PosRotMapping _middleMapping, PosRotMapping _ringMapping, PosRotMapping _pinkyMapping) {
            armMapping = _armMapping;
            thumbMapping = _thumbMapping;
            indexMapping = _indexMapping;
            middleMapping = _middleMapping;
            ringMapping = _ringMapping;
            pinkyMapping = _pinkyMapping;
        }

        public TargetMappingGroup() {
        }
    }
}
