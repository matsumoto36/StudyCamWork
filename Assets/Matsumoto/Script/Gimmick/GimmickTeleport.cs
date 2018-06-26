using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GimmickTeleport : GimmickBase {

	public GameObject effectPre;
	GameObject effect;

	public float waitTime;
	public Text text;
	Color playerCol;

	MouseCamera mouseCamera;

	float playerSpeed;

	PKFxFX fromEffect;

	public override void SpawnModel(Player player) {

		markModelName = "MarkTeleport";

		base.SpawnModel(player);

	}

	public override void OnRemainingTime(Player player, float t) {
		base.OnRemainingTime(player, t);

		if(text)
			text.text = "At. " + t;

		if(t + waitTime < 0.5f && !fromEffect) {
			fromEffect = ParticleManager.Spawn("GimmickTeleportFromEffect", markModel.transform.position, Quaternion.identity, 2);
			fromEffect.GetAttribute("Radius").ValueFloat = player.Body.localScale.x / 2;
		}
	}

	public override void OnAttach(Player player) {
		base.OnAttach(player);

		var render = player.GetComponentInChildren<Renderer>();
		playerCol = render.material.color;
		render.material.color = gimmickColor;

		playerSpeed = player.Speed;
		player.Speed = 0;

		var pos = path.GetPoint(endPoint / path.LineCount);
		effect = Instantiate(effectPre, pos, Quaternion.identity);

		mouseCamera = FindObjectOfType<MouseCamera>();
		mouseCamera.IsTeleport = true;

		var p = ParticleManager.Spawn(
			"GimmickTeleportToEffect", 
			path.GetPoint(endPoint / path.LineCount),
			Quaternion.identity,
			waitTime + 2);

		p.GetAttribute("WaitTime").ValueFloat = waitTime;
		p.GetAttribute("Radius").ValueFloat = player.Body.localScale.x / 2;

		AudioManager.PlaySE("magic-drain1");
		StartCoroutine(PlaySEDelay(waitTime - 0.1f));
	}

	public override void OnApplyUpdate(Player player, float t) {
		base.OnApplyUpdate(player, t);

		if(t > waitTime - 0.5f && !fromEffect) {
			fromEffect = ParticleManager.Spawn("GimmickTeleportFromEffect", markModel.transform.position, Quaternion.identity, 2);
			fromEffect.GetAttribute("Radius").ValueFloat = player.Body.localScale.x / 2;
		}
	}

	public override void OnDetach(Player player) {
		base.OnDetach(player);

		var render = player.GetComponentInChildren<Renderer>();
		render.material.color = playerCol;

		player.MovedLength += path.GetPointLength(startPoint, endPoint);
		player.Speed = playerSpeed;

		effect.GetComponent<ParticleSystem>().Stop();
		Destroy(effect, 1);

		mouseCamera.IsTeleport = false;
	}

	public override float GetSectionTime(float speed) {
		return waitTime;
	}

	public override void EditGimmickLine(LineRenderer lineRenderer, ref float z) {

		lineRenderer.material.SetColor("_Color", gimmickColor);
		base.EditGimmickLine(lineRenderer, ref z);
	}

	IEnumerator PlaySEDelay(float t) {
		yield return new WaitForSeconds(t);
		AudioManager.PlaySE("magic-gravity1");
	}
}
