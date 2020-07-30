using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AIMath
{
    public static float CalcuIntensityLevel(UnityEngine.Vector3 intensity)
    {
        float intensityNorm = intensity.magnitude;
        float intensityLevel = 10f * Mathf.Log10(intensityNorm / Mathf.Pow(10f, -12f));
        return intensityLevel;
    }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
