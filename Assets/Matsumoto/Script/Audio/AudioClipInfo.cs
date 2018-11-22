using System;
using System.Collections.Generic;
using UnityEngine;

class AudioClipInfo {

	public const int MaxPlayCount = 50;
	private const float DefaultVolume = 0.2f;

	public AudioClip Clip;
	public SortedList<int, SEInfo> StockList = new SortedList<int, SEInfo>();

	public float Attenuate;   // 合成時減衰率

	public AudioClipInfo(AudioClip clip) {
		Clip = clip;
		Attenuate = CalcAttenuateRate();

		// create stock list
		for(var i = 0;i < MaxPlayCount;i++) {
			var seInfo = new SEInfo(i, 0.0f, Mathf.Pow(Attenuate, i));
			StockList.Add(seInfo.Index, seInfo);
		}
	}

	/// <summary>
	/// 音量の減衰率を求める
	/// </summary>
	/// <returns></returns>
	private static float CalcAttenuateRate() {
		return NewtonMethod(p => (1.0f - Mathf.Pow(p, MaxPlayCount)) / (1.0f - p) - 1.0f / DefaultVolume,
			p => {
				var ip = 1.0f - p;
				var t0 = -MaxPlayCount * Mathf.Pow(p, MaxPlayCount - 1.0f) / ip;
				var t1 = (1.0f - Mathf.Pow(p, MaxPlayCount)) / ip / ip;
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
	private static float NewtonMethod(Func<float, float> func, Func<float, float> derive, float initX, int maxLoop) {
		var x = initX;
		for(var i = 0;i < maxLoop;i++) {
			var curY = func(x);
			if(curY < 0.00001f && curY > -0.00001f)
				break;
			x = x - curY / derive(x);
		}
		return x;
	}
}
