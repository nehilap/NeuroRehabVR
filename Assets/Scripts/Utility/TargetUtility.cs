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
    }
}