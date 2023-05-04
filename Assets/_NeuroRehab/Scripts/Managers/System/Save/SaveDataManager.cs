using System.Collections.Generic;
using UnityEngine;

public static class SaveDataManager {
	public static void SaveJsonData(IEnumerable<ISaveable> a_Saveables, string fileName) {
		SaveData sd = new SaveData();
		foreach (var saveable in a_Saveables) {
			saveable.PopulateSaveData(sd);
		}

		if (FileManager.WriteToFile(fileName, sd.ToJson())) {
			Debug.Log("Save successful");
		}
	}

	public static void LoadJsonData(IEnumerable<ISaveable> a_Saveables, string fileName) {
		if (FileManager.LoadFromFile(fileName, out var json)) {
			SaveData sd = new SaveData();
			sd.LoadFromJson(json);

			foreach (var saveable in a_Saveables) {
				saveable.LoadFromSaveData(sd);
			}

			Debug.Log("Load complete");
		}
	}
}