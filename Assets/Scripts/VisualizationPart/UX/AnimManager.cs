using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AnimManager : MonoBehaviour
{
    IntensityObject[] intObjs;
    bool playNow = false; //再生状態を管理
    bool suspend = false; //一時停止の管理
    bool forced_quit = false; //パネルを閉じるときに呼ぶ
    int nowframe = 0;//現在表示しているフレーム数
    float updatefreq = 0.1f;//フレーム更新速度
    int speed = 1;//更新時に進むフレーム数
    float sec = 0; //現在表示指定秒数(描画用)
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize()
    {
        nowframe = 0;
        updatefreq = 0.1f;
        speed = 1;
        playNow = false;
        forced_quit = false;
        suspend = false;
    }

    public void AnimationStart()
    {
        //最初に押す(アニメーション開始)
        if (!playNow)
            StartCoroutine("ShowAnim");
        //一時停止解除
        if (suspend)
            suspend = false;
        speed = 1;
    }

    //瞬時音響インテンシティアニメーション再生時は常に更新され続ける
    private IEnumerator ShowAnim()
    {
        Debug.Log("Animation Start");
        intObjs = GetComponentsInChildren<IntensityObject>();
        playNow = true;//状態遷移
        while (nowframe >= 0 && nowframe < intObjs[0].tranIntensity.Length)
        {
            if (forced_quit)
                yield break;
            foreach(IntensityObject ins in intObjs)
            {
                ins.ShowAnimation(nowframe);

            }
            nowframe+=speed;
            yield return new WaitForSeconds(updatefreq);
            while (suspend)//一時停止時は更新を止める
            {
                yield return new WaitForSeconds(updatefreq);
                if (forced_quit)
                    yield break;
            }
        }
        playNow = false;        

    }

    /// <summary>
    /// 一時停止ボタンに付与
    /// </summary>
    public void AnimationSuspend()
    {
        suspend = true;
    }

    /// <summary>
    /// 倍速ボタンに付与
    /// </summary>
    public void AnimationFastForward()
    {
        speed *= 2;
    }

    /// <summary>
    /// 逆再生ボタンに付与
    /// </summary>
    public void AnimationBackStart()
    {
        if (suspend)
            suspend = false;
        speed = -1;
    }

    /// <summary>
    /// 逆倍速ボタンに付与
    /// </summary>
    public void AnimationBackForward()
    {
        if (speed < 0)
            speed *= 2;
        else
            speed = -1;
    }

    /// <summary>
    /// アニメーションパネルを閉じる際に呼び出す
    /// </summary>
    public void AnimationPanelClosed()
    {
        forced_quit = true;
        intObjs = GetComponentsInChildren<IntensityObject>();
        //平均値を改めて表示する
        foreach (IntensityObject ins in intObjs)
        {
            Debug.Log("ShowAnim" + nowframe);
            ins.ShowAveraged();
        }
        playNow = false;
    }


    /// <summary>
    /// 更新速度パラメータの更新
    /// </summary>
    /// <param name="uf">更新速度パラメータのtextファイル</param>
    public void UpdateUF(float uf)
    {
        this.updatefreq = uf;
        Debug.Log("updatefreq: " + updatefreq.ToString());
    }

    public string UpdateDisplay()
    {
        sec = (float)nowframe / Holo2MeasurementParameter.fs;
        return $"frame: {nowframe}\n sec: {sec.ToString("F5")}";
    }
}
