using UnityEngine;
using UnityEngine.UI;

public class TherapistMenuManager : MonoBehaviour
{
    public Button stopAnimationButton;
    public Button playAnimationButton;
    public Button playAnimationShowcaseButton;

    // listeners for menu buttons
    // https://u3ds.blogspot.com/2021/01/get-post-rest-api-data-unitywebrequest.html
    public void setupFakeArmButtons(AnimationController animationController) {
        playAnimationShowcaseButton.onClick.AddListener(animationController.startAnimation);

        stopAnimationButton.onClick.AddListener(animationController.stopAnimation);
    }

    public void setupArmButtons(AnimationController animationController) {
        playAnimationButton.onClick.AddListener(animationController.startAnimation);

        stopAnimationButton.onClick.AddListener(animationController.stopAnimation);
    }
}
