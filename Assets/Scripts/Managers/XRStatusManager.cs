using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using System.Collections;
using System.Collections.Generic;
using Enums;

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

	void Awake() {
		#if UNITY_EDITOR
			if (isXRActive) {
				StartCoroutine(startXR());
			}
		#endif

		DontDestroyOnLoad(gameObject);
			
		hmdType = HMDType.Other;

		// NOT WORKING CURRENTLY (MOST LIKELY)
		// seems to only work when using MOCK HMD
		// discovers which type of HMD device is being used
		if(!XRSettings.isDeviceActive) {
			Debug.Log("No HMD discovered");
			hmdType = HMDType.NotFound;
		}else {
			Debug.Log("HMD discovered: " + XRSettings.loadedDeviceName);

			hmdType = HMDType.Other;
			if(XRSettings.loadedDeviceName.Equals("MockHMD Display")) { 
				// if MockHMD, we make the game view render only using one eye
				XRSettings.gameViewRenderMode = GameViewRenderMode.LeftEye;
				hmdType = HMDType.Mock;
			}
		}

		// Debug.Log(Application.platform.ToString());
		if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) {
			hmdType = HMDType.Mock;
			// StartCoroutine(startXR());
		} else if (Application.platform == RuntimePlatform.Android) {
			hmdType = HMDType.Other;
		} else {
			hmdType = HMDType.Server;
		}
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

		setupUIAndXRElements();
		setXRSettings();
	}

	void OnApplicationQuit() {
		stopXR();
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

	public IEnumerator startXR() {
		if (XRGeneralSettings.Instance.Manager.activeLoader == null) {
			yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
			if (XRGeneralSettings.Instance.Manager.activeLoader == null) {
				Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
			} else {
				if (hmdType == HMDType.Mock) {
					xrDeviceSimulator.SetActive(true);
				}

				Debug.Log("Starting XR...");
				XRGeneralSettings.Instance.Manager.StartSubsystems();
				isXRActive = true;

				setupUIAndXRElements();
				setXRSettings();
				yield return null;
			}
		} else {
			isXRActive = true;
			setupUIAndXRElements();
			setXRSettings();
		}
	}

	public void setXRSettings () {
		if(hmdType == HMDType.Mock){
			XRSettings.gameViewRenderMode = GameViewRenderMode.LeftEye;
		}
		
		// We actually have to increase the resolution scaling, to increase the image queality
		// because 1.0 creates artifacts / jagged lines
		// XRSettings.eyeTextureResolutionScale = 1f;
	}

	private void setupUIAndXRElements() {
		if (!this.gameObject.scene.isLoaded) return;

		if (desktopControls == null) {
			initObjects();
		}

		if (isXRActive) {
			activeXRButton.activateBar();

			desktopRig.SetActive(false);
			XRRig.SetActive(true);

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
			}
		} else {
			inactiveXRButton.activateBar();

			XRRig.SetActive(false);
			desktopRig.SetActive(true);

			controllerSetupMenu.SetActive(false);

			xrSetupMenu.SetActive(true);
			
			if (desktopRig.TryGetComponent<MouseManager>(out MouseManager mouseManager)) {
				mouseManager.activeTriggers = 0;
			}

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

}
