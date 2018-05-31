using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 0 ~ 1で表示するオブジェクト(ギミック用)
/// </summary>
public class GimmickGauge : MonoBehaviour {

	[SerializeField]
	GameObject gauge;

	[SerializeField]
	GameObject gaugeFill;

	Renderer gaugeRenderer;

	float v;
	public float Value {
		get { return v; }
		set {
			v = Mathf.Clamp(value, 0, 1);

			if(gauge) {
				var scale = gauge.transform.localScale;
				scale.y = v;

				gauge.transform.localScale = scale;
			}
				
		}
	}

	public Color GaugeColor {
		get { return gaugeRenderer.material.GetColor("_Emission"); }
		set { gaugeRenderer.material.SetColor("_Emission", value); }
	}

	void Awake() {
		gaugeRenderer = gaugeFill.GetComponent<Renderer>();
		gaugeRenderer.material = new Material(gaugeRenderer.material);

		gaugeRenderer.material.EnableKeyword("_EMISSION");
	}
}
