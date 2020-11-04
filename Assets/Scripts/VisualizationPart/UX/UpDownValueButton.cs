using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class MyfloatEvent: UnityEvent<float>
{

}

public class UpDownValueButton : MonoBehaviour
{
    TextMesh valueText;

    float interval = 5f;
    float num;

    [SerializeField, Tooltip("パラメータ変更時実行処理")]
    private MyfloatEvent OnValueChanged;

    // Start is called before the first frame update
    void Start()
    {
        valueText = this.GetComponent<TextMesh>();    
    }

    // Update is called once per frame
    void Update()
    {
        num = float.Parse(valueText.text);
        if (num > 10)
            interval = 5;
        else if (num > 1 && num <= 10)
            interval = 1;
        else if (num > 0.1 && num <= 1)
            interval = 0.1f;
        else if (num > 0.01 && num <= 0.1)
            interval = 0.01f;
        else if (num < 0.01)
            interval = 0.001f;
    }

    public void OnUpButtonClicked()
    {
        float setValue = float.Parse(valueText.text) + interval;
        valueText.text = setValue.ToString("f3");
        OnValueChanged.Invoke(setValue);
    }

    public void OnDownButtonClicked()
    {
        float setValue = float.Parse(valueText.text) - interval;
        valueText.text = setValue.ToString("f3");
        OnValueChanged.Invoke(setValue);
    }
}
