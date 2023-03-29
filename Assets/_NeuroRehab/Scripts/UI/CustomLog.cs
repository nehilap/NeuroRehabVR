using System.Collections;
using UnityEngine;

public class CustomLog : MonoBehaviour {
	private string myLog;
	private Queue myLogQueue = new Queue();
	[SerializeField] private TMPro.TMP_Text text;

	 void OnEnable () {
		 Application.logMessageReceived += HandleLog;
		 Debug.Log("Log initialized");
	 }

	 void OnDisable () {
		 Application.logMessageReceived -= HandleLog;
	 }

	 void HandleLog(string logString, string stackTrace, LogType type){
		 myLog = logString;
		 string newString = "\n [" + type + "] : " + myLog;
		 myLogQueue.Enqueue(newString);
		 if (type == LogType.Exception)
		 {
			 newString = "\n" + stackTrace;
			 myLogQueue.Enqueue(newString);
		 }
		 myLog = string.Empty;
		 for (int i = Mathf.Max(myLogQueue.Count - 15, 0); i < myLogQueue.Count; i++) {
			 myLog += myLogQueue.ToArray()[i];
		 }
		 text.text = myLog;
	 }
}
