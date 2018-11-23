using System;

[Serializable]
public class StageInfo {

	public string PathName;
	public string StudioName;
	public int WindowIndex;

	public StageInfo(string pathName, string studioName, int windowIndex) {
		PathName = pathName;
		StudioName = studioName;
		WindowIndex = windowIndex;
	}
}