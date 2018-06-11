using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPKFXDepth : MonoBehaviour {

	public PKFxRenderingPlugin plugin;
	Renderer myRenderer;

	// Use this for initialization
	void Start () {
		myRenderer = GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {
		myRenderer.material.SetTexture(
			"_MainTex",
			plugin.m_DepthRT
			);
	}
}
