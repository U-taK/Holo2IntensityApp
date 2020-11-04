using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimPanelManager : MonoBehaviour
{
    [SerializeField]
    GameObject animPanel;
    [SerializeField]
    AnimManager animManager;

    [SerializeField]
    TextMesh frameText;

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
        animPanel.SetActive(true);
        animManager.Initialize();
        onPanel = true;
    }

    public void CloseAnimPanel()
    {
        animManager.AnimationPanelClosed();
        animPanel.SetActive(false);
        onPanel = false;
    }
}
