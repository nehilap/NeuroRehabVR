using System.Collections.Generic;
using UnityEngine;

namespace Utility {
	public class TargetUtility : MonoBehaviour {
		/*
		*
		* SCRIPT USED TO IDENTIFY TARGET OBJECTS
		* this way we can search objects by type without referencing all GameObjects in the scene
		* store target helpers so we don't have to "gameObject.Find()" them every time
		*
		*/

		[SerializeField] public GameObject ArmIK_target_helper;
		[SerializeField] public GameObject ThumbIK_target_helper;
		[SerializeField] public GameObject IndexChainIK_target_helper;
		[SerializeField] public GameObject MiddleChainIK_target_helper;
		[SerializeField] public GameObject RingChainIK_target_helper;
		[SerializeField] public GameObject PinkyChainIK_target_helper;

		[SerializeField] public List<Renderer> renderers = new List<Renderer>();

		[SerializeField] public Vector3 zeroTransformPosition;
		[SerializeField] public Vector3 zeroTransformClampMin = Vector3.zero;
		[SerializeField] public Vector3 zeroTransformClampMax = new Vector3(360, 360, 360);
		[SerializeField] public bool ignoreZeroRotX = false;
		[SerializeField] public bool ignoreZeroRotY = false;
		[SerializeField] public bool ignoreZeroRotZ = false;
	}
}