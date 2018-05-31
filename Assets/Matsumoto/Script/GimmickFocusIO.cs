using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//プレイヤーを奥や手前に移動させる
public class GimmickFocusIO : GimmickBase {

	const float moveZ = 18;
	public float duration = 1;

	public bool isToFar = true;

	public Text text;

	float playerSpeed;

	public override void OnRemainingTime(Player player, float t) {
		base.OnRemainingTime(player, t);

		if(text)
			text.text = "At. " + t;
	}

	public override void OnAttach(Player player) {
		base.OnAttach(player);

		playerSpeed = player.speed;

		var baseLength = path.GetPointLength(startPoint, endPoint);
		var hypotenuseLength = Mathf.Sqrt(moveZ * moveZ + baseLength * baseLength);

		player.speed = baseLength / duration;
	}

	public override void OnApplyUpdate(Player player, float t) {
		base.OnApplyUpdate(player, t);

		var ratio = t / duration;
		if(!isToFar) ratio = 1 - ratio;
		player.transform.GetChild(0).localPosition = new Vector3(0, 0, ratio * moveZ);

		if(text)
			text.text = "Using. " + t;
	}

	public override void OnDetach(Player player) {
		base.OnDetach(player);

		player.speed = playerSpeed;
	}

	public override float GetSectionTime(float speed) {
		return duration;
	}

	public override void EditGimmickLine(LineRenderer lineRenderer, ref float z) {

		lineRenderer.startColor = gimmickColor;
		lineRenderer.endColor = gimmickColor;

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
