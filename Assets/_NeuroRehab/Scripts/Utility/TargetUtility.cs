using System.Collections.Generic;
using UnityEngine;

namespace Utility {
	public class TargetUtility : MonoBehaviour {
		[SerializeField] public GameObject ArmIK_target_helper;
		[SerializeField] public GameObject ThumbIK_target_helper;
		[SerializeField] public GameObject IndexChainIK_target_helper;
		[SerializeField] public GameObject MiddleChainIK_target_helper;
		[SerializeField] public GameObject RingChainIK_target_helper;
		[SerializeField] public GameObject PinkyChainIK_target_helper;

		[SerializeField] public List<Renderer> renderers = new List<Renderer>();

		[SerializeField] public GameObject customTargetPos;
	}
}