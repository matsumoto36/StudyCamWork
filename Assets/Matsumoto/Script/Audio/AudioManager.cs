using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;
using System;

/// <summary>
/// 音の管理をする
/// </summary>
public sealed class AudioManager : SingletonMonoBehaviour<AudioManager> {

	const string MIXER_PATH = "Sound/MainAudioMixer";		//ミキサーのパス
	const string BGM_PATH = "Sound/BGM/";					//BGMのフォルダーパス
	const string SE_PATH = "Sound/SE/";                     //SEのフォルダーパス

	const string BGM_VOLUME_KEY = "BGMVolume";				//MixerからBGMの音量にアクセスするためのキー
	const string SE_VOLUME_KEY = "SEVolume";				//MixerからBGMの音量にアクセスするためのキー

	const int MAX_PLAYING_SE_COUNT = 10;					//同時再生できるSEの数

	AudioMixerGroup[] mixerGroups = new AudioMixerGroup[2]; //ミキサーのグループ [0]SE [1]BGM

	ObjectPooler poolSE;

	Dictionary<string, AudioClipInfo> SEclips;				//SE再生用リスト
	Dictionary<string, AudioClip> BGMclips;					//BGM再生用リスト

	AudioSource nowPlayingBGM;                              //現在再生されているBGM
	string latestPlayBGM;									//再生されているBGMの種類

	Coroutine fadeInCol;									//BGMフェードインのコルーチン
	AudioSource fadeInAudio;                                //BGMフェードイン用のAudioSource

	public AudioMixer Mixer { get; private set; }           //ミキサー

	public static float SEVolume {
		get {
			float volume = 0.0f;
			Instance.Mixer.GetFloat(SE_VOLUME_KEY, out volume);
			return volume;
		}
		set {
			Instance.Mixer.SetFloat(SE_VOLUME_KEY, value);
		}
	}

	public static float BGMVolume {
		get {
			float volume = 0.0f;
			Instance.Mixer.GetFloat(BGM_VOLUME_KEY, out volume);
			return volume;
		}
		set {
			Instance.Mixer.SetFloat(BGM_VOLUME_KEY, value);
		}
	}

	public static string CurrentBGMName {
		get {
			if(!Instance.nowPlayingBGM) return "";
			return Instance.latestPlayBGM;
		}
	}

	protected override void Init() {
		base.Init();

		Load();
	}

	static IEnumerator StopSEDelay(float delayTime, Action action) {
		yield return new WaitForSeconds(delayTime);
		action();
	}

	public static void ReleaseRawSE(AudioSource src) {
		src.name = "[Stopped]";
		src.Stop();
		ObjectPooler.ReleaseInstance(src.gameObject);
	}

	/// <summary>
	/// 各音情報を読み込み
	/// </summary>
	public static void Load() {

		//LoadMixer
		Instance.Mixer = Resources.Load<AudioMixer>(MIXER_PATH);
		if(Instance.Mixer) {
			Instance.mixerGroups[0] = Instance.Mixer.FindMatchingGroups("SE")[0];
			Instance.mixerGroups[1] = Instance.Mixer.FindMatchingGroups("BGM")[0];
		}
		else {
			Debug.LogError("Failed Load AudioMixer! Path=" + MIXER_PATH);
		}


		//BGM読み込み
		Instance.BGMclips = new Dictionary<string, AudioClip>();
		foreach(var item in Resources.LoadAll<AudioClip>(BGM_PATH)) {
			Instance.BGMclips.Add(item.name, item);
		}

		//SE読み込み
		Instance.SEclips = new Dictionary<string, AudioClipInfo>();
		foreach(var item in Resources.LoadAll<AudioClip>(SE_PATH)) {
			Instance.SEclips.Add(item.name, new AudioClipInfo(item));
		}

		//プールの作成
		Instance.poolSE = ObjectPooler.GetObjectPool(
			new GameObject("[Stopped]")
			.AddComponent<AudioSource>()
			.gameObject);

		Instance.poolSE.maxCount = 20;
		Instance.poolSE.prepareCount = 10;
		Instance.poolSE.Generate(Instance.transform);
	}

