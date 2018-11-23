using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PopcornFXのパーティクルを出したり消したりする
/// </summary>
public sealed class ParticleManager : SingletonMonoBehaviour<ParticleManager> {

	private const string ParticleDataPath = "Particle";

	private PKFxRenderingPlugin _renderer;
	private Dictionary<string, ObjectPooler> _poolTable;
	private Dictionary<string, PKFxFX> _particleTable;

	public static bool IsRenderingParticle {
		get {
			if(!Instance._renderer)
				Instance._renderer = 
					Camera.main.GetComponent<PKFxRenderingPlugin>();

			return Instance._renderer.enabled;
		}
		set {
			if(!Instance._renderer)
				Instance._renderer = 
					Camera.main.GetComponent<PKFxRenderingPlugin>();

			Instance._renderer.enabled = value;

			if(!value) {
				//パーティクルの描画を消す
				PKFxManager.Reset();
			}
		}
	}

	//外部からのnew禁止
	private ParticleManager() { }

	private static IEnumerator StopParticleDelay(PKFxFX particle, float delayTime) {
		yield return new WaitForSeconds(delayTime);
		StopParticle(particle);
	}

	protected override void Init() {
		base.Init();
		Load();
	}

	/// <summary>
	/// エフェクトをすべてロード
	/// </summary>
	public static void Load() {

		Instance._particleTable = new Dictionary<string, PKFxFX>();
		Instance._poolTable = new Dictionary<string, ObjectPooler>();

		var data = Resources.LoadAll<PKFxFX>(ParticleDataPath);

		foreach(var item in data) {

			Instance._particleTable.Add(item.name, item);

			var pool = ObjectPooler.GetObjectPool(item.gameObject);
			pool.MaxCount = 200;
			pool.PrepareCount = 100;
			pool.Generate(Instance.transform);
			Instance._poolTable.Add(item.name, pool);
		}
	}

	/// <summary>
	/// パーティクルを生成する(自動削除)
	/// </summary>
	/// <param name="particleName"></param>
	/// <param name="position"></param>
	/// <param name="rotation"></param>
	/// <returns></returns>
	public static PKFxFX Spawn(string particleName, Vector3 position, Quaternion rotation) {

		return Spawn(particleName, position, rotation, .1f);
	}

	/// <summary>
	/// パーティクルを生成する
	/// deleteTimeに0を設定すると削除されない
	/// </summary>
	/// <param name="particleName"></param>
	/// <param name="position"></param>
	/// <param name="rotation"></param>
	/// <param name="deleteTime"></param>
	/// <returns></returns>
	public static PKFxFX Spawn(string particleName, Vector3 position, Quaternion rotation, float deleteTime) {

		if(!Instance._particleTable.ContainsKey(particleName)) {
			Debug.LogWarning(particleName + " is not found.");
			return null;
		}

		var pObj = Instance._poolTable[particleName].GetInstance().GetComponent<PKFxFX>();
		pObj.transform.SetPositionAndRotation(position, rotation);
		pObj.StartEffect();

		//自動削除の時間が設定されている場合
		if(deleteTime > 0) {
			pObj.StartCoroutine(StopParticleDelay(pObj, deleteTime));
		}

		return pObj;
	}

	/// <summary>
	/// エフェクトの再生を止めてプールに返す
	/// </summary>
	/// <param name="particle"></param>
	public static void StopParticle(PKFxFX particle) {
		particle.TerminateEffect();
		ObjectPooler.ReleaseInstance(particle.gameObject);
	}
}
