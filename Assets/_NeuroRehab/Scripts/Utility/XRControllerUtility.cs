using UnityEngine;

/// <summary>
/// Class used to determine whether XR Controller Object is used for Left or Right arm, so that we don't have to rely on names of objects.
/// </summary>
namespace NeuroRehab.Utility {
	public class XRControllerUtility : MonoBehaviour {
		[SerializeField] public bool isLeftHandController;
	}
}