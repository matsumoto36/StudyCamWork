﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ギミック全体を管理するクラス
/// ギミックを追加する場合はこのクラスの子オブジェクトにすること
/// </summary>
public class GimmickManager : MonoBehaviour {

	GimmickInfo[] gimmicks;
	Player player;
	float startTime;

	void Awake() {

		player = GameObject.FindGameObjectWithTag("Player")
			.GetComponent<Player>();

		var g = GetComponentsInChildren<GimmickBase>();
		gimmicks = new GimmickInfo[g.Length];

		for(int i = 0;i < g.Length;i++) {
			gimmicks[i] = new GimmickInfo(g[i]);
		}

		SetStartTime();
		startTime = Time.time;
	}

	// Update is called once per frame
	void Update () {
		GimmickUpdate();
	}

	void GimmickUpdate() {

		foreach(var item in gimmicks) {

			if(item.isUsed) continue;

			var gimmickTime = Time.time - startTime;

			//発動前
			if(!item.isActive && item.waitTime > gimmickTime) {

				item.gimmick.OnRemainingTime(player, item.waitTime - gimmickTime);
			}

			//発動した瞬間
			if(!item.isActive && item.waitTime < gimmickTime) {

				item.isActive = true;
				item.gimmick.OnAttach(player);
			}

			//発動中
			if(item.isActive) {

				item.gimmick.OnApplyUpdate(player, gimmickTime - item.waitTime);
			}

			//発動が終了した瞬間
			if(item.isActive && item.waitTime + item.duration < gimmickTime) {
				item.isActive = false;
				item.isUsed = true;
				item.gimmick.OnDetach(player);
			}
		}
	}

	void SetStartTime() {

		var speed = player.speed;

		//昇順にソート
		gimmicks = gimmicks
			.OrderBy((item) => item.gimmick.startPoint)
			.ToArray();

		var sumTime = 0.0f;
		var prevPoint = 0.0f;
		for(int i = 0;i < gimmicks.Length;i++) {

			var gimmick = gimmicks[i].gimmick;
			var path = gimmick.targetPath;
			var pathLength = path.GetLength();

			//通常ゾーン
			var t = path.GetPointLength(prevPoint, gimmick.startPoint) / speed;

			//ギミックのゾーン
			sumTime += t;
			gimmicks[i].waitTime = sumTime;
			gimmicks[i].duration = gimmick.GetSectionTime(speed);
			sumTime += gimmicks[i].duration;
			prevPoint = gimmick.endPoint;
		}
	}
}

/// <summary>
/// ギミックの情報をまとめておくクラス
/// </summary>
class GimmickInfo {

	public GimmickBase gimmick;
	public bool isActive;
	public bool isUsed;
	public float waitTime;
	public float duration;

	public GimmickInfo(GimmickBase gimmick) {
		this.gimmick = gimmick;
	}
}