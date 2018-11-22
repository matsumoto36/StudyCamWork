using UnityEngine;

/// <summary>
/// プレイヤーを奥や手前に移動させるギミック
/// </summary>
public class GimmickFocusIO : GimmickBase {

	public bool IsToFar = true;

	private float _moveZ;
	private float _playerSpeed;
	private DOFSlide _slide;

	public override void SpawnModel(Player player) {
		MarkModelName = IsToFar ? "MarkFocusIN" : "MarkFocusOUT";
		base.SpawnModel(player);
	}

	public override void OnAttach(Player player) {
		base.OnAttach(player);

		_playerSpeed = player.Speed;

		var baseLength = Path.GetPointLength(StartPoint, EndPoint);
		var duration = GameMaster.Instance.GameBalanceData.FocusDuration;
		player.Speed = baseLength / duration;
	}

	public override void OnApplyUpdate(Player player, float t) {
		base.OnApplyUpdate(player, t);

		if(!_slide) _slide = FindObjectOfType<DOFSlide>();

		var playerZRate = player.Body.localPosition.z / GimmickManager.MoveZ;
		var focusRate = _slide.Value;
		var focusGrace = GameMaster.Instance.GameBalanceData.FocusGrace;
		var isFocus = Mathf.Abs(playerZRate - focusRate) <= focusGrace;

		if(IsToFar == Input.GetMouseButton(0) && isFocus) {
			_slide.Value = playerZRate;
		}

		//プレイヤーのbodyのZを変更
		var duration = GameMaster.Instance.GameBalanceData.FocusDuration;
		var ratio = t / duration;

		if(!IsToFar) ratio = 1 - ratio;
		player.Body.localPosition = new Vector3(0, 0, ratio * _moveZ);
		player.SetScaleFromRatio(1 - ratio);

	}

	public override void OnDetach(Player player) {
		base.OnDetach(player);

		var z = IsToFar ? _moveZ : 0;
		player.Body.localPosition = new Vector3(0, 0, z);
		player.SetScaleFromRatio(IsToFar ? 0 : 1);

		player.Speed = _playerSpeed;
	}

	public override float GetSectionTime(float speed) {
		return GameMaster.Instance.GameBalanceData.FocusDuration;
	}

	public override void EditGimmickLine(LineRenderer lineRenderer, ref float z) {

		lineRenderer.material.SetColor("_Color", GimmickColor);

		_moveZ = GimmickManager.MoveZ;
		MarkModelSpawnZ = z;

		var partition = (int)(256 * (EndPoint - StartPoint));
		if(partition == 0) partition = 1;

		var diff = EndPoint - StartPoint;
		var dt =  partition == 0 ? 0 : 1.0f / partition;
		var point = new Vector3[partition + 1];
		var keyframe = new Keyframe[partition + 1];

		//奥もしくは手前に立体的に線を引く
		for(var i = 0;i <= partition;i++) {

			point[i] = Path.GetPoint((StartPoint + diff * dt * i) / Path.LineCount);
			point[i].z = _moveZ * dt * i;

			if(!IsToFar) {
				point[i].z = _moveZ - point[i].z;
			}

			var ratio = 1 - point[i].z / GimmickManager.MoveZ;
			keyframe[i] = 
				new Keyframe(i / (float)partition, Mathf.Lerp(GimmickManager.LineWidthMin, GimmickManager.LineWidthMax, ratio));
		}

		lineRenderer.positionCount = point.Length;
		lineRenderer.SetPositions(point);
		lineRenderer.widthCurve = new AnimationCurve(keyframe);

		//線のz位置が変わったので通知
		z = IsToFar ? _moveZ : 0.0f;
	}
}
