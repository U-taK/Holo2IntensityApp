using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpDownValueIP : MonoBehaviour
{
    TextMesh valueText;

    int interval = 1;

    bool onHold = false;
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

    public void OnDownButtonHolded()
    {
        onHold = true;
        StartCoroutine("OnHoldCoroutine", false);
    }

    public void OnDownButtonReleased()
    {
        onHold = false;
    }

    public void OnUpButtonHolded()
    {
        onHold = true;
        StartCoroutine("OnHoldCoroutine", true);
    }

    public void OnUpButtonReleased()
    {
        onHold = false;
        Debug.Log("released");
    }

    /// <summary>
    /// ボタンを押しっぱなしの際の挙動
    /// </summary>
    /// <param name="up"></param>
    /// <returns></returns>
    IEnumerator OnHoldCoroutine(bool up)
    {
        float accel = 0.5f;
        int counter = 0;
        while (onHold) 
        {
            if (up)
            {
                OnUpButtonClicked();
            }
            else
            {
                OnDownButtonClicked();
            }
            counter++;
            if (counter > 4)
            {
                accel -= 0.3f;
            }
            yield return new WaitForSeconds(accel);
        }

        
    }


}
