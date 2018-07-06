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

	DOFSlide slide;

	public override void SpawnModel(Player player) {

		if(isToFar) {
			markModelName = "MarkFocusIN";
		}
		else {
			markModelName = "MarkFocusOUT";
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

		playerSpeed = player.Speed;

		var baseLength = path.GetPointLength(startPoint, endPoint);
		var duration = GameMaster.Instance.GameBalanceData.FocusDuration;
		player.Speed = baseLength / duration;
	}

	public override void OnApplyUpdate(Player player, float t) {
		base.OnApplyUpdate(player, t);

		if(!slide) slide = FindObjectOfType<DOFSlide>();

		var playerZRate = player.Body.localPosition.z / GimmickManager.MOVE_Z;
		var focusRate = slide.Value;
		var focusGrace = GameMaster.Instance.GameBalanceData.FocusGrace;
		var isFocus = Mathf.Abs(playerZRate - focusRate) <= focusGrace;

		if(isToFar == Input.GetMouseButton(0) && isFocus) {
			slide.Value = playerZRate;
		}

		//プレイヤーのbodyのZを変更
		var duration = GameMaster.Instance.GameBalanceData.FocusDuration;
		var ratio = t / duration;
		if(!isToFar) ratio = 1 - ratio;
		player.Body.localPosition = new Vector3(0, 0, ratio * moveZ);
		player.SetScaleFromRatio(1 - ratio);

		if(text)
			text.text = "Using. " + t;
	}

	public override void OnDetach(Player player) {
		base.OnDetach(player);

		var z = isToFar ? moveZ : 0;
		player.Body.localPosition = new Vector3(0, 0, z);
		player.SetScaleFromRatio(isToFar ? 0 : 1);

		player.Speed = playerSpeed;
	}

	public override float GetSectionTime(float speed) {
		return GameMaster.Instance.GameBalanceData.FocusDuration;
	}

	public override void EditGimmickLine(LineRenderer lineRenderer, ref float z) {

		lineRenderer.material.SetColor("_Color", gimmickColor);

		moveZ = GimmickManager.MOVE_Z;
		markModelSpawnZ = z;

		var partition = (int)(256 * (endPoint - startPoint));
		if(partition == 0) partition = 1;

		var diff = endPoint - startPoint;
		var dt =  partition == 0 ? 0 : 1.0f / partition;
		var point = new Vector3[partition + 1];
		var keyframe = new Keyframe[partition + 1];

		for(int i = 0;i <= partition;i++) {

			point[i] = path.GetPoint((startPoint + diff * dt * i) / path.LineCount);
			point[i].z = moveZ * dt * i;

			if(!isToFar) {
				point[i].z = moveZ - point[i].z;
			}

			keyframe[i] = new Keyframe(i / (float)partition, Mathf.Lerp(GimmickManager.LINE_WIDTH_MIN, GimmickManager.LINE_WIDTH_MAX, 1 - point[i].z / GimmickManager.MOVE_Z));
		}

		lineRenderer.positionCount = point.Length;
		lineRenderer.SetPositions(point);
		lineRenderer.widthCurve = new AnimationCurve(keyframe);

		if(isToFar) {
			z = moveZ;
		}
		else {
			z = 0;
		}
	}
}
