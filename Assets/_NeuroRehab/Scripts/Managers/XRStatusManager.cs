using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using System.Collections;
using System.Collections.Generic;
using Enums;
using System;
using UnityEngine.XR.OpenXR;
using Unity.XR.MockHMD;

public class XRStatusManager : MonoBehaviour {

	private static XRStatusManager _instance;

	public static XRStatusManager Instance {
		get {
			if(_instance == null) {
				_instance = GameObject.FindObjectOfType<XRStatusManager>();
			}
			return _instance;
		}
	}

	// https://forum.unity.com/threads/openxr-is-it-no-longer-possible-to-get-descriptive-device-names.1051493/
	public ControllerType controllerType = ControllerType.Quest2;
	public HMDType hmdType;

	public List<GameObject> controllerPrefabs = new List<GameObject>();

	public bool isXRActive;
	private bool foundHMD = false;

	[SerializeField] private ActiveBarGroupsManager activeXRButton;
	[SerializeField] private ActiveBarGroupsManager inactiveXRButton;

	[SerializeField] private GameObject XRRig;
	[SerializeField] private GameObject desktopRig;

	[SerializeField] private GameObject controllerSetupMenu;
	[SerializeField] private GameObject xrSetupMenu;

	private GameObject desktopControls;
	private GameObject xrControls;
	private GameObject mockXRControls;
	[SerializeField] private GameObject xrDeviceSimulator;

	private static XRLoader activeLoader;
	private static bool xrInitialized;

	//private XRLoader removedLoader = null;

	void Awake() {
#if UNITY_EDITOR
		if (isXRActive) {
			StartCoroutine(startXR());
		}
#endif

		DontDestroyOnLoad(gameObject);

		initHMD();
	}

	void Start() {
		initObjects();

		if ((XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager.isInitializationComplete) || xrInitialized || activeLoader != null) {
			Debug.Log("XR running");
			isXRActive = true;
		} else {
			Debug.Log("XR not running");
			isXRActive = false;
		}
		setupRigs();
		setupUIAndXRElements();
		setXRSettings();
	}

	void OnApplicationQuit() {
		stopXR();
	}

	public void stopXR() {
		if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager.isInitializationComplete) {
			XRGeneralSettings.Instance.Manager.StopSubsystems();
			XRGeneralSettings.Instance.Manager.DeinitializeLoader();

			Debug.Log("Stopped XR...");
		}
		if (xrInitialized || activeLoader != null) {
			activeLoader.Stop();
			activeLoader.Deinitialize();
			xrInitialized = false;
			activeLoader = null;

			Debug.Log("Stopped XR...");
		}
		isXRActive = false;
		Application.targetFrameRate = 60;

