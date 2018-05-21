using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ギミックを構成するベースクラス
/// </summary>
public class GimmickBase : MonoBehaviour {

	#region カスタムインスペクターで表示するプロパティ
	[HideInInspector]
	public Color gimmickColor = Color.red;	//ギミックの適用範囲の色
	[HideInInspector]
	public float startPoint;				//ギミックを適用する開始地点
	[HideInInspector]
	public float endPoint;					//ギミックを終わらせる終了地点
	#endregion

	public Bezier2D targetPath;				//ギミックを取り付けるパス

	public GameObject startPointModelPre;	//ギミックの始点のモデルのプレハブ
	public Vector3 startPointModelOffset;	//モデルの配置のオフセット

	public GameObject endPointModelPre;		//ギミックの終点のモデルのプレハブ
	public Vector3 endPointModelOffset;		//モデルの配置のオフセット

	/// <summary>
	/// 残り時間があるときに毎フレーム呼ばれる
	/// </summary>
	/// <param name="t"></param>
	public virtual void OnRemainingTime(Player player, float t) {

	}

	/// <summary>
	/// ギミックが発動しているときに毎フレーム呼ばれる
	/// </summary>
	public virtual void OnApplyUpdate(Player player, float t) {

	}

	/// <summary>
	/// ギミックが開始するとき呼ばれる
	/// </summary>
	public virtual void OnAttach(Player player) {

	}

	/// <summary>
	/// ギミックが終了するとき呼ばれる
	/// </summary>
	public virtual void OnDetach(Player player) {

	}

	/// <summary>
	/// 始点から終点までかかる時間を返す(必ず実装すること)
	/// </summary>
	/// <returns></returns>
	public virtual float GetSectionTime(float speed) {
		return 0;
	}

	/// <summary>
	/// ギミック中のプレイヤーの位置を比率で取得する(必ず実装すること)
	/// </summary>
	/// <param name="t"></param>
	/// <returns></returns>
	public virtual Vector2 GetPlayerPosition(float t) {
		return new Vector2();
	}
}
