using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;
using System;

/// <summary>
/// 音の管理をする
/// </summary>
public sealed class AudioManager : SingletonMonoBehaviour<AudioManager> {

	// 各種パス
	private const string MixerPath = "Sound/MainAudioMixer";
	private const string BGMDirectory = "Sound/BGM/";
	private const string SEDirectory = "Sound/SE/";

	// Mixerの公開変数にアクセスするためのキー
	private const string BGMVolumeKey = "BGMVolume";
	private const string SEVolumeKey = "SEVolume";

	private readonly AudioMixerGroup[] _mixerGroups = new AudioMixerGroup[2]; //ミキサーのグループ [0]SE [1]BGM

	private ObjectPooler _poolSE;

	//再生用リスト
	private Dictionary<string, AudioClipInfo> _SEClips;
	private Dictionary<string, AudioClip> _BGMClips;

	private AudioSource _nowPlayingBGM;									//現在再生されているBGM
	private string _latestPlayBGM;										//再生されているBGMの種類

	private Coroutine _fadeInCoroutine;									//BGMフェードインのコルーチン
	private AudioSource _fadeInAudio;									//BGMフェードイン用のAudioSource

	public AudioMixer Mixer { get; private set; }           //ミキサー

	/// <summary>
	/// MixerのSEの音量を変更する
	/// </summary>
	public static float SEVolume {
		get {
			float volume;
			Instance.Mixer.GetFloat(SEVolumeKey, out volume);
			return volume;
		}
		set {
			Instance.Mixer.SetFloat(SEVolumeKey, value);
		}
	}

	/// <summary>
	/// MixerのBGMの音量を変更する
	/// </summary>
	public static float BGMVolume {
		get {
			float volume;
			Instance.Mixer.GetFloat(BGMVolumeKey, out volume);
			return volume;
		}
		set {
			Instance.Mixer.SetFloat(BGMVolumeKey, value);
		}
	}

	/// <summary>
	/// 現在再生されているBGMの名前を取得
	/// </summary>
	public static string CurrentBGMName {
		get {
			return !Instance._nowPlayingBGM ? "" : Instance._latestPlayBGM;
		}
	}

	protected override void Init() {
		base.Init();
		Load();
	}

	/// <summary>
	/// SEを遅延させて止める
	/// </summary>
	/// <param name="delayTime">遅延する時間(秒)</param>
	/// <param name="action">コールバック</param>
	static IEnumerator StopSEDelay(float delayTime, Action action) {
		yield return new WaitForSeconds(delayTime);
		action();
	}

	/// <summary>
	/// 各音情報を読み込み
	/// </summary>
	public static void Load() {

		//LoadMixer
		Instance.Mixer = Resources.Load<AudioMixer>(MixerPath);
		if(Instance.Mixer) {
			Instance._mixerGroups[0] = Instance.Mixer.FindMatchingGroups("SE")[0];
			Instance._mixerGroups[1] = Instance.Mixer.FindMatchingGroups("BGM")[0];
		}
		else {
			Debug.LogError("Failed Load AudioMixer! Setting Path=" + MixerPath);
		}


		//BGM読み込み
		Instance._BGMClips = new Dictionary<string, AudioClip>();
		foreach(var item in Resources.LoadAll<AudioClip>(BGMDirectory)) {
			Instance._BGMClips.Add(item.name, item);
		}

		//SE読み込み
		Instance._SEClips = new Dictionary<string, AudioClipInfo>();
		foreach(var item in Resources.LoadAll<AudioClip>(SEDirectory)) {
			Instance._SEClips.Add(item.name, new AudioClipInfo(item));
		}

		//プールの作成
		Instance._poolSE = ObjectPooler.GetObjectPool(new GameObject("[Stopped]")
			.AddComponent<AudioSource>()
			.gameObject);

		Instance._poolSE.MaxCount = 20;
		Instance._poolSE.PrepareCount = 10;
		Instance._poolSE.Generate(Instance.transform);
	}

