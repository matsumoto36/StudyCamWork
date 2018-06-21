using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ギミックを構成するベースクラス
/// </summary>
public class GimmickBase : MonoBehaviour {

	const string MARK_MODEL_BASE_PATH = "Prefab/Mark/";

	#region カスタムインスペクターで表示するプロパティ
	[HideInInspector]
	public Color gimmickColor = Color.red;	//ギミックの適用範囲の色
	[HideInInspector]
	public float startPoint;				//ギミックを適用する開始地点
	[HideInInspector]
	public float endPoint;                  //ギミックを終わらせる終了地点
	[HideInInspector]
	public GameObject markModel;			//生成されたマークのモデル
	#endregion

	GameObject ringObj;

	protected string markModelName;         //ギミックのマークの名前
	protected float markModelSpawnZ;

	protected Bezier2D path;
	protected GimmickManager manager;

	public virtual void Init() {

		manager = GetComponentInParent<GimmickManager>();
		path = manager.Path;
	}

	public virtual void SpawnModel(Player player) {

		//登録されたモデルのスポーン
		var lineCount = path.LineCount;
		var spawnPos = (Vector3)path.GetPoint(startPoint / lineCount);
		spawnPos.z = markModelSpawnZ;

		if(markModelName != "") {
			var model = Resources.Load<GameObject>(MARK_MODEL_BASE_PATH + markModelName);
			if(model) {
				markModel = Instantiate(model, spawnPos, Quaternion.identity);
				markModel.transform.localScale = Vector3.one * player.Scale;
			}
		}

		//タイミングとるためのリングの生成
		ringObj = Instantiate(Resources.Load<GameObject>("Prefab/Ring"), spawnPos, Quaternion.identity);
		ringObj.transform.localScale = new Vector3();
		ringObj.GetComponent<Renderer>().material.SetColor("_Color", gimmickColor);
	}

	/// <summary>
	/// 残り時間があるときに毎フレーム呼ばれる
	/// </summary>
	/// <param name="t"></param>
	public virtual void OnRemainingTime(Player player, float t) {

		if(t < 1.0f) {
			var scale = Mathf.Lerp(player.Scale, player.Scale * 4f, t);
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
		markModel.SetActive(false);
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

		markModelSpawnZ = z;
	}

	bool CheckUsableManager() {

		if(!manager) return false;
		if(!manager.Path) return false;

		return true;
	}
}
