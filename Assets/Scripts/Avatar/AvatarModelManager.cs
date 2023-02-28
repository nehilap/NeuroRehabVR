using UnityEngine;

public class AvatarModelManager : MonoBehaviour
{
	[SerializeField]
	private GameObject avatarFemale;
	[SerializeField]
	private GameObject avatarMale;
	
	[SerializeField]
	public bool isFemale;

	public GameObject changeModel(bool _isFemale, GameObject modelToUse) {
		isFemale = _isFemale;

		if (isFemale) {
			avatarFemale.SetActive(true);
			avatarMale.SetActive(false);

			avatarFemale.GetComponent<AvatarSetup>().setupModel(modelToUse);
			return avatarFemale;
		} else {
			avatarFemale.SetActive(false);
			avatarMale.SetActive(true);

			avatarMale.GetComponent<AvatarSetup>().setupModel(modelToUse);
			return avatarMale;
		}
	}

	public GameObject changeModel(bool _isFemale, GameObject modelToUse, float sizeMultiplier, float offsetDistance) {
		isFemale = _isFemale;
	
		avatarFemale.SetActive(false);
		avatarMale.SetActive(false);

		if (isFemale) {
			avatarFemale.GetComponent<AvatarController>().sizePreset = true;
			avatarFemale.GetComponent<AvatarController>().sizeMultiplier = sizeMultiplier;
			
			avatarFemale.GetComponent<AvatarLowerBodyAnimationController>().offsetPreset = true;
			avatarFemale.GetComponent<AvatarLowerBodyAnimationController>().offsetDistance = offsetDistance;

			avatarFemale.SetActive(true);
			avatarMale.SetActive(false);
			
			avatarFemale.GetComponent<AvatarSetup>().setupModel(modelToUse);

			return avatarFemale;
		} else {
			avatarMale.GetComponent<AvatarController>().sizePreset = true;
			avatarMale.GetComponent<AvatarController>().sizeMultiplier = sizeMultiplier;
			
			avatarMale.GetComponent<AvatarLowerBodyAnimationController>().offsetPreset = true;
			avatarMale.GetComponent<AvatarLowerBodyAnimationController>().offsetDistance = offsetDistance;

			avatarFemale.SetActive(false);
			avatarMale.SetActive(true);

			avatarMale.GetComponent<AvatarSetup>().setupModel(modelToUse);

			return avatarMale;
		}
	}

	public void resetHeight() {
		avatarFemale.GetComponent<AvatarController>().resetHeight();
		avatarFemale.GetComponent<AvatarLowerBodyAnimationController>().resetHeight();
		
		avatarMale.GetComponent<AvatarController>().resetHeight();
		avatarMale.GetComponent<AvatarLowerBodyAnimationController>().resetHeight();
	}
}
