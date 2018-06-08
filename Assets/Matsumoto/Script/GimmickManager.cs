﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ギミック全体を管理するクラス
/// ギミックを追加する場合はこのクラスの子オブジェクトにすること
/// </summary>
public class GimmickManager : MonoBehaviour {

	public Bezier2D path;
	public LineRenderer linePre;
	public float moveZ = 18;

	GimmickInfo[] gimmicks;
	public List<LineInfo> lines;

	Player player;
	float startTime;


	//debug
	public float _playerPoint;

	public void Init() {

		if(!path) return;

		player = GameObject.FindGameObjectWithTag("Player")
			.GetComponent<Player>();

		var g = GetComponentsInChildren<GimmickBase>();
		gimmicks = new GimmickInfo[g.Length];

		for(int i = 0;i < g.Length;i++) {
			gimmicks[i] = new GimmickInfo(g[i]);
			g[i].Init();
		}

		//昇順にソート
		gimmicks = gimmicks
			.OrderBy((item) => item.gimmick.startPoint)
			.ToArray();

		SetStartTime();
		SetLine();

		foreach(var item in gimmicks) {
			item.gimmick.SpawnModel();
		}

		GameMaster.gameMaster.OnGameStart += () => startTime = Time.time;
	}

	public void GimmickUpdate() {

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

		//線の可視状態を設定
		SetLineVisible();
	}

	/// <summary>
	/// プレイヤーの位置で線の可視状態を設定
	/// </summary>
	void SetLineVisible() {

		var playerRatio = player.MovedLength / path.Length;
		_playerPoint = playerRatio;

		foreach(var item in lines) {
			if(!item.renderer) continue;

			var fillValue = 1.0f;
			var startRatio = path.GetPointLength(0, item.pathStartPoint) / path.Length;
			var endRatio = path.GetPointLength(0, item.pathEndPoint) / path.Length;

			//通り過ぎた線の場合
			if(playerRatio >= endRatio) {
				fillValue = 0.0f;
			}
			//通っている途中の線の場合
			else if(playerRatio > startRatio) {
				var t = playerRatio - startRatio;
				fillValue = 1 - t / (endRatio - startRatio);
			}
			//可視範囲を設定
			item.renderer.material.SetFloat("_Fill", fillValue);
		}

	}

	/// <summary>
	/// 開始時間をあらかじめ計算する
	/// </summary>
	void SetStartTime() {

		var speed = player.speed;
		var sumTime = 0.0f;
		var prevPoint = 0.0f;
		for(int i = 0;i < gimmicks.Length;i++) {

			var gimmick = gimmicks[i].gimmick;

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

	/// <summary>
	/// LineRendererを各ポイントに設置
	/// </summary>
	void SetLine() {

		//普通に引く線はあらかじめメソッドを作っておく
		Func<Bezier2D, float, float, float, LineRenderer> DrawLine = (path, from, to, lineZ) => {
			var diff = to - from;
			if(diff > 0) {

				var partition = (int)(32 * diff);
				var dt = diff / partition;
				var point = new Vector3[partition + 1];

				for(int i = 0;i <= partition;i++) {
					point[i] = path.GetPoint((from + dt * i) / path.LineCount);
					point[i].z = lineZ;
				}

				var l = Instantiate(linePre);
				l.material = new Material(l.material);
				l.positionCount = point.Length;
				l.SetPositions(point);
				return l;
			}

			return null;
		};

		lines = new List<LineInfo>();
		var prevPoint = 0.0f;
		var z = 0.0f;
		for(int i = 0;i < gimmicks.Length;i++) {

			var gimmick = gimmicks[i].gimmick;

			//通常ゾーン
			lines.Add(new LineInfo(
				DrawLine(path, prevPoint, gimmick.startPoint, z),
				prevPoint,
				gimmick.startPoint
			 ));

			//ギミックゾーン
			var gimmickLine = Instantiate(linePre);
			gimmickLine.material = new Material(gimmickLine.material);
			gimmick.EditGimmickLine(gimmickLine, ref z);

			lines.Add(new LineInfo(
				gimmickLine,
				gimmick.startPoint,
				gimmick.endPoint
			));

			prevPoint = gimmick.endPoint;
		}

		//最後の線を引く
		lines.Add(new LineInfo(
			DrawLine(path, prevPoint, path.LineCount, z),
			prevPoint,
			path.LineCount
		));

	}
}

/// <summary>
/// 線の情報をまとめておくクラス
/// </summary>
[Serializable]
public class LineInfo {

	public LineRenderer renderer;
	public float pathStartPoint;
	public float pathEndPoint;

	public LineInfo(LineRenderer renderer, float pathStartPoint, float pathEndPoint) {
		this.renderer = renderer;
		this.pathStartPoint = pathStartPoint;
		this.pathEndPoint = pathEndPoint;
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