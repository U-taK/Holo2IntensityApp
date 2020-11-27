using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public class AnimPanelManager : MonoBehaviour
{
    [SerializeField]
    GameObject animPanel;
    [SerializeField]
    AnimManager animManager;

    [SerializeField]
    TextMesh frameText;

    //ユーザー追従するスクリプト
    [SerializeField]
    RadialView radialView;

    bool onPanel = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (onPanel)
        {
            frameText.text =  animManager.UpdateDisplay();
        }   
    }

    public void OpenAnimPanel()
    {
        if (Holo2MeasurementParameter.measurementType == MeasurementType.Transient)
        {
            animPanel.SetActive(true);
            radialView.enabled = true;
            animManager.Initialize();
            onPanel = true;
        }
    }

    public void CloseAnimPanel()
    {
        animManager.AnimationPanelClosed();
        radialView.enabled = false;
        animPanel.SetActive(false);        
        onPanel = false;
    }
}
