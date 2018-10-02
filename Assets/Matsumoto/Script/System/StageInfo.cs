using System;

[Serializable]
public class StageInfo {
	public string pathName;
	public string studioName;
	public int windowIndex;

	public StageInfo(string pathName, string studioName, int windowIndex) {
		this.pathName = pathName;
		this.studioName = studioName;
		this.windowIndex = windowIndex;
	}
}