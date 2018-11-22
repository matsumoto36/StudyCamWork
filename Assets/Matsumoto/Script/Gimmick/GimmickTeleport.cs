using System.Collections;
using UnityEngine;

/// <summary>
/// プレイヤーを転送するギミック
/// </summary>
public class GimmickTeleport : GimmickBase {

	public float WaitTime;

	private Color _playerCol;
	private MouseCamera _mouseCamera;
	private float _playerSpeed;
	private PKFxFX _fromEffect;

	public override void SpawnModel(Player player) {
		MarkModelName = "MarkTeleport";
		base.SpawnModel(player);
	}

	public override void OnRemainingTime(Player player, float t) {
		base.OnRemainingTime(player, t);

		if (!(t + WaitTime < 0.5f) || _fromEffect) return;

		_fromEffect = ParticleManager.Spawn("GimmickTeleportFromEffect", MarkModel.transform.position, Quaternion.identity, 2);
		_fromEffect.GetAttribute("Radius").ValueFloat = player.Body.localScale.x / 2;
	}

	public override void OnAttach(Player player) {
		base.OnAttach(player);

		var render = player.GetComponentInChildren<Renderer>();
		_playerCol = render.material.color;
		render.material.color = GimmickColor;

		_playerSpeed = player.Speed;
		player.Speed = 0;

		_mouseCamera = FindObjectOfType<MouseCamera>();
		_mouseCamera.IsTeleport = true;

		var p = ParticleManager.Spawn(
			"GimmickTeleportToEffect", 
			Path.GetPoint(EndPoint / Path.LineCount),
			Quaternion.identity,
			WaitTime + 2);

		p.GetAttribute("WaitTime").ValueFloat = WaitTime;
		p.GetAttribute("Radius").ValueFloat = player.Body.localScale.x / 2;

		AudioManager.PlaySE("magic-drain1");
		StartCoroutine(PlaySEDelay(WaitTime - 0.1f));
	}

	public override void OnApplyUpdate(Player player, float t) {
		base.OnApplyUpdate(player, t);

		if (!(t > WaitTime - 0.5f) || _fromEffect) return;

		_fromEffect = ParticleManager.Spawn("GimmickTeleportFromEffect", MarkModel.transform.position, Quaternion.identity, 2);
		_fromEffect.GetAttribute("Radius").ValueFloat = player.Body.localScale.x / 2;
	}

	public override void OnDetach(Player player) {
		base.OnDetach(player);

		var render = player.GetComponentInChildren<Renderer>();
		render.material.color = _playerCol;

		player.MovedLength += Path.GetPointLength(StartPoint, EndPoint);
		player.Speed = _playerSpeed;

		_mouseCamera.IsTeleport = false;
	}

	public override float GetSectionTime(float speed) {
		return WaitTime;
	}

	public override void EditGimmickLine(LineRenderer lineRenderer, ref float z) {
		lineRenderer.material.SetColor("_Color", GimmickColor);
		base.EditGimmickLine(lineRenderer, ref z);
	}

	private static IEnumerator PlaySEDelay(float t) {
		yield return new WaitForSeconds(t);
		AudioManager.PlaySE("magic-gravity1");
	}
}
