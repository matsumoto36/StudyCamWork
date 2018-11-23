using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ステージエディターの
/// 背景と実行部分を担うクラス
/// </summary>
[ExecuteInEditMode]
public class MakingScene : MonoBehaviour {

	public string LoadStudioSet;

	private GameObject _studioSet;

	private void Start() {
		if(!Application.isPlaying) return;

		//ゲーム開始
		GameMaster.IsTestPlayMode = true;
		Camera.main.gameObject.SetActive(false);

		DontDestroyOnLoad(gameObject);

		GameMaster.LoadStudioName = LoadStudioSet;
		var stage = FindObjectOfType<GimmickManager>();

		if(!stage) {
			Debug.LogError("テストするステージが見つかりませんでした");
			Debug.Break();
		}

		GameMaster.LoadPathName = stage.name;

		//生成できるかチェック
		var prefabPath = GameMaster.PathPrefabBasePath + GameMaster.LoadPathName;
		var prefab = Resources.Load(prefabPath);
		if(!prefab) {
#if UNITY_EDITOR
			//読み込めるようにプレハブを作成
			var savePath = "Assets/Resources/" + prefabPath + ".prefab";
			PrefabUtility.CreatePrefab(savePath, stage.gameObject);
			AssetDatabase.SaveAssets();
			Debug.Log("Create Stage path=" + savePath);
#endif
		}
		else {
			//更新しておく
			PrefabUtility.ReplacePrefab(stage.gameObject, prefab);
		}

		SceneManager.LoadScene("GameScene");
		Destroy(gameObject);
	}

	// Update is called once per frame
	private void Update() {
		if(Application.isPlaying) return;
		CheckStudioSet();
	}

	/// <summary>
	/// 背景が生成されているか確認する
	/// </summary>
	private void CheckStudioSet() {

		DestroyDuplicateStudioSet();

		if(LoadStudioSet == "") return;

		if(_studioSet) {
			if(_studioSet.name == LoadStudioSet) return;
			DestroyImmediate(_studioSet.gameObject);
		}

		SpawnStudioSet();

	}

	/// <summary>
	/// 背景を一つにする
	/// </summary>
	private void DestroyDuplicateStudioSet() {

		while(!_studioSet && transform.childCount > 0
			|| _studioSet && transform.childCount > 1) {

			var child = transform.GetChild(0).gameObject;
			if(child != _studioSet) DestroyImmediate(child);
		}

	}

	/// <summary>
	/// 背景を生成する
	/// </summary>
	private void SpawnStudioSet() {
		var obj = Resources.Load<GameObject>(GameMaster.StudioPrefabBasePath + LoadStudioSet);
		if(!obj) return;
		_studioSet = Instantiate(obj);
		_studioSet.transform.SetParent(transform);
	}
}
