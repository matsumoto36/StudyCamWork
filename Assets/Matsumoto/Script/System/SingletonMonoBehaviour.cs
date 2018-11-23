using UnityEngine;

/// <summary>
/// シングルトンの親クラス
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour {

	private static T _instance;
	public static T Instance {
		get {
			if(!_instance) Create();
			return _instance;
		}
	}

	/// <summary>
	/// 自身を生成する
	/// </summary>
	private static void Create() {
		_instance = new GameObject(string.Format("[Singleton - {0}]", typeof(T)))
			.AddComponent<T>();

		DontDestroyOnLoad(_instance.gameObject);

		_instance.GetComponent<SingletonMonoBehaviour<T>>().Init();
	}

	/// <summary>
	/// 生成されたときに一回だけ呼ばれる
	/// </summary>
	protected virtual void Init() { }

	private void Awake() {
		if(_instance) Destroy(gameObject);
	}
}
