using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ギミックを構成するベースクラス
/// </summary>
public class GimmickBase : MonoBehaviour {

	public static readonly Vector3 ModelPosition = new Vector3(1, -1);

	#region カスタムインスペクターで表示するプロパティ
	[HideInInspector]
	public Color gimmickColor = Color.red;	//ギミックの適用範囲の色
	[HideInInspector]
	public float startPoint;				//ギミックを適用する開始地点
	[HideInInspector]
	public float endPoint;					//ギミックを終わらせる終了地点
	#endregion

	public Bezier2D targetPath;             //ギミックを取り付けるパス
	public GameObject startPointModel;		//生成されたギミックの始点のモデル
	public GameObject endPointModel;        //生成されたギミックの終点のモデル


	[SerializeField]
	GameObject startPointModelPre;			//ギミックの始点のモデルのプレハブ
	public Vector3 startPointModelOffset;	//モデルの配置のオフセット

	[SerializeField]
	GameObject endPointModelPre;			//ギミックの終点のモデルのプレハブ
	public Vector3 endPointModelOffset;		//モデルの配置のオフセット

	void Awake() {

		//登録されたモデルのスポーン
		var offset = ModelPosition + startPointModelOffset;
		var lineCount = targetPath.LineCount;

		if(startPointModelPre) {
			var pos = (Vector3)targetPath.GetPoint(startPoint / lineCount) + offset;
			startPointModel = Instantiate(startPointModelPre, pos, Quaternion.identity);
		}

		if(endPointModelPre) {
			var pos = (Vector3)targetPath.GetPoint(endPoint / lineCount) + offset;
			endPointModel = Instantiate(endPointModelPre, pos, Quaternion.identity);
		}
	}

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

	void OnDrawGizmos() {

		if(!targetPath) return;
		if(targetPath.LineCount <= 0) return;

		if(startPoint > endPoint) return;

		//モデルを表示
		Gizmos.color = Color.white;

		var offset = ModelPosition + startPointModelOffset;
		var lineCount = targetPath.LineCount;

		if(startPointModelPre) {
			MeshFilter mfs = startPointModelPre.GetComponent<MeshFilter>();
			if(mfs) {
				var pos = (Vector3)targetPath.GetPoint(startPoint / lineCount) + offset;
				Gizmos.DrawMesh(mfs.sharedMesh, pos);
			}
		}

		if(endPointModelPre) {
			MeshFilter mfe = endPointModelPre.GetComponent<MeshFilter>();
			if(mfe) {
				var pos = (Vector3)targetPath.GetPoint(endPoint / lineCount) + offset;
				Gizmos.DrawMesh(mfe.sharedMesh, pos);
			}
		}

	}
}
