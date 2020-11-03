using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntensityObject : MonoBehaviour
{
    //public List<Vector3> tranIntensity;
    //public List<Color> colors;

    public Vector3[] tranIntensity;
    public Color[] colors;
    public Vector3[] scales;
    public GameObject child;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowAnimation(int k)
    {
        Debug.Log("ShowAnimation");
        if (tranIntensity[k].x != 0)
        {
            child.transform.localRotation = Quaternion.LookRotation(10000000000 * tranIntensity[k]);
            child.transform.GetComponent<Renderer>().material.color = colors[k];
            child.transform.localScale = scales[k]*Holo2MeasurementParameter.ObjSize;                
        }
        else
        {
            child.transform.localScale = Vector3.zero;
        }
    }

    public void PushInstantIntensity(Vector3[] insIntensity, List<Color> colors, List<Vector3> scales)
    {
        this.tranIntensity = insIntensity;
        this.colors = colors.ToArray();
        this.scales = scales.ToArray();
    }
}
