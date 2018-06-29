﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ResultUI : MonoBehaviour {
    public Text ResultScoreText;
    public Text ResultaccText;
    public Text ResultcomboText;

    public int ResultScore;
    public float ResultAccuracy;
    public int ResultCombo;

    // Use this for initialization
    void Start ()
    {
       
    }
	
	// Update is called once per frame
	void Update ()
    {
        ResultScore = MouseCamera.Score;
        ResultCombo = MouseCamera.ComboMax;
        ResultAccuracy = MouseCamera.Accuracy;

        ResultScoreText.text = ResultScore.ToString();
        if (ResultAccuracy == 1.0f) ResultaccText.text = "100%";
        else ResultaccText.text = ResultAccuracy.ToString("P");
        ResultcomboText.text = "x" + ResultCombo.ToString("");
    }
}
