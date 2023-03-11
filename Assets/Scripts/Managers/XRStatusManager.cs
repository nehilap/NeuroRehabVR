using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using System.Collections;
using System.Collections.Generic;
using Enums;
using System;

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

	private XRLoader removedLoader = null;

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

		if (XRGeneralSettings.Instance.Manager.isInitializationComplete) {
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
		if (removedLoader != null) {
			XRGeneralSettings.Instance.Manager.TryAddLoader(removedLoader);
		}
	}

	public void stopXR() {
		if (XRGeneralSettings.Instance.Manager.isInitializationComplete) {
			Debug.Log("Stopping XR...");
			XRGeneralSettings.Instance.Manager.StopSubsystems();
			XRGeneralSettings.Instance.Manager.DeinitializeLoader();

			Application.targetFrameRate = 60;

			isXRActive = false;

			setupUIAndXRElements();
		}
	}

	public IEnumerator startXR(bool isOpenXRActive = true) {
		setupLoader(isOpenXRActive);

		if (XRGeneralSettings.Instance.Manager.activeLoader == null) {
			yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

			if (XRGeneralSettings.Instance.Manager.activeLoader == null) {
				Debug.LogWarning("Initializing XR Failed. Check Editor or Player log for details.");
				if (isOpenXRActive) {
					Debug.LogWarning("Attempting to launch Mock HMD loader");
					yield return StartCoroutine(startXR(false));
				}
			} else {
				Debug.Log("Starting XR...");
				XRGeneralSettings.Instance.Manager.StartSubsystems();
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

			if (hmdType == HMDType.Mock) {
				desktopControls?.SetActive(false);
				xrControls?.SetActive(false);
				mockXRControls?.SetActive(true);

				xrSetupMenu.SetActive(true);
				xrDeviceSimulator.SetActive(true);
			} else {
				desktopControls?.SetActive(false);
				xrControls?.SetActive(true);
				mockXRControls?.SetActive(false);

				xrSetupMenu.SetActive(false);
				xrDeviceSimulator.SetActive(false);
			}
		} else {
			inactiveXRButton.activateBar();

			controllerSetupMenu.SetActive(false);

			xrSetupMenu.SetActive(true);
			xrDeviceSimulator.SetActive(false);

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

	private void setupLoader(bool isOpenXRActive) {
		if (removedLoader != null) {
			XRGeneralSettings.Instance.Manager.TryAddLoader(removedLoader);
		}

		Type openXRLoaderType = typeof(UnityEngine.XR.OpenXR.OpenXRLoader);
		IReadOnlyList<XRLoader> loaders = XRGeneralSettings.Instance.Manager.activeLoaders;
		bool loaderRemoved = false;
		for (int i = 0; i < loaders.Count; i++) {
			XRLoader loader = loaders[i];

			if (isOpenXRActive) {
				if (loader.GetType() != openXRLoaderType) {
					XRGeneralSettings.Instance.Manager.TryRemoveLoader(loader);
					loaderRemoved = true;
				}
			} else {
				if (loader.GetType() == openXRLoaderType) {
					XRGeneralSettings.Instance.Manager.TryRemoveLoader(loader);
					loaderRemoved = true;
				}
			}

			if (loaderRemoved) {
				removedLoader = loader;
				Debug.Log("Loader '" + loader.name + "' removed");
				break;
			}
		}
	}

}
