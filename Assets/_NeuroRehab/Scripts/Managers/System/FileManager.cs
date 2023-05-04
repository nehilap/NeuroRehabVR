using System;
using System.IO;
using UnityEngine;

/// <summary>
/// https://www.youtube.com/watch?v=uD7y4T4PVk0
/// </summary>
public static class FileManager {
	public static bool WriteToFile(string fileName, string fileContents) {
		var fullPath = Path.Combine(Application.persistentDataPath, fileName);

		try {
			File.WriteAllText(fullPath, fileContents);
			Debug.Log($"Data succesfully written to file {fullPath}.");
			return true;
		} catch (Exception e) {
			Debug.LogError($"Failed to write to {fullPath} with exception {e}");
			return false;
		}
	}

	public static bool LoadFromFile(string fileName, out string result) {
		var fullPath = Path.Combine(Application.persistentDataPath, fileName);

		try {
			result = File.ReadAllText(fullPath);
			return true;
		} catch (Exception e) {
			Debug.LogError($"Failed to read from {fullPath} with exception {e}");
			result = "";
			return false;
		}
	}
}