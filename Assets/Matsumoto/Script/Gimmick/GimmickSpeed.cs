using UnityEngine;

/// <summary>
/// プレイヤーの速さを変更するギミック
/// </summary>
public class GimmickSpeed : GimmickBase {

	public float SpeedMul;

	private Color _playerCol;
	private PKFxFX _particle;
	private Vector3 _prevPlayerPos;

	public override void SpawnModel(Player player) {

		MarkModelName = SpeedMul >= 1 ? "MarkSpeedUp" : "MarkSpeedDown";
		base.SpawnModel(player);
	}

	public override void OnAttach(Player player) {
		base.OnAttach(player);

		var render = player.GetComponentInChildren<Renderer>();
		_playerCol = render.material.color;
		render.material.color = GimmickColor;

		player.Speed *= SpeedMul;

		if (!(SpeedMul > 1)) return;

		//加速のとき
		_particle = ParticleManager.Spawn("GimmickSpeedUpEffect", new Vector3(), Quaternion.identity, 0);
		_particle.transform.SetParent(player.Body);
		_particle.transform.localPosition = new Vector3();
		_prevPlayerPos = player.transform.position;

		AudioManager.PlaySE("SpeedUP");
	}

	public override void OnApplyUpdate(Player player, float t) {
		base.OnApplyUpdate(player, t);

		if (!_particle) return;

		var pos = player.transform.position;
		var vec = (pos - _prevPlayerPos).normalized;

		_particle.GetAttribute("AccelDirection").ValueFloat3
			= -vec;

		_particle.transform.localPosition = vec * (player.Body.localScale.x / 2);
		_prevPlayerPos = pos;
	}

	public override void OnDetach(Player player) {
		base.OnDetach(player);

		var render = player.GetComponentInChildren<Renderer>();
		render.material.color = _playerCol;

		player.Speed /= SpeedMul;

		if(_particle) ParticleManager.StopParticle(_particle);
	}

	public override float GetSectionTime(float speed) {
		return Path.GetPointLength(StartPoint, EndPoint) / speed / SpeedMul;
	}

	public override void EditGimmickLine(LineRenderer lineRenderer, ref float z) {

		lineRenderer.material.SetColor("_Color", GimmickColor);
		base.EditGimmickLine(lineRenderer, ref z);
	}
}
