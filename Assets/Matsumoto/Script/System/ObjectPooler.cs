using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ObjectPooler : MonoBehaviour {

	public GameObject prefab;
	public Transform parent;
	public int maxCount = 100;
	public int prepareCount = 0;
	[SerializeField]
	private int interval = 1;
	private List<GameObject> pooledObjectList = new List<GameObject>();
	private static GameObject poolAttachedObject = null;

	void OnEnable() {
		//if(interval > 0)
			//StartCoroutine(RemoveObjectCheck());
	}

	void OnDisable() {
		StopAllCoroutines();
	}

	/// <summary>
	/// prepareの数まで生成する
	/// </summary>
	public void Generate(Transform parent) {

		if(!prefab) return;

		this.parent = parent;

		var count = prepareCount - pooledObjectList.Count;
		for(int i = 0;i < count;i++) {
			var g = Instantiate(prefab);
			g.transform.SetParent(parent);
			g.SetActive(false);
			pooledObjectList.Add(g);
		}
	}

	public void OnDestroy() {
		if(poolAttachedObject == null)
			return;

		if(poolAttachedObject.GetComponents<ObjectPooler>().Length == 1) {
			poolAttachedObject = null;
		}
		foreach(var obj in pooledObjectList) {
			Destroy(obj);
		}
		pooledObjectList.Clear();
	}

	public int Interval {
		get { return interval; }
		set {
			if(interval != value) {
				interval = value;

				StopAllCoroutines();
				//if(interval > 0)
					//StartCoroutine(RemoveObjectCheck());
			}
		}
	}

	public GameObject GetInstance() {
		return GetInstance(transform);
	}

	public GameObject GetInstance(Transform parent) {
		//pooledObjectList.RemoveAll((obj) => obj == null);

		var retObj = pooledObjectList
			.Where(item => item)
			.FirstOrDefault(item => !item.activeSelf);

		if(retObj) {
			retObj.SetActive(true);
			return retObj;
		}

		if(pooledObjectList.Count < maxCount) {
			GameObject obj = Instantiate(prefab);
			obj.SetActive(true);
			obj.transform.parent = parent;
			pooledObjectList.Add(obj);
			return obj;
		}

		Debug.LogWarning(prefab.name + " pool is Empty.");
		return null;
	}

	public static void ReleaseInstance(GameObject obj) {

		obj.transform.SetParent(poolAttachedObject.transform);
		obj.SetActive(false);
		obj = null;
	}

	IEnumerator RemoveObjectCheck() {
		while(true) {
			RemoveObject(prepareCount);
			yield return new WaitForSeconds(interval);
		}
	}

	public void RemoveObject(int max) {
		if(pooledObjectList.Count > max) {
			int needRemoveCount = pooledObjectList.Count - max;
			foreach(GameObject obj in pooledObjectList.ToArray()) {
				if(needRemoveCount == 0) {
					break;
				}
				if(obj.activeSelf == false) {
					pooledObjectList.Remove(obj);
					Destroy(obj);
					needRemoveCount--;
				}
			}
		}
	}

	public static ObjectPooler GetObjectPool(GameObject obj) {
		if(poolAttachedObject == null) {
			poolAttachedObject = GameObject.Find("ObjectPool");
			if(poolAttachedObject == null) {
				poolAttachedObject = new GameObject("ObjectPool");
				DontDestroyOnLoad(poolAttachedObject);
			}
		}

		foreach(var pool in poolAttachedObject.GetComponents<ObjectPooler>()) {
			if(pool.prefab == obj) {
				return pool;
			}
		}

		foreach(var pool in FindObjectsOfType<ObjectPooler>()) {
			if(pool.prefab == obj) {
				return pool;
			}
		}

		var newPool = poolAttachedObject.AddComponent<ObjectPooler>();
		newPool.prefab = obj;

		//FadeManager.onSceneChanged += () => {

		//	//使っているものがあればしまう
		//	foreach(var item in newPool.pooledObjectList) {
		//		if(item && item.activeSelf) item.SetActive(false);
		//	}

		//	//シーン移動時に削除されたオブジェクトを補てんする
		//	newPool.Generate(newPool.parent);
		//};

		return newPool;
	}
}