using Mappings;
using UnityEngine;

[System.Serializable]
public class TargetsHelper {
    
    public GameObject armRestTarget;

    // objects which are used as targets in dynamic animations (IK)
    public GameObject armTarget;
    public GameObject thumbTarget;
    public GameObject indexTarget;
    public GameObject middleTarget;
    public GameObject ringTarget;
    public GameObject pinkyTarget;

    // objects which are children of our targeted object (cube, block, cup, key etc.)
    public GameObject armTargetTemplate;
    public GameObject thumbTargetTemplate;
    public GameObject indexTargetTemplate;
    public GameObject middleTargetTemplate;
    public GameObject ringTargetTemplate;
    public GameObject pinkyTargetTemplate;

    public TargetsHelper(GameObject _armTarget, GameObject _thumbTarget, GameObject _indexTarget, 
                GameObject _middleTarget, GameObject _ringTarget, GameObject _pinkyTarget,
                GameObject _armTargetFake, GameObject _thumbTargetFake, GameObject _indexTargetFake, 
                GameObject _middleTargetFake, GameObject _ringTargetFake, GameObject _pinkyTargetFake) {
        armTarget = _armTarget;
        thumbTarget = _thumbTarget;
        indexTarget = _indexTarget;
        middleTarget = _middleTarget;
        ringTarget = _ringTarget;
        pinkyTarget = _pinkyTarget;
        
        armTargetTemplate = _armTargetFake;
        thumbTargetTemplate = _thumbTargetFake;
        indexTargetTemplate = _indexTargetFake;
        middleTargetTemplate = _middleTargetFake;
        ringTargetTemplate = _ringTargetFake;
        pinkyTargetTemplate = _pinkyTargetFake;
    }

    public void alignTargetTransforms() {
        armTarget.transform.position = armTargetTemplate.transform.position;
        armTarget.transform.rotation = armTargetTemplate.transform.rotation;
        
        thumbTarget.transform.position = thumbTargetTemplate.transform.position;
        thumbTarget.transform.rotation = thumbTargetTemplate.transform.rotation;
        
        indexTarget.transform.position = indexTargetTemplate.transform.position;
        indexTarget.transform.rotation = indexTargetTemplate.transform.rotation;
        
        middleTarget.transform.position = middleTargetTemplate.transform.position;
        middleTarget.transform.rotation = middleTargetTemplate.transform.rotation;
        
        ringTarget.transform.position = ringTargetTemplate.transform.position;
        ringTarget.transform.rotation = ringTargetTemplate.transform.rotation;
        
        pinkyTarget.transform.position = pinkyTargetTemplate.transform.position;
        pinkyTarget.transform.rotation = pinkyTargetTemplate.transform.rotation;
    }

    
        public void setTargetMapping(GameObject target, PosRotMapping targetMapping, bool useRelativePosition = true, bool userRelativeRotation = true) {
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

        public void setAllTargetMappings(TargetMappingGroup currentAnimMapping) {
            if (currentAnimMapping == null) {
                return;
            }

            // arm
            setTargetMapping(armTargetTemplate, currentAnimMapping.armMapping);
            
            // thumb
            setTargetMapping(thumbTargetTemplate, currentAnimMapping.thumbMapping);

            // index
            setTargetMapping(indexTargetTemplate, currentAnimMapping.indexMapping);
            
            // middle
            setTargetMapping(middleTargetTemplate, currentAnimMapping.middleMapping);
            
            // ring
            setTargetMapping(ringTargetTemplate, currentAnimMapping.ringMapping);
            
            // pinky
            setTargetMapping(pinkyTargetTemplate, currentAnimMapping.pinkyMapping);
        }
}
