using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class MakingScene : MonoBehaviour {

	public string loadStudioSet;

	GameObject studioSet;

	// Use this for initialization
	void Start () {
		if(!Application.isPlaying) return;

		Camera.main.gameObject.SetActive(false);

		DontDestroyOnLoad(gameObject);

		GameMaster.LoadStudioName = loadStudioSet;
		GameMaster.LoadPathName = FindObjectOfType<GimmickManager>().name;

		SceneManager.LoadScene("DefaultStage");
		Destroy(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		if(Application.isPlaying) return;
			CheckStudioSet();
	}

	void CheckStudioSet() {

		DestroyDuplicatetudioSet();

		if(loadStudioSet == "") return;

		if(studioSet) {
			if(studioSet.name == loadStudioSet) return;
			DestroyImmediate(studioSet.gameObject);
		}

		SpawnStudioSet();

	}

	void DestroyDuplicatetudioSet() {

		while(!studioSet && transform.childCount > 0 
			|| studioSet && transform.childCount > 1) {

			var child = transform.GetChild(0).gameObject;
			if(child != studioSet) DestroyImmediate(child);
		}
		
	}

	void SpawnStudioSet() {

		var obj = Resources.Load<GameObject>(GameMaster.STUDIO_PREFAB_BASE_PATH + loadStudioSet);
		if(!obj) return;
		studioSet = Instantiate(obj);
		studioSet.transform.SetParent(transform);
	}
}
