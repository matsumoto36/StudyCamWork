using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GimmickSpeed : GimmickBase {

	public Text text;
	public float speedMul;

	Color playerCol;

	PKFxFX particle;
	Vector3 prevPlayerPos;

	public override void SpawnModel(Player player) {

		if(speedMul >= 1) {
			markModelName = "MarkSpeedUp";
		}
		else {
			markModelName = "MarkSpeedDown";
		}

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

		//加速のとき
		if(speedMul > 1) {
			particle = ParticleManager.Spawn("GimmickSpeedUpEffect", new Vector3(), Quaternion.identity, 0);
			particle.transform.SetParent(player.Body);
			particle.transform.localPosition = new Vector3();
			prevPlayerPos = player.transform.position;

			AudioManager.PlaySE("SpeedUP");
		}
		else {

		}
	}

	public override void OnApplyUpdate(Player player, float t) {
		base.OnApplyUpdate(player, t);

		if(text)
			text.text = "Using. " + t;

		if(particle) {

			var pos = player.transform.position;
			var vec = (pos - prevPlayerPos).normalized;

			particle.GetAttribute("AccelDirection").ValueFloat3
				= -vec;

			particle.transform.localPosition = vec * (player.Body.localScale.x / 2);

			prevPlayerPos = pos;

		}
	}

	public override void OnDetach(Player player) {
		base.OnDetach(player);

		var render = player.GetComponentInChildren<Renderer>();
		render.material.color = playerCol;

		player.Speed /= speedMul;

		if(particle) ParticleManager.StopParticle(particle);
	}

	public override float GetSectionTime(float speed) {
		return path.GetPointLength(startPoint, endPoint) / speed / speedMul;
	}

	public override void EditGimmickLine(LineRenderer lineRenderer, ref float z) {

		lineRenderer.material.SetColor("_Color", gimmickColor);
		base.EditGimmickLine(lineRenderer, ref z);
	}
}
