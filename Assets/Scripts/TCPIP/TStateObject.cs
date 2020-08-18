///<summary>
///寺岡さんの作成したStageObject
/// </summary>


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net.Sockets;

public class TStateObject
{
    /** singleton **/
    private static TStateObject _stateObject = new TStateObject();
    public static TStateObject tstateObject { get { return _stateObject; } }
    private TStateObject() { }
    //TODO:singletonじゃなくてもいい気がする

    /** メンバ **/
    //Client Socket
    public Socket workSocket { get; set; } = null;
    //受信素データバッファサイズ
    //多めに16384ぐらい取ってみる
    public const int BufferSize = 16384;
    //受信素データ
    public byte[] buffer { get; set; } = new byte[BufferSize];
    //TODO:受信データオブジェクトあるから淘汰できるかも
    //色データ表示用que
    public Queue<float> que { get; set; } = new Queue<float>();
    //色データque安全？、マルチプロセスで使ってたやつ
    //public BlockingCollection<float> Que = new BlockingCollection<float>(BufferSize);
    //表示データ縦横長さ
    public int colorWidth { get; private set; }
    public int colorHight { get; private set; }
    //色データ受信時に千切れたデータ
    private string shortage = null;
    //設定情報受信したらtrue
    private bool gotData = false;

    //設定情報オブジェクト、一応初期状態付き
    public class SettingsData
    {
        //計測開始/停止
        public bool startMeasure = false;
        //データ保存開始/停止
        public bool saveData = false;
        //バンドパスフィルタ上限
        public float bpf_min = 0f;
        //バンドパスフィルタ下限
        public float bpf_max = 1f;
        //現在の状態？
        public string state = "Disconnected";
    }

    //色データオブジェクト
    public class ColorData
    {
        public float[] colorData;
    }

    /** プロパティ **/
    /// <summary>
    /// workSocket閉じ
    /// </summary>
    public void CloseWorkSocket()
    {
        if (workSocket != null)
        {
            try
            {
                workSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                workSocket.Close();
                workSocket = null;
                Debug.Log("workSocket閉じたよ");
            }
        }
        else Debug.Log("workSocket閉じてたよ");
    }

    /* 送信系 */
    /// <summary>
    /// UIの値からSettingsDataオブジェクトをセット
    /// </summary>
	/// <param name="settings">設定情報オブジェクト</param>
	/// <param name="measureStart">計測開始/停止toggle</param>
	/// <param name="saveStart">保存開始/停止toggle</param>
	/// <param name="bpf_min">BPF下限inputfield</param>
	/// <param name="bpf_max">BPF上限inputfield</param>
	/// <param name="state">状態inputfield</param>
    public void UIParams2SettingsObj(ref SettingsData settings, Toggle measureStart,
        Toggle saveStart, InputField bpf_min, InputField bpf_max, InputField state)
    {
        settings.startMeasure = measureStart.isOn;
        settings.saveData = saveStart.isOn;
        if (float.TryParse(bpf_min.text, out float bpfmin))
            settings.bpf_min = bpfmin;
        else
        {
            Debug.Log("BPF_minにはfloatの値入れて");
            return;
        }
        if (float.TryParse(bpf_max.text, out float bpfmax))
            settings.bpf_max = bpfmax;
        else
        {
            Debug.Log("BPF_maxにはfloatの値入れて");
            return;
        }
        settings.state = state.text;
    }

    /// <summary>
    /// SettingsDataオブジェクト->送信データ
    /// </summary>
	/// <param name="settings">設定情報オブジェクト</param>
	/// <returns>json形式送信データ</returns>
	public string SettingsObj2SendJson(SettingsData settings)
    {
        //json形式にシリアライズ
        string json = JsonUtility.ToJson(settings);
        Debug.Log(json + "を送信するよ");
        return json;
    }

    /* 受信系 */
    /// <summary>
	/// 受信jsonデータ->受信データオブジェクト、que
	/// </summary>
	/// <param name="data">受信データ</param>
	/// <param name="rd">受信データ格納用オブジェクト</param>
    public void ReceivedJson2ReceivedObj(string data, ref ColorData rd)
    {
        if (shortage == null)
        {
            try
            {
                rd = JsonUtility.FromJson<ColorData>(data);
                for (int i = 0; i < rd.colorData.Length; i++)
                    que.Enqueue(rd.colorData[i]);
            }
            //Missing a comma or ']' after an array element.対策
            catch (Exception e)
            {
                shortage = data;
            }
        }
        else
        {
            //くっつけてフルデータ！
            shortage += data;
            try
            {
                rd = JsonUtility.FromJson<ColorData>(shortage);
                //フルデータできたらqueに追加
                for (int i = 0; i < rd.colorData.Length; i++)
                    que.Enqueue(rd.colorData[i]);
                shortage = null;
            }
            //まだ千切れてたら次の受信待ち
            catch (Exception e2)
            {
                //The document root must not follow by other values.対策
                //受信データが壊れちゃってる時は末尾が'}'だったら今までのなしにして次の受信へ
                if (shortage[shortage.Length - 1] == '}')
                    shortage = null;
            }
        }
    }

    /// <summary>
	/// 受信jsonデータ->設定情報オブジェクト
	/// </summary>
	/// <param name="data">受信データ</param>
	/// <param name="settings">設定情報オブジェクト</param>
    public void ReceivedJson2SettingsObj(string data, ref SettingsData settings)
    {
        Debug.Log(data + "を受信したよ");
        try
        {
            //デシリアライズ
            settings = JsonUtility.FromJson<SettingsData>(data);
            gotData = true;
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    /// <summary>
	/// 設定情報オブジェクトをUIに反映
	/// </summary>
	/// <param name="settings">設定情報オブジェクト</param>
	/// <param name="measureStart">計測開始/停止toggle</param>
	/// <param name="saveStart">保存開始/停止toggle</param>
	/// <param name="bpf_min">BPF下限inputfield</param>
	/// <param name="bpf_max">BPF上限inputfield</param>
	/// <param name="state">状態inputfield</param>
    public void SettingsObj2UIParams(SettingsData settings, Toggle measureStart,
        Toggle saveStart, InputField bpf_min, InputField bpf_max, InputField state)
    {
        if (gotData)
        {
            measureStart.isOn = settings.startMeasure;
            saveStart.isOn = settings.saveData;
            bpf_min.text = settings.bpf_min.ToString();
            bpf_max.text = settings.bpf_max.ToString();
            state.text = settings.state;

            gotData = false;
        }
    }
}