	/// <summary>
	/// SEを再生する
	/// </summary>
	/// <param name="SEName">SEの名前</param>
	/// <param name="vol">音量</param>
	public static AudioSource PlaySE(string SEName, float vol = 1.0f) {

		//SE取得
		var info = GetSEInfo(SEName);
		if(info == null) return null;
		if (info.StockList.Count <= 0) return null;

		//stockListから空で且つ番号が一番若いSEInfoを受け取る
		var seInfo = info.StockList.Values[0];

		//ストックを削除
		info.StockList.Remove(seInfo.Index);

		//情報を取り付ける
		var obj = Instance._poolSE.GetInstance();
		if(!obj) return null;

		var src = obj.GetComponent<AudioSource>();
		src.name = "[Audio SE - " + SEName + "]";
		src.transform.SetParent(Instance.transform);
		src.clip = info.Clip;
		src.volume = seInfo.Volume * vol;
		src.outputAudioMixerGroup = Instance._mixerGroups[0];
		src.Play();

		//遅延でストップする
		Instance.StartCoroutine(StopSEDelay(src.clip.length + 0.1f, () => {

			src.name = "[Stopped]";
			src.Stop();
			ObjectPooler.ReleaseInstance(src.gameObject);

			info.StockList.Add(seInfo.Index, seInfo);
		}));

		return src;
	}

	/// <summary>
	/// SEを再生する
	/// </summary>
	/// <param name="type">SEの名前</param>
	/// <param name="vol">音量</param>
	public static AudioSource PlaySERaw(string SEName, float vol = 1.0f) {

		//SE取得
		var info = GetSEInfo(SEName);
		if(info == null) return null;

		//情報を取り付ける
		var src = Instance._poolSE.GetInstance().GetComponent<AudioSource>();
		src.name = "[Audio SE - " + SEName + "]";
		src.transform.SetParent(Instance.transform);
		src.clip = info.Clip;
		src.volume = vol;
		src.outputAudioMixerGroup = Instance._mixerGroups[0];
		src.Play();

		return src;
	}

	/// <summary>
	/// BGMを再生する
	/// </summary>
	/// <param name="BGMName">BGMの名前</param>
	/// <param name="vol">音量</param>
	/// <param name="isLoop">ループ再生するか</param>
	/// <returns>再生しているBGM</returns>
	public static AudioSource PlayBGM(string BGMName, float vol = 1.0f, bool isLoop = true) {

		//BGM取得
		var clip = GetBGM(BGMName);
		if(!clip) return null;
		if(Instance._nowPlayingBGM) Destroy(Instance._nowPlayingBGM.gameObject);

		var src = new GameObject("[Audio BGM - " + BGMName + "]").AddComponent<AudioSource>();
		src.transform.SetParent(Instance.transform);
		src.clip = clip;
		src.volume = vol;
		src.outputAudioMixerGroup = Instance._mixerGroups[1];
		src.Play();

		if(isLoop) {
			src.loop = true;
		}
		else {
			Destroy(src.gameObject, clip.length + 0.1f);
		}

		Instance._nowPlayingBGM = src;
		Instance._latestPlayBGM = BGMName;

		return src;
	}

	/// <summary>
	/// BGMをフェードインさせる
	/// </summary>
	/// <param name="fadeTime">フェードする時間</param>
	/// <param name="BGMName">新しいBGMのタイプ</param>
	/// <param name="vol">新しいBGMの大きさ</param>
	/// <param name="isLoop">新しいBGMがループするか</param>
	public static void FadeIn(float fadeTime, string BGMName, float vol = 1.0f, bool isLoop = true) {
		Instance._fadeInCoroutine = Instance.StartCoroutine(Instance.FadeInCoroutine(fadeTime, BGMName, vol, isLoop));
	}

	/// <summary>
	/// BGMをフェードアウトさせる
	/// </summary>
	/// <param name="fadeTime">フェードする時間</param>
	public static void FadeOut(float fadeTime) {
		Instance.StartCoroutine(Instance.FadeOutAnim(fadeTime));
	}

