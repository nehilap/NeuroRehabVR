using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveBarGroupsManager : MonoBehaviour
{
    [SerializeField] private List<Image> activeBars = new List<Image>();

    private void Start() {
        transform.GetComponent<Button>().onClick.AddListener(activateBar);
    }

    public void activateBar() {
        for (int i = 0; i < activeBars.Count; i++) {
            if (activeBars[i].transform.parent == transform) {
                activeBars[i].enabled = true;
            } else {
                activeBars[i].enabled = false;
            }
        }
    }
}
