using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility class containing references for helper objects, renderers or customTargetPosition if required.
/// </summary>
namespace NeuroRehab.Utility {
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