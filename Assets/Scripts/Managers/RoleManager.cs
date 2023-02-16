using UnityEngine;
using Enums;

// Component holding information about Character such as role etc
// this component is attached to object that is not despawned
public class RoleManager : MonoBehaviour {
	private static RoleManager _instance;
	public static RoleManager Instance { get { return _instance; } }

	public UserRole characterRole;

	private void Awake()
	{
		if (_instance != null && _instance != this) {
			Destroy(this.gameObject);
		} else {
			_instance = this;
		}
	}

	public void CreateCharacter(UserRole role) {
		characterRole = role;
	}
}
