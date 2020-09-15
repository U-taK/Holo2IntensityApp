using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHiddenManager : MonoBehaviour
{
    [SerializeField]
    GameObject panel;
    [SerializeField]
    GameObject openButton;
    public void ClosePanel()
    {
        panel.SetActive(false);
        openButton.SetActive(true);
    }

    public void OpenPanel()
    {
        panel.SetActive(true);
        openButton.SetActive(false);
    }
}
