using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class VuforiaExtendedDelete : MonoBehaviour,ITrackableEventHandler
{
    MeshRenderer plane;
    MeshRenderer micObject;
    TrackableBehaviour behaviour;

    // Start is called before the first frame update
    void Start()
    {
        plane = GameObject.Find("Plane").GetComponent<MeshRenderer>();
        micObject = GameObject.Find("MicPosition").GetComponent<MeshRenderer>();
        behaviour = gameObject.GetComponent<ImageTargetBehaviour>();

        if (behaviour)
        {
            behaviour.RegisterTrackableEventHandler(this);
        }
    }

    // Update is called once per frame
    void Update()
    { 
    }

    public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus)
    {
        if(previousStatus == TrackableBehaviour.Status.EXTENDED_TRACKED || newStatus == TrackableBehaviour.Status.NO_POSE) 
        {
            plane.enabled = false;
            micObject.enabled = false;
        }
    }
}
