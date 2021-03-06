﻿/////////////////////////
///送受信するデータを管理
/////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransferData
{
    private static TransferData _trasferData = new TransferData();
    public static TransferData transferData {get { return _trasferData; } }
    private TransferData() { }
    //千切れたデータを保持
    private string shortage = null;
    //情報受信したらtrue
    private bool gotData = false;
    //仮に置く場所
    private string instantJson;

    ///受信データから複数のjsonファイルに変換
    public string[] DevideData2Jsons(string ms)
    {
        var wordList = ms.Replace("\r\n", "\n").Split(new[] { '\n', '\r' });
        List<string> messages = new List<string>();
        foreach (var word in wordList)
        {
            if (word != "")
                messages.Add(word);
        }
        return messages.ToArray();
    }

    ///<summary>
    ///特定の型をシリアライズ
    ///送信するデータにキーワードを含む
    /// </summary>
    /// <param name="data">送信データをシリアライズ</param>
    public string SerializeJson<T>(T data)
    {
        //json形式にシリアライズ
        string json = JsonUtility.ToJson(data);
        Debug.Log(json + "を送信");
        return json;
    }

    ///<summary>
    ///受信したデータをデシリアライズ
    ///デシリアライズが終了するとtrueを返す
    /// </summary>
    public bool CanDesirializeJson<T>(string json, out T data)
    {
        if(shortage == null)
        {
            try
            {
                data = JsonUtility.FromJson<T>(json);
                instantJson = json;
                return true;
            }
            //Missing a comma or ']' after an array element.対策
            catch (Exception e)
            {
                shortage = json;

            }
        }
        else
        {
            //保持したデータを組み合わせる
            shortage += json;
            try
            {
                data = JsonUtility.FromJson<T>(shortage);
                instantJson = shortage;
                shortage = null;
                return true;
            }
            catch (Exception e)
            {
                
            }
        }

        data = default;
        return false;
    }

    public void DesirializeJson<T>(out T data)
    {
        data = JsonUtility.FromJson<T>(instantJson);
        instantJson = null;
    }

    //データが欠損しクライアントからサーバにデータを送っても反応がなくなった際に押す
    public void CleanStorage()
    {
        shortage = null;
    }
}
