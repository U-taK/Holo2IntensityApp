using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using AsioCSharpDll;
using System.IO;

public class IntensityManager : MonoBehaviour
{

    //PC版生成用プレハブ
    [SerializeField]
    GameObject Cone;
    //空間基準マーカプレハブ
    [SerializeField]
    GameObject copyStandard;

    //インテンシティオブジェクトを作成しておく
    List<DataStorage> dataStorages = new List<DataStorage>();

    //生成オブジェクトの管理
    Dictionary<int, GameObject> intensities = new Dictionary<int, GameObject>();
    //送信データの管理番号
    int Num = 0;

    int length_bit;


    //テスト用変数
    Vector3 testPos = Vector3.zero;
    Quaternion testRot = Quaternion.Euler(0, 0, 0);
    int testNum = 0;
    // Start is called before the first frame update
    void Start()
    {
        length_bit = (int)(Mathf.Log(MeasurementParameter.SampleNum, 2f));
    }

    // Update is called once per frame
    void Update()
    {
        /*テストコード
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            MeasurementParameter.ObjInterval = 0.05f;
            testPos += Vector3.right * testNum * 0.1f;

            var sendPosition = new SendPosition(testNum.ToString(), testPos, testRot);
            MicPosReceivedTester(sendPosition);
            testNum++;
        }*/
    }

    public IntensityPackage MicPosReceived(SendPosition sendPosition)
    {
        if (CheckPlotDistance(sendPosition.sendPos))
        {
            GameObject micPoint = new GameObject("measurementPoint" + Num);
            micPoint.transform.parent = copyStandard.transform;
            micPoint.transform.localPosition = sendPosition.sendPos;
            micPoint.transform.localRotation = sendPosition.sendRot;
            // 音声取得
            var soundSignals = asiocsharpdll.GetAsioSoundSignals(MeasurementParameter.SampleNum, MeasurementParameter.CalibValue);
            //intensity計算
            var intensityDir = AcousticMathNew.CrossSpectrumMethod(soundSignals, MeasurementParameter.Fs, length_bit,
                MeasurementParameter.FreqMin, MeasurementParameter.FreqMax, MeasurementParameter.AtmDensity, MeasurementParameter.MInterval);
            float intensityLv_dB = AcousticMathNew.CalcuIntensityLevel(intensityDir);

            //オブジェクト作成
            var vectorObj = Instantiate(Cone) as GameObject;
            vectorObj.transform.localScale = new Vector3(MeasurementParameter.objSize, MeasurementParameter.objSize, MeasurementParameter.objSize * 4);
            vectorObj.transform.parent = micPoint.transform;
            vectorObj.transform.localPosition = Vector3.zero;
            vectorObj.transform.localRotation = Quaternion.LookRotation(10000000000 * intensityDir);
            var vecColor = ColorBar.DefineColor(MeasurementParameter.colormapID, intensityLv_dB, MeasurementParameter.MinIntensity, MeasurementParameter.MaxIntensity);
            vectorObj.transform.GetComponent<Renderer>().material.color = vecColor;
            vectorObj.name = "IntensityObj";
            intensities.Add(Num, vectorObj);

            //データそのものを保管
            DataStorage data = new DataStorage(Num, sendPosition.sendPos, sendPosition.sendRot, soundSignals, intensityDir);
            dataStorages.Add(data);

            //送信データを作成

            Num++;
            return new IntensityPackage(sendPosition, intensityDir, Num);
        }
        else
        {
            return new IntensityPackage();
        }
    }
    /// <summary>
    /// テストコード
    /// </summary>
    /// <param name="sendPosition"></param>
    public void MicPosReceivedTester(SendPosition sendPosition)
    {
        if (CheckPlotDistance(sendPosition.sendPos))
        {
            GameObject micPoint = new GameObject("measurementPoint" + Num);
            micPoint.transform.parent = copyStandard.transform;
            micPoint.transform.localPosition = sendPosition.sendPos;
            micPoint.transform.localRotation = sendPosition.sendRot;

            //intensity計算(テストコード)
            var intensityDir = Vector3.one;
            float intensityLv_dB = 82f;

            //オブジェクト作成
            var vectorObj = Instantiate(Cone) as GameObject;
            vectorObj.transform.localScale = new Vector3(MeasurementParameter.objSize, MeasurementParameter.objSize, MeasurementParameter.objSize * 4);
            vectorObj.transform.parent = micPoint.transform;
            vectorObj.transform.localPosition = Vector3.zero;
            vectorObj.transform.localRotation = Quaternion.LookRotation(10000000000 * intensityDir);
            var vecColor = ColorBar.DefineColor(MeasurementParameter.colormapID, intensityLv_dB, MeasurementParameter.MinIntensity, MeasurementParameter.MaxIntensity);
            vectorObj.transform.GetComponent<Renderer>().material.color = vecColor;
            vectorObj.name = "IntensityObj";
            intensities.Add(Num, vectorObj);

            //データそのものを保管
            double[][] soundSignals = new double[4][];
            DataStorage data = new DataStorage(Num, sendPosition.sendPos, sendPosition.sendRot, soundSignals, intensityDir);
            dataStorages.Add(data);

            //送信データを作成

            Num++;
        }
    }

