using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// シングルトンの親クラス
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour {

	static T instance;

	public static T Instance {
		get
		{
			if (!instance) Create();
			return instance;
		}
	}

	static void Create()
	{
		instance = new GameObject(string.Format("[Singleton - {0}]", typeof(T).ToString()))
			.AddComponent<T>();

		DontDestroyOnLoad(instance.gameObject);

		instance.GetComponent<SingletonMonoBehaviour<T>>().Init();
	}

	/// <summary>
	/// 初期化用
	/// </summary>
	protected virtual void Init() { }

	void Awake() {
		if(instance) Destroy(gameObject);
	}
}
