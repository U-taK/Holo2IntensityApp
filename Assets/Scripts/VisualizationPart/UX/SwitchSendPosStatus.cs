using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SwitchSendPosStatus : MonoBehaviour
{
    [SerializeField]
    TextMeshPro text;
    [SerializeField]
    MeshRenderer mrenderer;

    [SerializeField]
    Material suspend;
    [SerializeField]
    Material start;

    //Hand Menu アイコン
    //データ送信を一時停止＆再開
    public void SwitchSendPos()
    {
        UIManager._measure = !UIManager._measure;
        if (UIManager._measure)
        {
            text.text = "SendSuspend";
            mrenderer.material = suspend;
        }
        else
        {
            text.text = "SendStart";
            mrenderer.material = start;
        }
    }


}
