using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEffect : MonoBehaviour {

	public Material mat;
	public float fade;
	public new float light;
	public float wave;
	public float animSpeed;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnRenderImage(RenderTexture src, RenderTexture dest) {

		mat.SetFloat("_Fade", fade);
		mat.SetFloat("_Light", light);
		mat.SetFloat("_Wave", wave);
		mat.SetFloat("_AnimSpeed", animSpeed);

		Graphics.Blit(src, dest, mat);
	}
}
