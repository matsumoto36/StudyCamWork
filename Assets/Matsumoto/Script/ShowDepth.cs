using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowDepth : MonoBehaviour {

	public Material mat;

	void Awake() {
		GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
	}

	protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination) {
		Graphics.Blit(source, destination, mat);
	}

}
