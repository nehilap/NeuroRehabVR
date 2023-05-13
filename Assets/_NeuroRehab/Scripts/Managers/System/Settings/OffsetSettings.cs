using System;
using UnityEngine;

namespace NeuroRehab.Settings {
	[Serializable]
	public class OffsetSettings {
		public bool miniMenusOffsetSettingsInitialized = false;
		public bool staticMenusOffsetSettingsInitialized = false;
		public Vector3 miniMenuTransformOffset;
		public Vector3 staticMenuTransformOffset;
	}
}