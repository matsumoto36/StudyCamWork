using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

/// <summary>
/// オブジェクトプールパターンを実現する
/// </summary>
public class ObjectPooler : MonoBehaviour {

	private static GameObject _poolAttachedObject;

	private readonly List<GameObject> _pooledObjectList = new List<GameObject>();

	public GameObject Prefab;
	public Transform Parent;
	public int MaxCount = 100;
	public int PrepareCount;

	[SerializeField]
	private int _interval = 1;

	private void OnDisable() {
		StopAllCoroutines();
	}

	/// <summary>
	/// prepareの数まで生成する
	/// </summary>
	public void Generate(Transform parent) {

		if(!Prefab) return;

		Parent = parent;

		var count = PrepareCount - _pooledObjectList.Count;
		for(var i = 0;i < count;i++) {
			var g = Instantiate(Prefab);
			g.transform.SetParent(parent);
			g.SetActive(false);
			_pooledObjectList.Add(g);
		}
	}

	public void OnDestroy() {
		if(_poolAttachedObject == null)
			return;

		if(_poolAttachedObject.GetComponents<ObjectPooler>().Length == 1) {
			_poolAttachedObject = null;
		}
		foreach(var obj in _pooledObjectList) {
			Destroy(obj);
		}
		_pooledObjectList.Clear();
	}

	public int Interval {
		get { return _interval; }
		set {
			if(_interval == value) return;

			_interval = value;

			StopAllCoroutines();
		}
	}

	public GameObject GetInstance() {
		return GetInstance(transform);
	}

	public GameObject GetInstance(Transform parent) {

		var retObj = _pooledObjectList
			.Where(item => item)
			.FirstOrDefault(item => !item.activeSelf);

		if(retObj) {
			retObj.SetActive(true);
			return retObj;
		}

		if(_pooledObjectList.Count < MaxCount) {
			var obj = Instantiate(Prefab);
			obj.SetActive(true);
			obj.transform.parent = parent;
			_pooledObjectList.Add(obj);
			return obj;
		}

		Debug.LogWarning(Prefab.name + " pool is Empty.");
		return null;
	}

	public static void ReleaseInstance(GameObject obj) {

		obj.transform.SetParent(_poolAttachedObject.transform);
		obj.SetActive(false);
	}

	private IEnumerator RemoveObjectCheck() {
		while(true) {
			RemoveObject(PrepareCount);
			yield return new WaitForSeconds(_interval);
		}
	}

	public void RemoveObject(int max) {
		if(_pooledObjectList.Count <= max) return;

		var needRemoveCount = _pooledObjectList.Count - max;
		foreach(var obj in _pooledObjectList.ToArray()) {

			if(needRemoveCount == 0) break;
			if(obj.activeSelf) continue;

			_pooledObjectList.Remove(obj);
			Destroy(obj);
			needRemoveCount--;
		}
	}

	public static ObjectPooler GetObjectPool(GameObject obj) {

		if(_poolAttachedObject == null) {
			_poolAttachedObject = GameObject.Find("ObjectPool");
			if(_poolAttachedObject == null) {
				_poolAttachedObject = new GameObject("ObjectPool");
				DontDestroyOnLoad(_poolAttachedObject);
			}
		}

		foreach(var pool in _poolAttachedObject.GetComponents<ObjectPooler>()) {
			if(pool.Prefab == obj) {
				return pool;
			}
		}

		foreach(var pool in FindObjectsOfType<ObjectPooler>()) {
			if(pool.Prefab == obj) {
				return pool;
			}
		}

		var newPool = _poolAttachedObject.AddComponent<ObjectPooler>();
		newPool.Prefab = obj;
		return newPool;
	}
}