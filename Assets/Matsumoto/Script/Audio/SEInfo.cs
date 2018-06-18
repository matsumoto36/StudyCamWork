using UnityEngine;
using System.Collections;

public class SEInfo {

	public int index;
    public float curTime;
    public float volume;

	public SEInfo(int index, float curTime, float volume) {
		this.index = index;
		this.curTime = curTime;
		this.volume = volume;
	}
}
