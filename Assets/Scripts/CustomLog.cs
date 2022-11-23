using System.Collections;
using UnityEngine;

public class CustomLog : MonoBehaviour
{
    string myLog;
    Queue myLogQueue = new Queue();
    public TMPro.TMP_Text text;

 
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
