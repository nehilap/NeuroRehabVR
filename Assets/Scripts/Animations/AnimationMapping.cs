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

		public void resizeMappings(float multiplier) {
			cubeMapping.resizeMapping(multiplier);
			blockMapping.resizeMapping(multiplier);
			cupMapping.resizeMapping(multiplier);
			keyMapping.resizeMapping(multiplier);
		}
	}
}
