using System.Collections.Generic;
using UnityEngine;
using Enums;

public class RoleBasedVisibility : MonoBehaviour
{
    public List<UserRole> allowedRoles = new List<UserRole>();

    private RoleManager roleManager;
    
    void Start() {
        roleManager = GameObject.Find("GameManager").GetComponent<RoleManager>();

        if (!allowedRoles.Contains(roleManager.characterRole)) {
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
