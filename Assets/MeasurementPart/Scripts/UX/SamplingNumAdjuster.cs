using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// サンプル数の数値調整
/// Asioの都合や計算の都合上,2のべき乗に調整
/// </summary>

public class SamplingNumAdjuster : MonoBehaviour
{
    InputField in_sNum;
    // Start is called before the first frame update
    void Start()
    {
        in_sNum = GetComponent<InputField>();
        in_sNum.contentType = InputField.ContentType.DecimalNumber;
        in_sNum.onEndEdit.AddListener(EndEdit);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //2のべき乗に変更
    public void EndEdit(string input)
    {
        var inputInt = int.Parse(input);
        int res = (int)Mathf.Log(inputInt, 2);
        in_sNum.text = Mathf.Pow(2, res).ToString(); 
    }
}
