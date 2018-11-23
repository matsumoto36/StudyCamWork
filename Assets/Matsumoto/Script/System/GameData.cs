using System.Collections.Generic;

/// <summary>
/// ゲームのセーブデータを持つクラス
/// </summary>
public static class GameData {

	private const string SaveDataKey = "SaveData";

	public static Dictionary<string, StageData> StageData;

	private static GameSaveLoad.SaveData _saveData;
	private static string[] _pathNames;

	public static void Load(string[] pathNames) {

		StageData = new Dictionary<string, StageData>();
		_saveData = GameSaveLoad.LoadGameFile(SaveDataKey, 0);
		_pathNames = pathNames;

		foreach(var item in pathNames) {

			if(StageData.ContainsKey(item)) continue;

			var data = new StageData {
				Score = _saveData.GetIntData(item + "_Score", 0),
				Accuracy = _saveData.GetFloatData(item + "_Accuracy", 0.0f),
				MaxCombo = _saveData.GetIntData(item + "_MaxCombo", 0)
			};

			StageData.Add(item, data);
		}
	}

	public static void Save() {

		_saveData = new GameSaveLoad.SaveData();

		foreach(var item in _pathNames) {

			var data = StageData[item];
			_saveData.SetData(item + "_Score", data.Score);
			_saveData.SetData(item + "_Accuracy", data.Accuracy);
			_saveData.SetData(item + "_MaxCombo", data.MaxCombo);
		}

		GameSaveLoad.SaveGameFile(SaveDataKey, 0, _saveData);
		Load(_pathNames);
	}
}

public struct StageData {
	public int Score;
	public float Accuracy;
	public int MaxCombo;
}