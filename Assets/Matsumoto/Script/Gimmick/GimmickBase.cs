using UnityEngine;

/// <summary>
/// ギミックを構成するベースクラス
/// </summary>
public abstract class GimmickBase : MonoBehaviour {

	private const string MarkModelBase = "Prefab/Mark/";

	//インスペクタ拡張で表示するため隠しておく
	//派生クラスの表示は拡張で記述しなくてもできるようにするため
	#region カスタムインスペクターで表示するプロパティ
	[HideInInspector]
	public Color GimmickColor;	//ギミックの適用範囲の色
	 [HideInInspector]
	public float StartPoint;	//ギミックを適用する開始地点
	 [HideInInspector]
	public float EndPoint;		//ギミックを終わらせる終了地点
	#endregion

	protected string MarkModelName;
	protected float MarkModelSpawnZ;

	protected Bezier2D Path;
	protected GimmickManager Manager;

	private GameObject _ringObj;

	//生成されたマークのモデル
	public GameObject MarkModel { get; private set; }

	public virtual void Init() {

		Manager = GetComponentInParent<GimmickManager>();
		Path = Manager.Path;
	}

	public virtual void SpawnModel(Player player) {

		//登録されたモデルのスポーン
		var lineCount = Path.LineCount;
		var spawnPos = (Vector3)Path.GetPoint(StartPoint / lineCount);
		spawnPos.z = MarkModelSpawnZ;

		if(MarkModelName != "") {
			var model = Resources.Load<GameObject>(MarkModelBase + MarkModelName);
			if(model) {
				MarkModel = Instantiate(model, spawnPos, Quaternion.identity);
				MarkModel.transform.localScale = Vector3.one * player.GetScaleFromRatio(1 - MarkModelSpawnZ / GimmickManager.MoveZ);
			}
		}

		//タイミングとるためのリングの生成
		_ringObj = Instantiate(Resources.Load<GameObject>("Prefab/Ring"), spawnPos, Quaternion.identity);
		_ringObj.transform.localScale = new Vector3();

		var rRender = _ringObj.GetComponent<Renderer>();
		rRender.material = new Material(rRender.material);
		rRender.material.EnableKeyword("_EMISSION");
		rRender.material.SetColor("_EmissionColor", GimmickColor);

	}

	/// <summary>
	/// 残り時間があるときに毎フレーム呼ばれる
	/// </summary>
	/// <param name="player"></param>
	/// <param name="t"></param>
	public virtual void OnRemainingTime(Player player, float t) {

		if(t < 1.0f) {
			var playerScale = player.GetScaleFromRatio(1 - MarkModelSpawnZ / GimmickManager.MoveZ);
			var scale = playerScale + playerScale * 3 * t;
			_ringObj.transform.localScale = new Vector3(scale, scale, 1);
		}
		else {
			_ringObj.transform.localScale = new Vector3();
		}
	}

	/// <summary>
	/// ギミックが発動しているときに毎フレーム呼ばれる
	/// </summary>
	public virtual void OnApplyUpdate(Player player, float t) { }

	/// <summary>
	/// ギミックが開始するとき呼ばれる
	/// </summary>
	public virtual void OnAttach(Player player) {
		_ringObj.transform.localScale = new Vector3();
		MarkModel.SetActive(false);

		var p = ParticleManager.Spawn("GimmickApplyEffect", MarkModel.transform.position, Quaternion.identity, 2);
		p.GetAttribute("MainColor").ValueFloat4 = GimmickColor;
	}

	/// <summary>
	/// ギミックが終了するとき呼ばれる
	/// </summary>
	public virtual void OnDetach(Player player) { }

	/// <summary>
	/// 始点から終点までかかる時間を返す(必ず実装すること)
	/// </summary>
	/// <returns></returns>
	public abstract float GetSectionTime(float speed);

	/// <summary>
	/// ギミックの予測線を引く(必ず実装すること)
	/// </summary>
	/// <param name="lineRenderer"></param>
	/// <param name="z">線のZ座標</param>
	/// <returns></returns>
	public virtual void EditGimmickLine(LineRenderer lineRenderer, ref float z) {

		if(!CheckUsableManager()) return;

		var partition = (int)(256 * (EndPoint - StartPoint));
		if(partition == 0) partition = 1;

		var dt = (EndPoint - StartPoint) * (1.0f / partition);
		var point = new Vector3[partition + 1];
		var keyframe = new Keyframe[partition + 1];

		//Zの位置に従って線を引く
		for(var i = 0;i <= partition;i++) {
			point[i] = Path.GetPoint((StartPoint + dt * i) / Path.LineCount);
			point[i].z = z;

			var ratio = 1 - z / GimmickManager.MoveZ;
			keyframe[i] = new Keyframe(i / (float)partition, Mathf.Lerp(GimmickManager.LineWidthMin, GimmickManager.LineWidthMax, ratio));
		}

		lineRenderer.positionCount = point.Length;
		lineRenderer.SetPositions(point);
		lineRenderer.widthCurve = new AnimationCurve(keyframe);

		MarkModelSpawnZ = z;
	}

	private bool CheckUsableManager() {
		return !Manager ? false : Manager.Path;
	}
}
