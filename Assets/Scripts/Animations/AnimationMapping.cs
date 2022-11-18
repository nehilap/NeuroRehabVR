using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Mappings
{
    public class AnimationMapping
    {
        // order: armTarget; thumbTarget; indexFingerTarget; middleFingerTarget; ringFingerTarget; pinkyFingerTarget;
        public TargetMappingGroup cubeMapping;

        public TargetMappingGroup blockMapping;

        public TargetMappingGroup cupMapping;

        public TargetMappingGroup keyMapping;

        // objects which are used as targets in dynamic animations (IK)
        public GameObject armTarget;
        public GameObject thumbTarget;
        public GameObject indexTarget;
        public GameObject middleTarget;
        public GameObject ringTarget;
        public GameObject pinkyTarget;

        // objects which are children of our targeted object (cube, block, cup, key etc.)
        public GameObject armTargetFake;
        public GameObject thumbTargetFake;
        public GameObject indexTargetFake;
        public GameObject middleTargetFake;
        public GameObject ringTargetFake;
        public GameObject pinkyTargetFake;

        public AnimationMapping(GameObject _armTarget, GameObject _thumbTarget, GameObject _indexTarget, 
                 GameObject _middleTarget, GameObject _ringTarget, GameObject _pinkyTarget,
                 GameObject _armTargetFake, GameObject _thumbTargetFake, GameObject _indexTargetFake, 
                 GameObject _middleTargetFake, GameObject _ringTargetFake, GameObject _pinkyTargetFake) {
            cubeMapping = new TargetMappingGroup();
            blockMapping = new TargetMappingGroup();
            cupMapping = new TargetMappingGroup();
            keyMapping = new TargetMappingGroup();
            
            armTarget = _armTarget;
            thumbTarget = _thumbTarget;
            indexTarget = _indexTarget;
            middleTarget = _middleTarget;
            ringTarget = _ringTarget;
            pinkyTarget = _pinkyTarget;
            
            armTargetFake = _armTargetFake;
            thumbTargetFake = _thumbTargetFake;
            indexTargetFake = _indexTargetFake;
            middleTargetFake = _middleTargetFake;
            ringTargetFake = _ringTargetFake;
            pinkyTargetFake = _pinkyTargetFake;
        }

        public AnimationMapping(TargetMappingGroup _cubeMapping, TargetMappingGroup _blockMapping, TargetMappingGroup _cupMapping, TargetMappingGroup _keyMapping, 
                 GameObject _armTarget, GameObject _thumbTarget, GameObject _indexTarget,
                 GameObject _middleTarget, GameObject _ringTarget, GameObject _pinkyTarget,
                 GameObject _armTargetFake, GameObject _thumbTargetFake, GameObject _indexTargetFake, 
                 GameObject _middleTargetFake, GameObject _ringTargetFake, GameObject _pinkyTargetFake) {  
            cubeMapping = _cubeMapping;
            blockMapping = _blockMapping;
            cupMapping = _cupMapping;
            keyMapping = _keyMapping;

            armTarget = _armTarget;
            thumbTarget = _thumbTarget;
            indexTarget = _indexTarget;
            middleTarget = _middleTarget;
            ringTarget = _ringTarget;
            pinkyTarget = _pinkyTarget;
            
            armTargetFake = _armTargetFake;
            thumbTargetFake = _thumbTargetFake;
            indexTargetFake = _indexTargetFake;
            middleTargetFake = _middleTargetFake;
            ringTargetFake = _ringTargetFake;
            pinkyTargetFake = _pinkyTargetFake;
        }

        public AnimationMapping() {
            cubeMapping = new TargetMappingGroup();
            blockMapping = new TargetMappingGroup();
            cupMapping = new TargetMappingGroup();
            keyMapping = new TargetMappingGroup();
        }

        public void setTargetMapping(GameObject target, TargetMapping targetMapping, bool useRelativePosition = true, bool userRelativeRotation = true) {
            if (userRelativeRotation) {
                target.transform.localRotation = Quaternion.Euler(targetMapping.rotation);
            } else {
                target.transform.rotation = Quaternion.Euler(targetMapping.rotation);
            }

            if (useRelativePosition) {
                target.transform.localPosition = targetMapping.position;
            } else {
                target.transform.position = targetMapping.position;
            }
        }

        public void setAllTargetMappings(AnimationType animType) {
            TargetMappingGroup currentAnimMapping = getTargetMappingByType(animType);

            if (currentAnimMapping == null) {
                return;
            }
            Debug.Log(currentAnimMapping.startPositionRotation);
            Debug.Log(currentAnimMapping.movePositions);
            if (currentAnimMapping.startPositionRotation == null || currentAnimMapping.movePositions.Count <= 0) {
                Debug.LogError("Start or End animation position not set");
                throw new System.Exception("Start or End animation position not set");
            }

            // arm
            setTargetMapping(armTargetFake, currentAnimMapping.armMapping);
            
            // thumb
            setTargetMapping(thumbTargetFake, currentAnimMapping.thumbMapping);

            // index
            setTargetMapping(indexTargetFake, currentAnimMapping.indexMapping);
            
            // middle
            setTargetMapping(middleTargetFake, currentAnimMapping.middleMapping);
            
            // ring
            setTargetMapping(ringTargetFake, currentAnimMapping.ringMapping);
            
            // pinky
            setTargetMapping(pinkyTargetFake, currentAnimMapping.pinkyMapping);
        }

        public void alignTargetTransforms() {
            armTarget.transform.position = armTargetFake.transform.position;
            armTarget.transform.rotation = armTargetFake.transform.rotation;
            
            thumbTarget.transform.position = thumbTargetFake.transform.position;
            thumbTarget.transform.rotation = thumbTargetFake.transform.rotation;
            
            indexTarget.transform.position = indexTargetFake.transform.position;
            indexTarget.transform.rotation = indexTargetFake.transform.rotation;
            
            middleTarget.transform.position = middleTargetFake.transform.position;
            middleTarget.transform.rotation = middleTargetFake.transform.rotation;
            
            ringTarget.transform.position = ringTargetFake.transform.position;
            ringTarget.transform.rotation = ringTargetFake.transform.rotation;
            
            pinkyTarget.transform.position = pinkyTargetFake.transform.position;
            pinkyTarget.transform.rotation = pinkyTargetFake.transform.rotation;
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
