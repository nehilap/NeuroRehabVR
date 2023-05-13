using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NeuroRehab.Settings {
	[Serializable]
	public class GraphicsSettings {
		[SerializeField] public float renderScale = 1.1f;

		[NonSerialized] private UniversalRenderPipelineAsset m_cachedRenderPipeline;
		public UniversalRenderPipelineAsset CachedRenderPipeline{
			get
			{
				if (m_cachedRenderPipeline == null)
					m_cachedRenderPipeline = (UniversalRenderPipelineAsset) QualitySettings.renderPipeline;

				return m_cachedRenderPipeline;
			}
		}

		public float currentRenderScale {
			get {
				VerifyCachedRenderPipeline ();
				if (CachedRenderPipeline == null) return -1;
				return CachedRenderPipeline.renderScale;
			}
		}

		public void SetRenderScale (float value) {
			VerifyCachedRenderPipeline ();
			if (CachedRenderPipeline == null) {
				Debug.LogError ("[QualityWrapper](SetRenderScale): Current Pipeline is null");
				return;
			}
			renderScale = Mathf.Clamp(value, 0.5f, 2);
			CachedRenderPipeline.renderScale = renderScale;
		}

		private void VerifyCachedRenderPipeline () {
			if ((UniversalRenderPipelineAsset) QualitySettings.renderPipeline == null)
				return;

			if (CachedRenderPipeline != (UniversalRenderPipelineAsset) QualitySettings.renderPipeline) {
				m_cachedRenderPipeline = (UniversalRenderPipelineAsset) QualitySettings.renderPipeline;
			}
		}
	}
}