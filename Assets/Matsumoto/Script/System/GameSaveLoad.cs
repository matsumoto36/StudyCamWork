using UnityEngine;
using System.IO;
using System.Collections;
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

		public enum DataKey {ALL = -1, INT = 0, FLOAT, STRING}

		int[] count = new int[3];// 0:int 1:float 2:string

		//DataSet(int, float, string)
		Dictionary<string, int> dataI = new Dictionary<string, int>();
		Dictionary<string, float> dataF = new Dictionary<string, float>();
		Dictionary<string, string> dataS = new Dictionary<string, string>();

		//SetData-Start--------------------------------------
		public int AddData(string key, int data) {
			dataI.Add(key, data);
			return ++count[0];
		}
		public int AddData(string key, float data) {

			dataF.Add(key, data);
			return ++count[1];
		}
		public int AddData(string key, string data) {

			dataS.Add(key, data);
			return ++count[2];
		}

		public void AddDataAll(Dictionary<string, int> dI, Dictionary<string, float> dF, Dictionary<string, string> dS) {

			dataI = dI;
			dataF = dF;
			dataS = dS;
		}

		//GetData-Start--------------------------------------
		public int GetIntData(string key, int defaultValue) {

			int d = defaultValue;

			try {
				d = dataI[key];
			}
			catch(Exception e){
				Debug.Log("Key(" + key + ") is not found");
			}
			return d;
		}
		public float GetFloatData(string key, float defaultValue) {

			float d = defaultValue;

			try {
				d = dataF[key];
			}
			catch(Exception e) {
				Debug.Log("Key(" + key + ") is not found");
			}
			return d;
		}
		public string GetSrtingData(string key, string defaultValue) {

			string d = defaultValue;

			try {
				d = dataS[key];
			}
			catch(Exception e) {
				Debug.Log("Key(" + key + ") is not found");
			}
			return d;
		}

		public Dictionary<string, int> GetIntDataAll() {

			return dataI;
		}
		public Dictionary<string, float> GetFloatDataAll() {

			return dataF;
		}
		public Dictionary<string, string> GetStringDataAll() {

			return dataS;
		}

		//System-Start---------------------------------------
		public int GetDataCount(DataKey key) {

			int retC;

			switch(key) {
				case DataKey.ALL:
					retC = count[(int)DataKey.INT] + count[(int)DataKey.FLOAT] + count[(int)DataKey.STRING];
					break;
				case DataKey.INT:
					retC = count[(int)DataKey.INT];
					break;
				case DataKey.FLOAT:
					retC = count[(int)DataKey.FLOAT];
					break;
				case DataKey.STRING:
					retC = count[(int)DataKey.STRING];
					break;
				default :
					Debug.Log("The value is out of range : Return 0");
					retC = 0;
					break;
			}

			return retC;
		}

		public void SetCount(int[] c) {
			c.CopyTo(count, 0);
		}
	}

	/// <summary>
	/// セーブする
	/// </summary>
	/// <param name="fileName"></param>
	/// <param name="slot"></param>
	/// <param name="data"></param>
	public static void SaveGameFile(string fileName, int slot, SaveData data) {

		string path = Application.dataPath;//Dynamic
		string name = fileName + slot + ".sav";//SaveSlot
		Debug.Log("[path] " + path + "/" + name);

		FileInfo info = new FileInfo(path + "/" + name);

		using(StreamWriter sw = info.CreateText()) {

			string strData = "";
			foreach(KeyValuePair<string, int> pair in data.GetIntDataAll()) {

				strData += pair.Key + "|" + pair.Value + ",";
				
			}
			sw.WriteLine(strData);
			Debug.Log("[int] " + strData);

			strData = "";
			foreach(KeyValuePair<string, float> pair in data.GetFloatDataAll()) {

				strData += pair.Key + "|" + pair.Value + ",";

			}
			sw.WriteLine(strData);
			Debug.Log("[float] " + strData);

			strData = "";
			foreach(KeyValuePair<string, string> pair in data.GetStringDataAll()) {

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

		SaveData data = new SaveData();

		string path = Application.dataPath;//Dynamic
		string name = fileName + slot + ".sav";//SaveSlot
		Debug.Log("[path] " + path + "/" + name);

		FileInfo info = new FileInfo(path + "/" + name);

		if(!File.Exists(fileName)) return data;

		using(StreamReader sr = new StreamReader(info.OpenRead(), Encoding.UTF8)) {

			int target = 0;//int > float > string target
			int[] c = new int[3];//count
			string buff = "";//buffer

			//int-load--------------------------------------
			buff = sr.ReadLine();
			Debug.Log("[int] " + buff);
			string[] spBaff = DataSplit(buff, ',');
			for(c[target] = 0;c[target] < spBaff.Length - 1;c[target]++) {

				string[] dataInt = spBaff[c[target]].Split('|');
				data.AddData(dataInt[0], int.Parse(dataInt[1]));
			}
			target++;

			//float-load------------------------------------
			buff = sr.ReadLine();
			Debug.Log("[float] " + buff);
			 spBaff = DataSplit(buff, ',');
			for(c[target] = 0;c[target] < spBaff.Length - 1;c[target]++) {

					string[] dataFloat = spBaff[c[target]].Split('|');
					data.AddData(dataFloat[0], float.Parse(dataFloat[1]));
			}
			target++;

			//string-load-----------------------------------
			buff = sr.ReadLine();
			Debug.Log("[string] " + buff);
			spBaff = DataSplit(buff, ',');
			for(c[target] = 0;c[target] < spBaff.Length - 1;c[target]++) {

				string[] dataString = spBaff[c[target]].Split('|');
				data.AddData(dataString[0], dataString[1]);
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
	static string[] DataSplit(string rawData, char splitC) {

		return rawData.Split(splitC);
	}
}
