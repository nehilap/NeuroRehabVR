using System;
using NeuroRehab.Enums;

namespace NeuroRehab.Settings {
	[Serializable]
	public class RoleSettings {
		public UserRole characterRole;

		public void createCharacter(UserRole role) {
			characterRole = role;
		}
	}

}
