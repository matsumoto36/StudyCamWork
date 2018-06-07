using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GimmickTest : GimmickBase {

	public Text text;
	Color playerCol;

	GimmickGauge startGauge;

	float duration;

	public override void SpawnModel() {
		base.SpawnModel();


		if(!startPointModel) return;
		if(!startGauge) {
			startGauge = startPointModel.GetComponent<GimmickGauge>();
			if(!startGauge) return;
		}

		startGauge.GaugeColor = gimmickColor;
	}

	public override void OnRemainingTime(Player player, float t) {
		base.OnRemainingTime(player, t);

		if(text)
			text.text = "At. " + t;

		if(!startGauge) return;
		startGauge.Value = 1 - Mathf.Min(t, 1);
	}

	public override void OnAttach(Player player) {
		base.OnAttach(player);

		var render = player.GetComponentInChildren<Renderer>();
		playerCol = render.material.color;
		render.material.color = gimmickColor;

		player.speed *= 5;
	}

	public override void OnApplyUpdate(Player player, float t) {
		base.OnApplyUpdate(player, t);

		if(text)
			text.text = "Using. " + t;

		if(!startGauge) return;
		startGauge.Value = 1 - (t / duration);
	}

	public override void OnDetach(Player player) {
		base.OnDetach(player);

		var render = player.GetComponentInChildren<Renderer>();
		render.material.color = playerCol;

		player.speed /= 5;
	}

	public override float GetSectionTime(float speed) {
		return duration = path.GetPointLength(startPoint, endPoint) / speed / 5;
	}

	public override void EditGimmickLine(LineRenderer lineRenderer, ref float z) {

		lineRenderer.material.SetColor("_Color", gimmickColor);

		var partition = (int)(32 * (endPoint - startPoint));
		var dt = (endPoint - startPoint) * (1.0f / partition);
		var point = new Vector3[partition + 1];

		for(int i = 0;i <= partition;i++) {
			point[i] = path.GetPoint((startPoint + dt * i) / path.LineCount);
			point[i].z = z;
		}

		lineRenderer.positionCount = point.Length;
		lineRenderer.SetPositions(point);

		endPointModelSpawnZ = startPointModelSpawnZ = z;

	}
}
