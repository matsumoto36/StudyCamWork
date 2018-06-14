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
	public float endPoint;                  //ギミックを終わらせる終了地点
	[HideInInspector]
	public GameObject startPointModel;      //生成されたギミックの始点のモデル
	[HideInInspector]
	public GameObject endPointModel;        //生成されたギミックの終点のモデル
	#endregion

	GameObject ringObj;

	[SerializeField]
	GameObject startPointModelPre;			//ギミックの始点のモデルのプレハブ
	public Vector3 startPointModelOffset;	//モデルの配置のオフセット

	[SerializeField]
	GameObject endPointModelPre;			//ギミックの終点のモデルのプレハブ
	public Vector3 endPointModelOffset;     //モデルの配置のオフセット

	protected Bezier2D path;
	protected float startPointModelSpawnZ;
	protected float endPointModelSpawnZ;
	protected GimmickManager manager;

	public virtual void Init() {

		manager = GetComponentInParent<GimmickManager>();
		path = manager.path;
	}

	public virtual void SpawnModel() {

		//登録されたモデルのスポーン
		var offset = ModelPosition + startPointModelOffset;
		var lineCount = path.LineCount;

		if(startPointModelPre) {
			var pos = (Vector3)path.GetPoint(startPoint / lineCount) + offset;
			pos.z += startPointModelSpawnZ;
			startPointModel = Instantiate(startPointModelPre, pos, Quaternion.identity);
		}

		if(endPointModelPre) {
			var pos = (Vector3)path.GetPoint(endPoint / lineCount) + offset;
			pos.z += endPointModelSpawnZ;
			endPointModel = Instantiate(endPointModelPre, pos, Quaternion.identity);
		}

		Vector3 ringPos = path.GetPoint(startPoint / lineCount);
		ringPos.z = startPointModelSpawnZ;
		ringObj = Instantiate(Resources.Load<GameObject>("Prefab/Ring"), ringPos, Quaternion.identity);
		ringObj.transform.localScale = new Vector3();
		ringObj.GetComponent<Renderer>().material.SetColor("_Color", gimmickColor);
	}

	/// <summary>
	/// 残り時間があるときに毎フレーム呼ばれる
	/// </summary>
	/// <param name="t"></param>
	public virtual void OnRemainingTime(Player player, float t) {

		if(t < 1.0f) {
			float scale = Mathf.Lerp(1.1f, 4, t);
			ringObj.transform.localScale = new Vector3(scale, scale, 1);
		}
		else {
			ringObj.transform.localScale = new Vector3();
		}
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
		ringObj.transform.localScale = new Vector3();
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
	/// ギミックの予測線を引く(必ず実装すること)
	/// </summary>
	/// <param name="t"></param>
	/// <returns></returns>
	public virtual void EditGimmickLine(LineRenderer lineRenderer, ref float z) {

		if(!CheckUsableManager()) return;

		var partition = (int)(32 * (endPoint - startPoint));
		if(partition == 0) partition = 1;

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

	void OnDrawGizmos() {

		manager = GetComponentInParent<GimmickManager>();

		if(!CheckUsableManager()) return;

		path = manager.path;

		if(path.LineCount <= 0) return;

		if(startPoint > endPoint) return;

		//モデルを表示
		Gizmos.color = Color.white;

		var offset = ModelPosition + startPointModelOffset;
		var lineCount = path.LineCount;

		if(startPointModelPre) {
			MeshFilter mfs = startPointModelPre.GetComponent<MeshFilter>();
			if(mfs) {
				var pos = (Vector3)path.GetPoint(startPoint / lineCount) + offset;
				Gizmos.DrawMesh(mfs.sharedMesh, pos);
			}
		}

		if(endPointModelPre) {
			MeshFilter mfe = endPointModelPre.GetComponent<MeshFilter>();
			if(mfe) {
				var pos = (Vector3)path.GetPoint(endPoint / lineCount) + offset;
				Gizmos.DrawMesh(mfe.sharedMesh, pos);
			}
		}

	}

	bool CheckUsableManager() {

		if(!manager) return false;
		if(!manager.path) return false;

		return true;
	}
}
