using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Enums;

public class GeneralMenuManager : MonoBehaviour
{
    public string targetAPI;

    public Canvas settingsCanvas;

    private JoinManager joinManager;

    void Start() {
        joinManager = new JoinManager();
        
        // https://u3ds.blogspot.com/2021/01/get-post-rest-api-data-unitywebrequest.html
    }

    public void JoinTherapist() {
        RoleManager.instance.CreateCharacter(UserRole.Therapist);
        joinManager.Join();
    }

    public void JoinPatient() {
        RoleManager.instance.CreateCharacter(UserRole.Patient);
        joinManager.Join();
    }

    public void OpenSettings() {
        settingsCanvas.enabled = true;
        gameObject.GetComponent<Canvas>().enabled = false;
    }

    public void QuitApp() {
        Application.Quit();
        // UnityEditor.EditorApplication.isPlaying = false; // TODO remove later
    }

    public void SendSync() => StartCoroutine(PostData_Coroutine());
 
    IEnumerator PostData_Coroutine()
    {
        if (targetAPI == "") {
            targetAPI = "https://8f15f933-34cb-4124-a097-d0a0c0b82f95.mock.pstmn.io";
        }
        WWWForm form = new WWWForm();
        using(UnityWebRequest request = UnityWebRequest.Post(targetAPI + "/sync", form))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                Debug.LogError("Failed to retrieve response from server!!");
            else
                Debug.Log("POST request received, message: \"" + request.downloadHandler.text + "\"");
        }
    }
}