	/// <summary>
	/// BGMをクロスフェードする
	/// </summary>
	/// <param name="fadeTime">フェードする時間</param>
	/// <param name="fadeInBGMName">新しいBGMのタイプ</param>
	/// <param name="vol">新しいBGMの大きさ</param>
	/// <param name="isLoop">新しいBGMがループするか</param>
	public static void CrossFade(float fadeTime, string fadeInBGMName, float vol = 1.0f, bool isLoop = true) {
		Instance.StartCoroutine(Instance.FadeOutAnim(fadeTime));
		Instance._fadeInCoroutine = Instance.StartCoroutine(Instance.FadeInCoroutine(fadeTime, fadeInBGMName, vol, isLoop));
	}

	/// <summary>
	/// SEを取得する
	/// </summary>
	/// <param name="SEName">SEの名前</param>
	/// <returns>SE</returns>
	private static AudioClipInfo GetSEInfo(string SEName) {
		if (Instance._SEClips.ContainsKey(SEName)) return Instance._SEClips[SEName];
		Debug.LogWarning("SEName:" + SEName + " is not found.");
		return null;
	}

	/// <summary>
	/// BGMを取得する
	/// </summary>
	/// <param name="BGMName">BGMの名前</param>
	/// <returns>BGM</returns>
	private static AudioClip GetBGM(string BGMName) {
		if (Instance._BGMClips.ContainsKey(BGMName)) return Instance._BGMClips[BGMName];
		Debug.LogError("BGMName:" + BGMName + " is not found.");
		return null;
	}

	/// <summary>
	/// フェードインするためのコルーチン
	/// </summary>
	/// <param name="fadeTime"></param>
	/// <param name="BGMName"></param>
	/// <param name="vol"></param>
	/// <param name="isLoop"></param>
	/// <returns></returns>
	private IEnumerator FadeInCoroutine(float fadeTime, string BGMName, float vol, bool isLoop) {

		//BGM取得
		var clip = GetBGM(BGMName);
		if(!clip) yield break;

		//初期設定
		_fadeInAudio = new GameObject("[Audio BGM - " + BGMName + " - FadeIn ]").AddComponent<AudioSource>();
		_fadeInAudio.transform.SetParent(Instance.transform);
		_fadeInAudio.clip = clip;
		_fadeInAudio.volume = 0;
		_fadeInAudio.outputAudioMixerGroup = _mixerGroups[1];
		_fadeInAudio.Play();

		_latestPlayBGM = BGMName;

		//フェードイン
		var t = 0.0f;
		while((t += Time.deltaTime / fadeTime) < 1.0f) {
			_fadeInAudio.volume = t * vol;
			yield return null;
		}

		_fadeInAudio.volume = vol;
		_fadeInAudio.name = "[Audio BGM - " + BGMName + "]";

		if(_nowPlayingBGM) Destroy(_nowPlayingBGM.gameObject);

		if(isLoop) {
			_fadeInAudio.loop = true;
		}
		else {
			Destroy(_fadeInAudio.gameObject, clip.length);
		}

		_nowPlayingBGM = _fadeInAudio;
	}

	/// <summary>
	/// フェードアウトするためのコルーチン
	/// </summary>
	/// <param name="fadeTime"></param>
	/// <returns></returns>
	private IEnumerator FadeOutAnim(float fadeTime) {

		var src = _nowPlayingBGM;

		//フェードイン中にフェードアウトが呼ばれた場合
		if (!src) {
			//フェードイン処理停止
			if(_fadeInCoroutine == null) yield break;
			Instance.StopCoroutine(_fadeInCoroutine);
			src = _fadeInAudio;

			if(!src) yield break;
		}

		src.name = "[Audio BGM - " + _latestPlayBGM + " - FadeOut ]";
		_nowPlayingBGM = null;

		//フェードアウト
		var t = 0.0f;
		var vol = src.volume;
		while((t += Time.deltaTime / fadeTime) < 1.0f) {
			src.volume = (1 - t) * vol;
			yield return null;
		}

		Destroy(src.gameObject);
	}
}
