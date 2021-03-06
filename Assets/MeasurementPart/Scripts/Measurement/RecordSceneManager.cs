﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AsioCSharpDll;
using System.IO;
using System;

public class RecordSceneManager : MonoBehaviour
{
    //録音ボタン
    [SerializeField]
    Button rButton;

    //計測時間[s]
    [SerializeField]
    Text mTime;

    int fs = 44100;
    int sampleLength = 4096;

    //録音された音圧信号[マイクID][サンプル数]
    double[][] soundSignals;

    //録音可能か
    bool canRec = false;

    //接続断イベント
    public delegate void AsioReady(EventArgs e);
    public event AsioReady OnAsioReady;
    public event AsioReady AsioNotReady;


    // Start is called before the first frame update
    void Start()
    {
        //起動時は録音許可しない
        if (rButton != null)
            rButton.interactable = false;
    }

    //ボタンに紐づけ
    public void CalibReady()
    {
        //ドライバー名デバッグ
        string[] asioDriverIDNames = asiocsharpdll.GetAsioDriverNames();
        foreach (string asioDriverIDName in asioDriverIDNames)
        {
            Debug.Log(asioDriverIDName);
        }

        fs = MeasurementParameter.Fs;
        sampleLength = MeasurementParameter.SampleNum;

        //Asioスタート
        //string canStart = asiocsharpdll.PrepareAsio("MOTU Pro Audio", fs, sampleLength);
        //string canStart = asiocsharpdll.PrepareAsio("ASIO4ALL v2", fs, sampleLength);
        string canStart = asiocsharpdll.PrepareAsio(MeasurementParameter.AsioDriverName, MeasurementParameter.Fs, MeasurementParameter.SampleNum);
        Debug.Log(canStart);
        if (canStart == "Asio start")
        {
            OnAsioReady(new EventArgs());
            rButton.interactable = true;
            canRec = true;
        }
        else
        {
            AsioNotReady(new EventArgs());
        }

    }
    // Update is called once per frame
    void Update()
    {
        
    }

    //ボタンに紐づけ
    /// <summary>
    /// Binaryデータに4点マイクの録音データを保存
    /// </summary>
    public void Record2Binary()
    {
        rButton.interactable = false;
        soundSignals = asiocsharpdll.GetAsioSoundSignals(sampleLength);

        Debug.Log("Save Bytes Data");
        SaveSound2Binary(soundSignals, Application.streamingAssetsPath);
        rButton.interactable = true;
    }

    /// <summary>
    /// Wavファイルに1点のマイクから得られた録音データを保存
    /// </summary>
    public void Record2wav()
    {
        //ディレクトリなかったら作成
        SafeCreateDirectory(Application.streamingAssetsPath);

        //録音＆マイク位置バイナリファイル保存
        string fileName = @"CalibValues.wav";

        if (mTime != null)
            asiocsharpdll.StartAsioRecord(fileName, int.Parse(mTime.text));
        else
            asiocsharpdll.StartAsioRecord(fileName, 3);
    }

    private void OnDestroy()
    {

        Debug.Log("Return home scene");
        if (canRec)
            asiocsharpdll.StopAsioMain();
    }

    /// <summary>
    /// バイナリデータセーブ
    /// </summary>
    void SaveSound2Binary(double[][] soundSignals, string saveDirPath)
    {
        //ディレクトリなかったら作成
        SafeCreateDirectory(saveDirPath);

        //録音＆マイク位置バイナリファイル保存
        string Filteredname = saveDirPath + @"\CalibValues.bytes";
        FileStream fs = new FileStream(Filteredname, FileMode.Create);
        BinaryWriter bw = new BinaryWriter(fs);
        for (int micId = 0; micId < 4; micId++)
        {
            for (int sample = 0; sample < sampleLength; sample++)
            {
                bw.Write(soundSignals[micId][sample]);
            }
        }
        bw.Close();
        fs.Close();
    }

    /// <summary>
    /// 指定したパスにディレクトリが存在しない場合
    /// すべてのディレクトリとサブディレクトリを作成
    /// </summary>
    public static DirectoryInfo SafeCreateDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            return null;
        }
        return Directory.CreateDirectory(path);
    }
}
