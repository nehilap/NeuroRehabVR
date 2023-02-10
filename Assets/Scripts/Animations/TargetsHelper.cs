using Mappings;
using UnityEngine;

[System.Serializable]
public class TargetsHelper {
	
	[Header("Arm rest targets")]
	public GameObject armRestTarget;

	[Header("Arm targets")]
	// objects which are used as targets in dynamic animations (IK)
	public GameObject armTarget;
	public GameObject thumbTarget;
	public GameObject indexTarget;
	public GameObject middleTarget;
	public GameObject ringTarget;
	public GameObject pinkyTarget;

	[Header("Target object target templates (filled in code)")]
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

	
	public void setTargetMapping(GameObject target, PosRotMapping targetMapping, bool useLocalPosition = true, bool userLocalRotation = true, GameObject parentObject = null) {
		if (userLocalRotation) {
			target.transform.localRotation = Quaternion.Euler(targetMapping.rotation);
		} else {
			target.transform.rotation = Quaternion.Euler(targetMapping.rotation);
		}

		if (useLocalPosition) {
			target.transform.localPosition = targetMapping.position;
		} else {
			target.transform.position = parentObject.transform.position - targetMapping.position;
		}
	}

	public void setAllTargetMappings(TargetMappingGroup currentAnimMapping, GameObject targetObject = null) {
		if (currentAnimMapping == null) {
			return;
		}

		if (targetObject == null) {
			setTargetMapping(armTargetTemplate, currentAnimMapping.armMapping);
			setTargetMapping(thumbTargetTemplate, currentAnimMapping.thumbMapping);
			setTargetMapping(indexTargetTemplate, currentAnimMapping.indexMapping);
			setTargetMapping(middleTargetTemplate, currentAnimMapping.middleMapping);
			setTargetMapping(ringTargetTemplate, currentAnimMapping.ringMapping);
			setTargetMapping(pinkyTargetTemplate, currentAnimMapping.pinkyMapping);
		} else {
			setTargetMapping(armTargetTemplate, currentAnimMapping.armMapping, false, false, targetObject);
			setTargetMapping(thumbTargetTemplate, currentAnimMapping.thumbMapping, false, false, targetObject);
			setTargetMapping(indexTargetTemplate, currentAnimMapping.indexMapping, false, false, targetObject);
			setTargetMapping(middleTargetTemplate, currentAnimMapping.middleMapping, false, false, targetObject);
			setTargetMapping(ringTargetTemplate, currentAnimMapping.ringMapping, false, false, targetObject);
			setTargetMapping(pinkyTargetTemplate, currentAnimMapping.pinkyMapping, false, false, targetObject);
		}
	}
}
