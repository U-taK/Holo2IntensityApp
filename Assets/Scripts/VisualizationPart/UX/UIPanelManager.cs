using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPanelManager : MonoBehaviour
{

    enum measureType
    {
        start, shared, measure
    }

    measureType type = measureType.start;

    [SerializeField]
    GameObject startPanel;
    [SerializeField]
    GameObject sharedPanel;
    [SerializeField]
    GameObject measurePanel;
    [SerializeField]
    GameObject loading;
    [SerializeField]
    GameObject readyPanel;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //スタートパネルの動作
    public void SwitchMeasure()
    {
        startPanel.SetActive(false);
        measurePanel.SetActive(true);
        type = measureType.measure;

    }

    public void SwitchShared()
    {
        startPanel.SetActive(false);
        sharedPanel.SetActive(true);
        type = measureType.shared;

    }

    //スタートパネルに戻る
    public void ReturnStart()
    {
        startPanel.SetActive(true);
        measurePanel.SetActive(false);
        sharedPanel.SetActive(false);
        type = measureType.start;
    }

    //観測側が計測側の条件を受け取ったとき
    public void ReadyShare()
    {
        loading.SetActive(false);
        readyPanel.SetActive(true);
    }

    //観測側の観測スタート
    public void StartMeasure()
    {
        this.gameObject.SetActive(false);
        if (type == measureType.measure)
        {
            //UDPの場合
            UIManager._measure = true;
            //TCPの場合
            Holo2MeasurementParameter._measure = true;
        }
        //UDPの場合
        UIManager._instance = true;
        //TCPの場合
        Holo2MeasurementParameter._instance = true;
    }

}
