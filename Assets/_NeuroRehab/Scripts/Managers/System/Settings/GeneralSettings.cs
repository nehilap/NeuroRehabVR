using System;
using NeuroRehab.Enums;
using UnityEngine;

namespace NeuroRehab.Settings {
	[Serializable]
	public class GeneralSettings {
		public bool measureFps = true;
		public bool writeFps = false;
		public string fpsCounterFilePath;

		[SerializeField] private float reticleScale = 1f;
		public float ReticleScale { get => reticleScale; set {
				reticleScale = value;
				if (OnReticleChange != null)
					OnReticleChange();
			}
		}

		[SerializeField] private Color reticleColor = Color.white;
		public Color ReticleColor { get => reticleColor; set {
				reticleColor = value;
				if (OnReticleChange != null)
					OnReticleChange();
			}
		}

		[SerializeField] private ReticleStyle reticleStyle = ReticleStyle.EMPTY;
		public ReticleStyle ReticleStyle { get => reticleStyle;	set {
				reticleStyle = value;
				if (OnReticleChange != null)
					OnReticleChange();
			}
		}

		public delegate void OnReticleChangeDelegate();
		public event OnReticleChangeDelegate OnReticleChange;
	}
}