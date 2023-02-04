using System.Collections.Generic;
using UnityEngine;
using Enums;

public class RoleBasedVisibility : MonoBehaviour
{
    public List<UserRole> allowedRoles = new List<UserRole>();
    
    void Start() {
        if (!allowedRoles.Contains(RoleManager.Instance.characterRole)) {
            Canvas cr = gameObject.GetComponent<Canvas>();
            if (cr != null) {
                cr.enabled = false;
            }

            Renderer r = gameObject.GetComponent<Renderer>();
            if (r != null) {
                r.enabled = false;
            }
        }
    }
}
