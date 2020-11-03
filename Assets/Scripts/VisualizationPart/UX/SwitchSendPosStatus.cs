using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

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
    [SerializeField]
    Interactable interactable_trigger;

    //Hand Menu アイコン
    //データ送信を一時停止＆再開
    public void SwitchSendPos()
    {
        UIManager._measure = !UIManager._measure;
        if (UIManager._measure)
        {
            text.text = "SendSuspend";
            mrenderer.material = suspend;
            if (Holo2MeasurementParameter.measurementType == MeasurementType.Transient)
                interactable_trigger.IsEnabled = true;
        }
        else
        {
            text.text = "SendStart";
            mrenderer.material = start;
            if (Holo2MeasurementParameter.measurementType == MeasurementType.Transient)
                interactable_trigger.IsEnabled = false;
        }
    }


}
