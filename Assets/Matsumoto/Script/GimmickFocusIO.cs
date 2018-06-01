using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//プレイヤーを奥や手前に移動させる
public class GimmickFocusIO : GimmickBase {

	float moveZ;
	public bool isToFar = true;
	public Text text;

	GimmickGauge startGauge;
	float playerSpeed;

	void Start() {

	}

	public override void OnRemainingTime(Player player, float t) {
		base.OnRemainingTime(player, t);

		if(text)
			text.text = "At. " + t;

		if(!startPointModel) return;
		if(!startGauge) {
			startGauge = startPointModel.GetComponent<GimmickGauge>();
			if(!startGauge) return;
		}

		startGauge.Value = t;
	}

	public override void OnAttach(Player player) {
		base.OnAttach(player);

		playerSpeed = player.speed;

		var baseLength = path.GetPointLength(startPoint, endPoint);
		var duration = GameMaster.gameMaster.gameBalanceData.FocusDuration;
		player.speed = baseLength / duration;
	}

	public override void OnApplyUpdate(Player player, float t) {
		base.OnApplyUpdate(player, t);

		//プレイヤーのbodyのZを変更
		var duration = GameMaster.gameMaster.gameBalanceData.FocusDuration;
		var ratio = t / duration;
		if(!isToFar) ratio = 1 - ratio;
		var body = player.transform.GetChild(0);
		body.localPosition = new Vector3(0, 0, ratio * moveZ);

		if(text)
			text.text = "Using. " + t;

		//ゲージに反映
		if(!startPointModel) return;
		if(!startGauge) {
			startGauge = startPointModel.GetComponent<GimmickGauge>();
			if(!startGauge) return;
		}
		startGauge.Value = t / duration;
	}

	public override void OnDetach(Player player) {
		base.OnDetach(player);

		player.speed = playerSpeed;
	}

	public override float GetSectionTime(float speed) {
		return GameMaster.gameMaster.gameBalanceData.FocusDuration;
	}

	public override void EditGimmickLine(LineRenderer lineRenderer, ref float z) {

		lineRenderer.startColor = gimmickColor;
		lineRenderer.endColor = gimmickColor;

		moveZ = manager.moveZ;
		startPointModelSpawnZ = z;
		Debug.Log(z);

		var partition = (int)(32 * (endPoint - startPoint));
		var diff = endPoint - startPoint;
		var dt =  partition == 0 ? 0 : 1.0f / partition;
		var point = new Vector3[partition + 1];

		for(int i = 0;i <= partition;i++) {

			if(path) Debug.Log("aa");

			point[i] = path.GetPoint((startPoint + diff * dt * i) / path.LineCount);
			point[i].z = moveZ * dt * i;

			if(!isToFar) {
				point[i].z = moveZ - point[i].z;
			}
		}

		lineRenderer.positionCount = point.Length;
		lineRenderer.SetPositions(point);

		if(isToFar) {
			z = moveZ;
		}
		else {
			z = 0;
		}

		endPointModelSpawnZ = z;
		Debug.Log(z);


	}
}
