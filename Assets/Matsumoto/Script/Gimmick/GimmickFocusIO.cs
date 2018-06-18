using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//プレイヤーを奥や手前に移動させる
public class GimmickFocusIO : GimmickBase {

	float moveZ;
	public bool isToFar = true;
	public Text text;

	float playerSpeed;

	public override void SpawnModel(Player player) {

		markModelName = "MarkFocusIO";

		base.SpawnModel(player);
	}

	public override void OnRemainingTime(Player player, float t) {
		base.OnRemainingTime(player, t);

		if(text)
			text.text = "At. " + t;

	}

	public override void OnAttach(Player player) {
		base.OnAttach(player);

		playerSpeed = player.Speed;

		var baseLength = path.GetPointLength(startPoint, endPoint);
		var duration = GameMaster.Instance.GameBalanceData.FocusDuration;
		player.Speed = baseLength / duration;
	}

	public override void OnApplyUpdate(Player player, float t) {
		base.OnApplyUpdate(player, t);

		//プレイヤーのbodyのZを変更
		var duration = GameMaster.Instance.GameBalanceData.FocusDuration;
		var ratio = t / duration;
		if(!isToFar) ratio = 1 - ratio;
		var body = player.transform.GetChild(0);
		body.localPosition = new Vector3(0, 0, ratio * moveZ);

		if(text)
			text.text = "Using. " + t;
	}

	public override void OnDetach(Player player) {
		base.OnDetach(player);

		var z = isToFar ? moveZ : 0;
		player.transform.GetChild(0)
			.localPosition = new Vector3(0, 0, z);

		player.Speed = playerSpeed;
	}

	public override float GetSectionTime(float speed) {
		return GameMaster.Instance.GameBalanceData.FocusDuration;
	}

	public override void EditGimmickLine(LineRenderer lineRenderer, ref float z) {

		lineRenderer.material.SetColor("_Color", gimmickColor);

		moveZ = manager.moveZ;
		markModelSpawnZ = z;

		var partition = (int)(32 * (endPoint - startPoint));
		if(partition == 0) partition = 1;

		var diff = endPoint - startPoint;
		var dt =  partition == 0 ? 0 : 1.0f / partition;
		var point = new Vector3[partition + 1];

		for(int i = 0;i <= partition;i++) {

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
	}
}
