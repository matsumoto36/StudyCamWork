using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ステージエディターの
/// 背景と実行部分を担うクラス
/// </summary>
[ExecuteInEditMode]
public class MakingScene : MonoBehaviour {

	public string LoadStudioSet;

	GameObject _studioSet;

	// Use this for initialization
	void Start () {
		if(!Application.isPlaying) return;

		//ゲーム開始
		Camera.main.gameObject.SetActive(false);

		DontDestroyOnLoad(gameObject);

		GameMaster.LoadStudioName = LoadStudioSet;
		GameMaster.LoadPathName = FindObjectOfType<GimmickManager>().name;

		SceneManager.LoadScene("GameScene");
		Destroy(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		if(Application.isPlaying) return;
			CheckStudioSet();
	}

	/// <summary>
	/// 背景が生成されているか確認する
	/// </summary>
	void CheckStudioSet() {

		DestroyDuplicatetudioSet();

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
	void DestroyDuplicatetudioSet() {

		while(!_studioSet && transform.childCount > 0 
			|| _studioSet && transform.childCount > 1) {

			var child = transform.GetChild(0).gameObject;
			if(child != _studioSet) DestroyImmediate(child);
		}
		
	}

	/// <summary>
	/// 背景を生成する
	/// </summary>
	void SpawnStudioSet() {
		var obj = Resources.Load<GameObject>(GameMaster.STUDIO_PREFAB_BASE_PATH + LoadStudioSet);
		if(!obj) return;
		_studioSet = Instantiate(obj);
		_studioSet.transform.SetParent(transform);
	}
}