	/// <summary>
	/// SEを再生する
	/// </summary>
	/// <param name="type">SEの名前</param>
	/// <param name="vol">音量</param>
	public static AudioSource PlaySE(string SEName, float vol = 1.0f) {

		//SE取得
		var info = GetSEInfo(SEName);
		if(info == null) return null;

		if(info.stockList.Count > 0) {
			//stockListから空で且つ番号が一番若いSEInfoを受け取る
			var seInfo = info.stockList.Values[0];

			//ストックを削除
			info.stockList.Remove(seInfo.index);

			//情報を取り付ける
			var src = Instance.poolSE.GetInstance().GetComponent<AudioSource>();
			src.name = "[Audio SE - " + SEName + "]";
			src.transform.SetParent(Instance.transform);
			src.clip = info.clip;
			src.volume = seInfo.volume * vol;
			src.outputAudioMixerGroup = Instance.mixerGroups[0];
			src.Play();

			//遅延でストップする
			Instance.StartCoroutine(StopSEDelay(src.clip.length + 0.1f, () => {

				src.name = "[Stopped]";
				src.Stop();
				ObjectPooler.ReleaseInstance(src.gameObject);

				info.stockList.Add(seInfo.index, seInfo);
			}));

			return src;
		}

		return null;
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
		var src = Instance.poolSE.GetInstance().GetComponent<AudioSource>();
		src.name = "[Audio SE - " + SEName + "]";
		src.transform.SetParent(Instance.transform);
		src.clip = info.clip;
		src.volume = vol;
		src.outputAudioMixerGroup = Instance.mixerGroups[0];
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
		if(Instance.nowPlayingBGM) Destroy(Instance.nowPlayingBGM.gameObject);

		var src = new GameObject("[Audio BGM - " + BGMName + "]").AddComponent<AudioSource>();
		src.transform.SetParent(Instance.transform);
		src.clip = clip;
		src.volume = vol;
		src.outputAudioMixerGroup = Instance.mixerGroups[1];
		src.Play();

		if(isLoop) {
			src.loop = true;
		}
		else {
			Destroy(src.gameObject, clip.length + 0.1f);
		}

		Instance.nowPlayingBGM = src;
		Instance.latestPlayBGM = BGMName;

		return src;
	}

	/// <summary>
	/// BGMをフェードインさせる
	/// </summary>
	/// <param name="fadeTime">フェードする時間</param>
	/// <param name="type">新しいBGMのタイプ</param>
	/// <param name="vol">新しいBGMの大きさ</param>
	/// <param name="isLoop">新しいBGMがループするか</param>
	public static void FadeIn(float fadeTime, string BGMName, float vol = 1.0f, bool isLoop = true) {
		Instance.fadeInCol = Instance.StartCoroutine(Instance.FadeInAnim(fadeTime, BGMName, vol, isLoop));
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
	/// <param name="type">新しいBGMのタイプ</param>
	/// <param name="vol">新しいBGMの大きさ</param>
	/// <param name="isLoop">新しいBGMがループするか</param>
	public static void CrossFade(float fadeTime, string fadeInBGMName, float vol = 1.0f, bool isLoop = true) {
		Instance.StartCoroutine(Instance.FadeOutAnim(fadeTime));
		Instance.fadeInCol = Instance.StartCoroutine(Instance.FadeInAnim(fadeTime, fadeInBGMName, vol, isLoop));
	}

	/// <summary>
	/// SEを取得する
	/// </summary>
	/// <param name="SEName">SEの名前</param>
	/// <returns>SE</returns>
	static AudioClipInfo GetSEInfo(string SEName) {

		if(!Instance.SEclips.ContainsKey(SEName)) {
			Debug.LogWarning("SEName:" + SEName + " is not found.");
			return null;
		}
		return Instance.SEclips[SEName];
	}

	/// <summary>
	/// BGMを取得する
	/// </summary>
	/// <param name="BGMName">BGMの名前</param>
	/// <returns>BGM</returns>
	static AudioClip GetBGM(string BGMName) {

		if(!Instance.BGMclips.ContainsKey(BGMName)) {
			Debug.LogError("BGMName:" + BGMName + " is not found.");
			return null;
		}
		return Instance.BGMclips[BGMName];
	}

	IEnumerator FadeInAnim(float fadeTime, string BGMName, float vol, bool isLoop) {

		//BGM取得
		var clip = GetBGM(BGMName);
		if(!clip) yield break;

		//初期設定
		fadeInAudio = new GameObject("[Audio BGM - " + BGMName + " - FadeIn ]").AddComponent<AudioSource>();
		fadeInAudio.transform.SetParent(Instance.transform);
		fadeInAudio.clip = clip;
		fadeInAudio.volume = 0;
		fadeInAudio.outputAudioMixerGroup = mixerGroups[1];
		fadeInAudio.Play();

		latestPlayBGM = BGMName;

		//フェードイン
		var t = 0.0f;
		while((t += Time.deltaTime / fadeTime) < 1.0f) {
			fadeInAudio.volume = t * vol;
			yield return null;
		}

		fadeInAudio.volume = vol;
		fadeInAudio.name = "[Audio BGM - " + BGMName + "]";

		if(nowPlayingBGM) Destroy(nowPlayingBGM.gameObject);

		if(isLoop) {
			fadeInAudio.loop = true;
		}
		else {
			Destroy(fadeInAudio.gameObject, clip.length + 0.1f);
		}

		nowPlayingBGM = fadeInAudio;
	}

	IEnumerator FadeOutAnim(float fadeTime) {

		var src = nowPlayingBGM;

		//フェードイン中にフェードアウトが呼ばれた場合
		if (!src) {
			//フェードイン処理停止
			if(fadeInCol == null) yield break;
			Instance.StopCoroutine(fadeInCol);
			src = fadeInAudio;

			if(!src) yield break;
		}

		src.name = "[Audio BGM - " + latestPlayBGM + " - FadeOut ]";
		nowPlayingBGM = null;

		//フェードアウト
		var t = 0.0f;
		float vol = src.volume;
		while((t += Time.deltaTime / fadeTime) < 1.0f) {
			src.volume = (1 - t) * vol;
			yield return null;
		}

		Destroy(src.gameObject);
	}
}
