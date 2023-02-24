using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputBindingText : MonoBehaviour
{
	[SerializeField] private InputActionReference inputActionReference;
	private TMP_Text textComponent;

	void Start() {
		textComponent = GetComponent<TMP_Text>();

		// https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/ActionBindings.html?_gl=1*1k2d9xx*_ga*OTgyNjM5MDEzLjE1OTM1NTM3Mzg.*_ga_1S78EFL1W5*MTYyOTk5NDMxNy40NDUuMS4xNjI5OTk4MTgxLjI1&_ga=2.114602183.1394560935.1629563507-982639013.1593553738&_gac=1.214180453.1629682290.CjwKCAjw64eJBhAGEiwABr9o2AleTuUMGvEqxEWvNJkDGaee64a6xyJzHE7UgU2il9LRn20gQAdTThoCMKAQAvD_BwE#showing-current-bindings
		if (inputActionReference.action.bindings.Count == 1) {
			textComponent.text = InputControlPath.ToHumanReadableString(inputActionReference.action.bindings[0].effectivePath);
		} else {
			textComponent.text = inputActionReference.action.GetBindingDisplayString().Replace("/", "; ").Replace("|", " | ");
		}
		
		#if UNITY_EDITOR
			// foreach (var item in inputActionReference.action.bindings) {
			// 	Debug.Log(item.name + "__" + InputControlPath.ToHumanReadableString(item.effectivePath));
			// }
		#endif
	}
}
