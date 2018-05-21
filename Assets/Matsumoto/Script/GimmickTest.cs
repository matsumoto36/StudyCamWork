using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GimmickTest : GimmickBase {

	public Text text;
	Color playerCol;


	public override void OnRemainingTime(Player player, float t) {
		base.OnRemainingTime(player, t);

		text.text = "At. " + t;
	}

	public override void OnAttach(Player player) {
		base.OnAttach(player);

		var render = player.GetComponent<Renderer>();
		playerCol = render.material.color;
		render.material.color = gimmickColor;
	}

	public override void OnApplyUpdate(Player player, float t) {
		base.OnApplyUpdate(player, t);

		text.text = "Using. " + t;
	}

	public override void OnDetach(Player player) {
		base.OnDetach(player);

		var render = player.GetComponent<Renderer>();
		render.material.color = playerCol;
	}


	public override float GetSectionTime(float speed) {
		return targetPath.GetPointLength(startPoint, endPoint) / speed;
	}

	public override Vector2 GetPlayerPosition(float t) {
		return base.GetPlayerPosition(t);
	}
}
