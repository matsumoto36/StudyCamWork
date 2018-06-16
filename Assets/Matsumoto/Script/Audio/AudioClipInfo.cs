using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class AudioClipInfo {

	public const int MAX_PLAY_COUNT = 50;
	const float INIT_VOLUME = 0.2f;

	public AudioClip clip;
	public SortedList<int, SEInfo> stockList = new SortedList<int, SEInfo>();

	public float attenuate = 0.0f;   // 合成時減衰率

	public AudioClipInfo(AudioClip clip) {
		this.clip = clip;
		attenuate = CalcAttenuateRate();
		// create stock list
		for(int i = 0;i < MAX_PLAY_COUNT;i++) {
			SEInfo seInfo = new SEInfo(i, 0.0f, Mathf.Pow(attenuate, i));
			stockList.Add(seInfo.index, seInfo);
		}
	}

	float CalcAttenuateRate() {
		return NewtonMethod((p) => {
				return (1.0f - Mathf.Pow(p, MAX_PLAY_COUNT)) / (1.0f - p) - 1.0f / INIT_VOLUME;
			},
			(p) => {
				float ip = 1.0f - p;
				float t0 = -MAX_PLAY_COUNT * Mathf.Pow(p, MAX_PLAY_COUNT - 1.0f) / ip;
				float t1 = (1.0f - Mathf.Pow(p, MAX_PLAY_COUNT)) / ip / ip;
				return t0 + t1;
			},
			0.9f, 100
		);
	}

	/// <summary>
	/// ニュートン法で方程式の解を求める
	/// </summary>
	/// <param name="func"></param>
	/// <param name="derive"></param>
	/// <param name="initX"></param>
	/// <param name="maxLoop"></param>
	/// <returns></returns>
	static float NewtonMethod(Func<float, float> func, Func<float, float> derive, float initX, int maxLoop) {
		float x = initX;
		for(int i = 0;i < maxLoop;i++) {
			float curY = func(x);
			if(curY < 0.00001f && curY > -0.00001f)
				break;
			x = x - curY / derive(x);
		}
		return x;
	}
}
