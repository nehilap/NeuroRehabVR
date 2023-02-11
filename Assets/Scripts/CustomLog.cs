using System.Collections;
using UnityEngine;

public class CustomLog : MonoBehaviour {
	private string myLog;
	private Queue myLogQueue = new Queue();
	[SerializeField] private TMPro.TMP_Text text;
 
	 void OnEnable () {
		 Application.logMessageReceived += HandleLog;
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
		 foreach(string mylog in myLogQueue){
			 myLog += mylog;
		 }
		 text.text = myLog;
	 }
}
