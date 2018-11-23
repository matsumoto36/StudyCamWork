using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン遷移を行う
/// </summary>
public class TimerController : MonoBehaviour {

	public static string NextScene;//シーン名

	private PostEffect _effect;
	private bool _isSceneMoving;

	private void Start() {
		_effect = GetComponent<PostEffect>();
		DontDestroyOnLoad(gameObject);
	}

	/// <summary>
	/// シーン遷移
	/// </summary>
	/// <param name="nextSceneName"></param>
	public void StageSelect(string nextSceneName) {
		NextScene = nextSceneName;//Next_Sceneに名前を入れる
		SceneManager.LoadScene(NextScene);//飛びますよ。
	}

	//フェードアウト->シーン移動->フェードイン
	IEnumerator SceneMoveCoroutine(string nextSceneName) {

		yield return StartCoroutine(_effect.FadeOut());

		NextScene = nextSceneName;
		SceneManager.LoadScene(NextScene);

		yield return new WaitForSeconds(1);
		yield return StartCoroutine(_effect.FadeIn());

		Destroy(gameObject);
	}
	public void SceneMove(string SceneName) {
		if(_isSceneMoving) {
			foreach(var item in FindObjectsOfType<TimerController>()) {
				if(item == this) continue;
				item.SceneMove(SceneName);
				return;
			}
		}

		_isSceneMoving = true;
		StartCoroutine(SceneMoveCoroutine(SceneName));
	}
}