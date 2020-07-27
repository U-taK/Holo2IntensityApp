using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OriginChaser : MonoBehaviour
{
    [SerializeField]
    GameObject standardMarker;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (standardMarker != null)
        {
            this.transform.position = standardMarker.transform.position;
            this.transform.localRotation = standardMarker.transform.localRotation;
        }
    }
}
