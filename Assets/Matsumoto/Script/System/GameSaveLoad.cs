using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System;

/**
	[使い方]
	基本的にPlayerPrefsと同じです。
	1. SaveData クラスを生成して、SetDataで書き込みます。(int, float, string)の3種類対応
	2. GetIntData系列を使うと内容を持ち出すことができます。
	3. ファイルとして保存する場合はSaveGameFileを実行してください。
	4. ロードする場合はLoadGameFileを実行してください。
	※以下の文字はキーとして使わないでください(データが壊れます)
	| ,
*/

//
// Game Save/Load System.
//
// IN  : SaveData
// OUT : SaveData
//
public static class GameSaveLoad {

	//
	//SaveDataClass
	//
	public class SaveData {

		public enum DataKey { All = -1, Int = 0, Float, String }

		private readonly int[] _count = new int[3];// 0:int 1:float 2:string

		//DataSet(int, float, string)
		private readonly Dictionary<string, int> _dataI = new Dictionary<string, int>();
		private readonly Dictionary<string, float> _dataF = new Dictionary<string, float>();
		private readonly Dictionary<string, string> _dataS = new Dictionary<string, string>();

		//SetData-Start--------------------------------------
		public int SetData(string key, int data) {
			if(_dataI.ContainsKey(key)) {
				_dataI[key] = data;
			}
			else {
				_dataI.Add(key, data);
				_count[0]++;
			}
			return _count[0];
		}
		public int SetData(string key, float data) {
			if(_dataF.ContainsKey(key)) {
				_dataF[key] = data;
			}
			else {
				_dataF.Add(key, data);
				_count[1]++;
			}
			return _count[1];
		}
		public int SetData(string key, string data) {
			if(_dataS.ContainsKey(key)) {
				_dataS[key] = data;
			}
			else {
				_dataS.Add(key, data);
				_count[2]++;
			}
			return _count[2];
		}

		//GetData-Start--------------------------------------
		public int GetIntData(string key, int defaultValue) {

			var d = defaultValue;

			try {
				d = _dataI[key];
			}
			catch(Exception e) {
				Debug.Log("Key(" + key + ") is not found");
			}
			return d;
		}
		public float GetFloatData(string key, float defaultValue) {

			var d = defaultValue;

			try {
				d = _dataF[key];
			}
			catch(Exception e) {
				Debug.Log("Key(" + key + ") is not found");
			}
			return d;
		}
		public string GetStringData(string key, string defaultValue) {

			var d = defaultValue;

			try {
				d = _dataS[key];
			}
			catch(Exception e) {
				Debug.Log("Key(" + key + ") is not found");
			}
			return d;
		}

		public Dictionary<string, int> GetIntDataAll() {

			return _dataI;
		}
		public Dictionary<string, float> GetFloatDataAll() {

			return _dataF;
		}
		public Dictionary<string, string> GetStringDataAll() {

			return _dataS;
		}

		//System-Start---------------------------------------
		public int GetDataCount(DataKey key) {

			int retC;

			switch(key) {
				case DataKey.All:
					retC = _count[(int)DataKey.Int] + _count[(int)DataKey.Float] + _count[(int)DataKey.String];
					break;
				case DataKey.Int:
					retC = _count[(int)DataKey.Int];
					break;
				case DataKey.Float:
					retC = _count[(int)DataKey.Float];
					break;
				case DataKey.String:
					retC = _count[(int)DataKey.String];
					break;
				default:
					Debug.Log("The value is out of range : Return 0");
					retC = 0;
					break;
			}

			return retC;
		}

		public void SetCount(int[] c) {
			c.CopyTo(_count, 0);
		}
	}

	/// <summary>
	/// セーブする
	/// </summary>
	/// <param name="fileName"></param>
	/// <param name="slot"></param>
	/// <param name="data"></param>
	public static void SaveGameFile(string fileName, int slot, SaveData data) {

		var path = Application.dataPath;//Dynamic
		var name = fileName + slot + ".sav";//SaveSlot
		Debug.Log("[path] " + path + "/" + name);

		var info = new FileInfo(path + "/" + name);

		using(var sw = info.CreateText()) {

			var strData = "";
			foreach(var pair in data.GetIntDataAll()) {

				strData += pair.Key + "|" + pair.Value + ",";

			}
			sw.WriteLine(strData);
			Debug.Log("[int] " + strData);

			strData = "";
			foreach(var pair in data.GetFloatDataAll()) {

				strData += pair.Key + "|" + pair.Value + ",";

			}
			sw.WriteLine(strData);
			Debug.Log("[float] " + strData);

			strData = "";
			foreach(var pair in data.GetStringDataAll()) {

				strData += pair.Key + "|" + pair.Value + ",";

			}
			sw.WriteLine(strData);
			Debug.Log("[string] " + strData);

		}
	}

	/// <summary>
	/// ロードする
	/// </summary>
	/// <param name="fileName"></param>
	/// <param name="slot"></param>
	/// <returns></returns>
	public static SaveData LoadGameFile(string fileName, int slot) {

		var data = new SaveData();

		var path = Application.dataPath;//Dynamic
		var name = fileName + slot + ".sav";//SaveSlot
		Debug.Log("[path] " + path + "/" + name);

		var info = new FileInfo(path + "/" + name);

		if(!File.Exists(path + "/" + name)) return data;

		using(var sr = new StreamReader(info.OpenRead(), Encoding.UTF8)) {

			var target = 0;//int > float > string target
			var c = new int[3];//count

			//int-load--------------------------------------
			var buff = sr.ReadLine();
			Debug.Log("[int] " + buff);
			var dataSplit = DataSplit(buff, ',');
			for(c[target] = 0;c[target] < dataSplit.Length - 1;c[target]++) {

				var dataInt = dataSplit[c[target]].Split('|');
				data.SetData(dataInt[0], int.Parse(dataInt[1]));
			}
			target++;

			//float-load------------------------------------
			buff = sr.ReadLine();
			Debug.Log("[float] " + buff);
			dataSplit = DataSplit(buff, ',');
			for(c[target] = 0;c[target] < dataSplit.Length - 1;c[target]++) {

				var dataFloat = dataSplit[c[target]].Split('|');
				data.SetData(dataFloat[0], float.Parse(dataFloat[1]));
			}
			target++;

			//string-load-----------------------------------
			buff = sr.ReadLine();
			Debug.Log("[string] " + buff);
			dataSplit = DataSplit(buff, ',');
			for(c[target] = 0;c[target] < dataSplit.Length - 1;c[target]++) {

				var dataString = dataSplit[c[target]].Split('|');
				data.SetData(dataString[0], dataString[1]);
			}
			data.SetCount(c);
		}

		return data;
	}

	/// <summary>
	/// データを切り分ける
	/// </summary>
	/// <param name="rawData"></param>
	/// <param name="splitC"></param>
	/// <returns></returns>
	private static string[] DataSplit(string rawData, char splitC) {

		return rawData.Split(splitC);
	}
}
