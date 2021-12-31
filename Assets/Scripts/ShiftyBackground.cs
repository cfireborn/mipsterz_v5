using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShiftyBackground : MonoBehaviour
{
    [SerializeField]
    public Image BackgroundImage;
    
    Gradient gradient;
    
    [SerializeField]
    public GradientColorKey[] colorKey;
    [SerializeField]
    public GradientAlphaKey[] alphaKey;

    [SerializeField] 
    public Color ColorStart = Color.red;
    [SerializeField] 
    public Color ColorEnd = Color.blue;

    [SerializeField] 
    public float AlphaStart = 1.0f;
    [SerializeField] 
    public float AlphaEnd = 0.0f;
    
    [SerializeField] 
    public float CycleTime = 3.0f;

    public bool ReverseOnEnd = false;

    // Start is called before the first frame update
    void Start()
    {
        gradient = new Gradient();

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        colorKey = new GradientColorKey[2];
        colorKey[0].color = ColorStart;
        colorKey[0].time = 0.0f;
        colorKey[1].color = ColorEnd;
        colorKey[1].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = AlphaStart;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = AlphaEnd;
        alphaKey[1].time = 1.0f;

        gradient.SetKeys(colorKey, alphaKey);

        // What's the color at the relative time 0.25 (25 %) ?
        Debug.Log(gradient.Evaluate(0.25f));
    }


    // Update is called once per frame
    void Update()
    {
        if (BackgroundImage)
        {
            if (ReverseOnEnd)
            {
                BackgroundImage.color = gradient.Evaluate(Math.Abs(((Time.timeSinceLevelLoad % (CycleTime*2)) / CycleTime) - 1.0f)) ;
            }
            else
            {
                BackgroundImage.color = gradient.Evaluate((Time.timeSinceLevelLoad % CycleTime) / CycleTime);
            }
        }
    }
}
