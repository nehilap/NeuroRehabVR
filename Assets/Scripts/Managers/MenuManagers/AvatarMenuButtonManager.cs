using UnityEngine;
using UnityEngine.UI;

public class AvatarMenuButtonManager : MonoBehaviour {
    [SerializeField]
    private AvatarModelManager avatarModelManager;

    [SerializeField]
    private GameObject modelToUse;

    [SerializeField]
    private bool isFemale;

    [SerializeField]
    private int avatarNumber;

    private void Start() {
        transform.GetComponent<Button>().onClick.AddListener(setAvatar);
    }

    public void setAvatar() {
        avatarModelManager.changeModel(isFemale, modelToUse);
        
        SettingsManager.instance.avatarSettings.avatarNumber = avatarNumber;
        SettingsManager.instance.avatarSettings.isFemale = isFemale;
        SettingsManager.instance.avatarSettings.currentModel = modelToUse;
    }
}
