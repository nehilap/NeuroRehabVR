using Enums;
using UnityEngine;

namespace Mappings
{
	[System.Serializable]
	public class AnimationMapping {

		[NonReorderable] public TargetMappingGroup[] targetMappingGroups;

		public TargetMappingGroup cubeMapping;

		public TargetMappingGroup blockMapping;

		public TargetMappingGroup cupMapping;

		public TargetMappingGroup keyMapping;

		public TargetMappingGroup getTargetMappingByType(AnimationType animType) {
			foreach (TargetMappingGroup item in targetMappingGroups) {
				if (item.animationType == animType) {
					return item;
				}
			}
			return null;
		}

		public void resizeMappings(float multiplier) {
			foreach (TargetMappingGroup item in targetMappingGroups) {
				item.resizeMapping(multiplier);
			}
		}

		public void mirrorMappings(Transform _mirror) {
			foreach (TargetMappingGroup item in targetMappingGroups) {
				item.mirrorMapping(_mirror);
			}
		}
	}
}
