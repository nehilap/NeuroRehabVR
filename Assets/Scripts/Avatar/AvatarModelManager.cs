using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarModelManager : MonoBehaviour
{
    [SerializeField]
    private GameObject avatarFemale;
    [SerializeField]
    private GameObject avatarMale;
    
    [SerializeField]
    public bool isFemale;

    public void changeModel(bool _isFemale, GameObject modelToUse) {
        isFemale = _isFemale;

        if (isFemale) {
            avatarFemale.SetActive(true);
            avatarMale.SetActive(false);

            avatarFemale.GetComponent<AvatarSetup>().setupModel(modelToUse);
        } else {
            avatarFemale.SetActive(false);
            avatarMale.SetActive(true);

            avatarMale.GetComponent<AvatarSetup>().setupModel(modelToUse);
        }
    }
}
