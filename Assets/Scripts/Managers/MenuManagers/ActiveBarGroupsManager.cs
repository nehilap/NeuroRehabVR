using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveBarGroupsManager : MonoBehaviour
{
    public List<Image> activeBars = new List<Image>();

    public void activateBar() {
        for (int i = 0; i < activeBars.Count; i++) {
            activeBars[i].enabled = false;
        }
        
        gameObject.transform.Find("ActiveBar").GetComponent<Image>().enabled = true;
    }
}
