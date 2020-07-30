using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpDownValueIP : MonoBehaviour
{
    TextMesh valueText;

    int interval = 1;
   // int num;
    // Start is called before the first frame update
    void Start()
    {
        valueText = this.GetComponent<TextMesh>();
    }

    // Update is called once per frame
    void Update()
    {
       // num = int.Parse(valueText.text);
    }

    public void OnUpButtonClicked()
    {
        int SetValue = int.Parse(valueText.text) + interval;
        valueText.text = SetValue.ToString();
    }
    public void OnDownButtonClicked()
    {
        int SetValue = int.Parse(valueText.text) - interval;
        valueText.text = SetValue.ToString();
    }
}