		setupRigs();
		setupUIAndXRElements();
	}

	public IEnumerator startXR(bool isOpenXRActive = true) {
		if (xrInitialized) {
			Debug.Log("XR is already initialized.");
			yield return null;
		}

		XRLoader loader = setupLoader(isOpenXRActive);
		if (loader == null) {
			Debug.LogError("Error resolving loader");
			yield return null;
		}

		if (activeLoader == null) {
			xrInitialized = loader.Initialize();
			if (!xrInitialized) {
				Debug.LogWarning("Initializing XR loader failed. Check log for details.");
				if (isOpenXRActive) {
					Debug.LogWarning("Failed to start OpenXR loader. Attempting to launch Mock HMD loader");
					loader.Deinitialize();

					// if there is an issue with OpenXR loader restarting itself / reloading scene after Mock HMD is loaded, refer to https://gist.github.com/Kroporo/f7201d7c9ce6dd015a461992c62cb946 for possible solution; Should be fixed in OpenXR plugin, just in case
					yield return StartCoroutine(startXR(false));
				}
			} else {
				Debug.Log("Starting XR...");
				loader.Start();
				activeLoader = loader;

				isXRActive = true;

				setupRigs();
				initHMD();

				setupUIAndXRElements();
				setXRSettings();
			}
		} else {
			isXRActive = true;

			setupRigs();
			initHMD();

			setupUIAndXRElements();
			setXRSettings();
		}
	}

	public void setXRSettings () {
		if(hmdType == HMDType.Mock){
			XRSettings.gameViewRenderMode = GameViewRenderMode.LeftEye;

			XRSettings.eyeTextureResolutionScale = 1.6f;
		}
	}

	private void setupRigs() {
		if (!this.gameObject.scene.isLoaded) return;

		if (isXRActive) {
			desktopRig.SetActive(false);
			XRRig.SetActive(true);
		} else {
			XRRig.SetActive(false);
			desktopRig.SetActive(true);

			if (desktopRig.TryGetComponent<MouseManager>(out MouseManager mouseManager)) {
				mouseManager.activeTriggers = 0;
			}
		}
	}

	private void setupUIAndXRElements() {
		if (!this.gameObject.scene.isLoaded) return;

		if (desktopControls == null) {
			initObjects();
		}

		if (isXRActive) {
			activeXRButton.activateBar();

			controllerSetupMenu.SetActive(true);

			desktopControls?.SetActive(false);
			if (hmdType == HMDType.Mock) {
				xrControls?.SetActive(false);
				mockXRControls?.SetActive(true);

				xrSetupMenu.SetActive(true);
				xrDeviceSimulator?.SetActive(true);
			} else {
				xrControls?.SetActive(true);
				mockXRControls?.SetActive(false);

				xrSetupMenu.SetActive(false);
				xrDeviceSimulator?.SetActive(false);
			}
		} else {
			inactiveXRButton.activateBar();

			controllerSetupMenu.SetActive(false);

			xrSetupMenu.SetActive(true);
			xrDeviceSimulator?.SetActive(false);

			desktopControls?.SetActive(true);
			xrControls?.SetActive(false);
			mockXRControls?.SetActive(false);
		}

		StatusTextManager.Instance.InitStatusText();
	}

	private void initObjects() {
		desktopControls = ObjectManager.Instance.getFirstObjectByName("DesktopControls");
		xrControls = ObjectManager.Instance.getFirstObjectByName("XRControls");
		mockXRControls = ObjectManager.Instance.getFirstObjectByName("MockXRControls");
	}

	/// <summary>
	/// Initializes HMD type based on either HMD discovered or platform used
	/// </summary>
	private void initHMD() {
		hmdType = HMDType.Other;

		if(XRSettings.loadedDeviceName == null || XRSettings.loadedDeviceName.Trim().Equals("")) {
			Debug.Log("No HMD discovered");
			hmdType = HMDType.NotFound;
			foundHMD = false;
		}else {
			foundHMD = true;
			Debug.Log("HMD discovered: " + XRSettings.loadedDeviceName);

			hmdType = HMDType.Other;
			if(XRSettings.loadedDeviceName.Equals("MockHMD Display")) {
				// if MockHMD, we make the game view render only using one eye
				XRSettings.gameViewRenderMode = GameViewRenderMode.LeftEye;
				hmdType = HMDType.Mock;
			}
		}

		if (!foundHMD && (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)) {
			hmdType = HMDType.Mock;
		} else if (Application.platform == RuntimePlatform.Android) {
			hmdType = HMDType.Other;
		} else if (Application.platform == RuntimePlatform.WindowsServer || Application.platform == RuntimePlatform.LinuxServer){
			hmdType = HMDType.Server;
		}
	}

	/// <summary>
	/// Swaps laoders, so that we have only one active loader. Refer to https://forum.unity.com/threads/xr-management-controlling-the-used-xrloader-manually.1019677/ for more details
	/// </summary>
	/// <param name="useOpenXRLoader"></param>
	private XRLoader setupLoader(bool useOpenXRLoader) {
		IReadOnlyList<XRLoader> loaders = XRGeneralSettings.Instance.Manager.activeLoaders;
		//bool loaderRemoved = false;
		for (int i = 0; i < loaders.Count; i++) {
			XRLoader loader = loaders[i];

			if ((useOpenXRLoader && loader.GetType() == typeof(OpenXRLoader)) ||
			 (!useOpenXRLoader && loader.GetType() == typeof(MockHMDLoader))) {
				return loader;
			}
		}
		return null;
	}
}