    //ほかのプロットと距離が離れているかチェック
    //当均等に配置するために精度要素を追加
    bool CheckPlotDistance(Vector3 realtimeLocalPosition)
    {
        int plotNum = dataStorages.Count;
        //一個目だったらtrue
        if (plotNum == 0)
        {
            return true;
        }
        else
        {
            for (int index = 0; index < plotNum; index++)
            {
                if (!dataStorages[index].CheckPlotDistance(realtimeLocalPosition, MeasurementParameter.ObjInterval)) 
                    return false;
            }
            return true;
        }
    }    
    public async Task<ReCalcDataPackage> RecalcIntensity()
    {
        //データの更新
        ReCalcDataPackage recalcIntensity = await Task.Run(() => AsyncReCalc());

        //送信データの更新
        StartCoroutine("UpdateEditor",recalcIntensity);

        return recalcIntensity;
    }

    private ReCalcDataPackage AsyncReCalc()
    {
        //送信データの作成
        ReCalcDataPackage data = new ReCalcDataPackage(dataStorages.Count);
        foreach(DataStorage dataStorage in dataStorages)
        {
            Vector3 intensity = AcousticMathNew.CrossSpectrumMethod(dataStorage.soundSignal, MeasurementParameter.Fs, length_bit,
                MeasurementParameter.FreqMin, MeasurementParameter.FreqMax, MeasurementParameter.AtmDensity, MeasurementParameter.MInterval);
            data.intensities.Add(intensity);
            data.sendNums.Add(dataStorage.measureNo);
        }
        return data;
    }

    /// <summary>
    /// 更新インテンシティデータをエディタ上でも反映させていくようにする
    /// </summary>
    /// <param name="reCalcDataPackage">更新データ</param>
    /// <returns></returns>
    private IEnumerator UpdateEditor(ReCalcDataPackage reCalcDataPackage)
    {
        for(int i = 0; i < reCalcDataPackage.storageNum; i++)
        {
            if (intensities.ContainsKey(reCalcDataPackage.sendNums[i]))
            {
                var pushObj = intensities[reCalcDataPackage.sendNums[i]];
                //色変更を行う
                pushObj.transform.localRotation = Quaternion.LookRotation(10000000000 * reCalcDataPackage.intensities[i]);
                var lv = AcousticMathNew.CalcuIntensityLevel(reCalcDataPackage.intensities[i]);
                pushObj.transform.GetComponent<Renderer>().material.color = ColorBar.DefineColor(MeasurementParameter.colormapID, lv, MeasurementParameter.MinIntensity, MeasurementParameter.MaxIntensity);
                yield return null;
            }
        }
        yield return null;
    }

    /// <summary>
    /// バイナリデータセーブ
    /// </summary>    
    public async void SaveBinaryData()
    {
        await Task.Run(() => Save());

    }
    async Task Save()
    {
        Debug.Log(MeasurementParameter.SaveDir);
        //ディレクトリなかったら作成
        SafeCreateDirectory(MeasurementParameter.SaveDir);

        //録音＆マイク位置バイナリファイル保存
        for (int dataIndex = 0; dataIndex < dataStorages.Count; dataIndex++)
        {
            string Filteredname = MeasurementParameter.SaveDir + @"\measurepoint_" + (dataIndex + 1).ToString() + ".bytes";
            FileStream fs = new FileStream(Filteredname, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);

            for (int micID = 0; micID < 4; micID++)
            {
                for (int sample = 0; sample < dataStorages[dataIndex].soundSignal[micID].Length; sample++)
                {
                    bw.Write(dataStorages[dataIndex].soundSignal[micID][sample]);
                }
            }

            bw.Write((double)dataStorages[dataIndex].micLocalPos.x);
            bw.Write((double)dataStorages[dataIndex].micLocalPos.y);
            bw.Write((double)dataStorages[dataIndex].micLocalPos.z);

            bw.Write((double)dataStorages[dataIndex].micLocalRot.x);
            bw.Write((double)dataStorages[dataIndex].micLocalRot.y);
            bw.Write((double)dataStorages[dataIndex].micLocalRot.z);
            bw.Write((double)dataStorages[dataIndex].micLocalRot.w);

            bw.Close();
            fs.Close();            
        }
        MeasurementParameter.plotNumber = dataStorages.Count;
        await Task.Run(() => SettingSave());
    }



    /// <summary>
    /// 指定したパスにディレクトリが存在しない場合
    /// すべてのディレクトリとサブディレクトリを作成します
    /// </summary>
    public static DirectoryInfo SafeCreateDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            return null;
        }
        return Directory.CreateDirectory(path);
    }

    void SettingSave()
    {
        //設定値メモ保存
        string settingTxtPath = MeasurementParameter.SaveDir + @"\setting.txt";
        StreamWriter settingSW = new StreamWriter(settingTxtPath, false, System.Text.Encoding.GetEncoding("shift_jis"));
        //    settingSW.WriteLine("MeasurePointNum : " + dataStorages.Count.ToString());
        settingSW.WriteLine("sampleRate : " + MeasurementParameter.Fs);
        settingSW.WriteLine("sampleLength : " + MeasurementParameter.SampleNum);
        settingSW.WriteLine("freqRange : " + MeasurementParameter.FreqMin + " - " + MeasurementParameter.FreqMax);
        settingSW.WriteLine("Mic size : " + MeasurementParameter.MInterval);
        settingSW.WriteLine("atmPressure : " + MeasurementParameter.Atm);
        settingSW.WriteLine("temperature : " + MeasurementParameter.Temp);
        settingSW.WriteLine("Measure point interval : " + MeasurementParameter.ObjInterval);
        settingSW.WriteLine("Color ID :" + MeasurementParameter.colormapID);
        settingSW.WriteLine("Level gain :" + MeasurementParameter.MinIntensity + " - " + MeasurementParameter.MaxIntensity);
        settingSW.WriteLine("The number of plot :" + MeasurementParameter.plotNumber);
        settingSW.Close();
        return;
    }
}
