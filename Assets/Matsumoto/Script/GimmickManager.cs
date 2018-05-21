using System.Linq;
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

	void Awake() {

		player = GameObject.FindGameObjectWithTag("Player")
			.GetComponent<Player>();

		var g = GetComponentsInChildren<GimmickBase>();
		gimmicks = new GimmickInfo[g.Length];

		for(int i = 0;i < g.Length;i++) {
			gimmicks[i] = new GimmickInfo(g[i]);
		}

		SetStartTime();
	}

	// Update is called once per frame
	void Update () {
		GimmickUpdate();
	}

	void GimmickUpdate() {

		foreach(var item in gimmicks) {

			if(item.isUsed) continue;

			var length = player.MovedLength / player.speed;

			//発動前
			if(!item.isActive && item.waitTime > length) {

				item.gimmick.OnRemainingTime(player, item.waitTime - length);
			}

			//発動した瞬間
			if(!item.isActive && item.waitTime < length) {

				item.isActive = true;
				item.gimmick.OnAttach(player);
			}

			//発動中
			if(item.isActive) {

				item.gimmick.OnApplyUpdate(player, length - item.waitTime);
			}

			//発動が終了した瞬間
			if(item.isActive && item.waitTime + item.gimmick.GetSectionTime(player.speed) < length) {
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

			//通常ゾーン *ここがわからない
			var t = path.GetPointLength(prevPoint, gimmick.startPoint) / (speed);

			//ギミックのゾーン
			sumTime += t;
			gimmicks[i].waitTime = sumTime;
			//Debug.Log(gimmicks[i].waitTime);

			sumTime += gimmick.GetSectionTime(speed);
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

	public GimmickInfo(GimmickBase gimmick) {
		this.gimmick = gimmick;
	}
}