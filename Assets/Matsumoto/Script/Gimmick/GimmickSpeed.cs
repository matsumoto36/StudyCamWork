using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GimmickSpeed : GimmickBase {

	public Text text;
	public float speedMul;

	Color playerCol;

	float duration;

	public override void SpawnModel(Player player) {

		markModelName = "MarkChangeSpeed";

		base.SpawnModel(player);
	}

	public override void OnRemainingTime(Player player, float t) {
		base.OnRemainingTime(player, t);

		if(text)
			text.text = "At. " + t;
	}

	public override void OnAttach(Player player) {
		base.OnAttach(player);

		var render = player.GetComponentInChildren<Renderer>();
		playerCol = render.material.color;
		render.material.color = gimmickColor;

		player.Speed *= speedMul;
	}

	public override void OnApplyUpdate(Player player, float t) {
		base.OnApplyUpdate(player, t);

		if(text)
			text.text = "Using. " + t;
	}

	public override void OnDetach(Player player) {
		base.OnDetach(player);

		var render = player.GetComponentInChildren<Renderer>();
		render.material.color = playerCol;

		player.Speed /= speedMul;
	}

	public override float GetSectionTime(float speed) {
		return duration = path.GetPointLength(startPoint, endPoint) / speed / speedMul;
	}

	public override void EditGimmickLine(LineRenderer lineRenderer, ref float z) {

		lineRenderer.material.SetColor("_Color", gimmickColor);

		var partition = (int)(32 * (endPoint - startPoint));
		if(partition == 0) partition = 1;

		var dt = (endPoint - startPoint) * (1.0f / partition);
		var point = new Vector3[partition + 1];

		for(int i = 0;i <= partition;i++) {
			point[i] = path.GetPoint((startPoint + dt * i) / path.LineCount);
			point[i].z = z;
		}

		lineRenderer.positionCount = point.Length;
		lineRenderer.SetPositions(point);

		markModelSpawnZ = z;
	}
}
