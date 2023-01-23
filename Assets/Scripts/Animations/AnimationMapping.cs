using Enums;

namespace Mappings
{
    [System.Serializable]
    public class AnimationMapping {
        // order: armTarget; thumbTarget; indexFingerTarget; middleFingerTarget; ringFingerTarget; pinkyFingerTarget;
        public TargetMappingGroup cubeMapping;

        public TargetMappingGroup blockMapping;

        public TargetMappingGroup cupMapping;

        public TargetMappingGroup keyMapping;

        public AnimationMapping() {
            cubeMapping = new TargetMappingGroup();
            blockMapping = new TargetMappingGroup();
            cupMapping = new TargetMappingGroup();
            keyMapping = new TargetMappingGroup();
        }

        public AnimationMapping(TargetMappingGroup _cubeMapping, TargetMappingGroup _blockMapping, TargetMappingGroup _cupMapping, TargetMappingGroup _keyMapping) {  
            cubeMapping = _cubeMapping;
            blockMapping = _blockMapping;
            cupMapping = _cupMapping;
            keyMapping = _keyMapping;
        }


        public TargetMappingGroup getTargetMappingByType(AnimationType animType) {
            switch (animType) {
                case AnimationType.Block:
                    return blockMapping;
                case AnimationType.Cube: 
                    return cubeMapping;
                case AnimationType.Cup: 
                    return cupMapping;
                case AnimationType.Key: 
                    return keyMapping;
                default: return null; // AnimType.Off
            }
        }
    }
}
