using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameData {

	const string SAVE_DATA_KEY = "SaveData";
	
	public static Dictionary<string, StageData> stageData;

	static GameSaveLoad.SaveData saveData;
	static string[] pathNames;

	public static void Load(string[] pathNames) {

		stageData = new Dictionary<string, StageData>();
		saveData = GameSaveLoad.LoadGameFile(SAVE_DATA_KEY, 0);

		GameData.pathNames = pathNames;
		foreach(var item in pathNames) {

			if(stageData.ContainsKey(item)) continue;

			var data = new StageData();
			data.score = saveData.GetIntData(item + "_Score", 0);
			data.accuracy = saveData.GetFloatData(item + "_Accuracy", 0.0f);
			data.maxCombo = saveData.GetIntData(item + "_MaxCombo", 0);
			stageData.Add(item, data);
		}
	}

	public static void Save() {

		saveData = new GameSaveLoad.SaveData();

		foreach(var item in pathNames) {

			var data = stageData[item];
			saveData.SetData(item + "_Score", data.score);
			saveData.SetData(item + "_Accuracy", data.accuracy);
			saveData.SetData(item + "_MaxCombo", data.maxCombo);
		}

		GameSaveLoad.SaveGameFile(SAVE_DATA_KEY, 0, saveData);
		Load(pathNames);
	}
}

public struct StageData {
	public int score;
	public float accuracy;
	public int maxCombo;
}