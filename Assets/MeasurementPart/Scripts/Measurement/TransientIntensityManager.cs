using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AsioCSharpDll;
using System.IO;
using System.Threading.Tasks;

public class TransientIntensityManager : MonoBehaviour
{
    //0:直接法,1:STFTを使ったクロススペクトル法, 2:アンビソニックマイクを使った時間領域処理, 3:アンビソニックマイクを使った周波数領域処理
    [SerializeField]
    Dropdown algorithmList;

    //PC版生成用プレハブ
    [SerializeField]
    GameObject Cone;
    //空間基準マーカプレハブ
    [SerializeField]
    GameObject copyStandard;

    //インテンシティオブジェクトを作成しておく
    Dictionary<int, DataStorage> dataStorages = new Dictionary<int, DataStorage>();

    //生成オブジェクトの管理
    Dictionary<int, GameObject> intensities = new Dictionary<int, GameObject>();
    //送信データの管理番号
    int Num = 0;
   
    TransientServerManager tServerManager;

    // Start is called before the first frame update
    void Start()
    {
        tServerManager = gameObject.GetComponent<TransientServerManager>();

        algorithmList.onValueChanged.AddListener(delegate 
        {
            DropdownValueChanged(algorithmList);
        });
    }

    void DropdownValueChanged(Dropdown dropDown)
    {
        switch (dropDown.value)
        {
            //時間領域で平均を取らない場合
            case 0:
            case 2:
                MeasurementParameter.i_block = 2;
                break;
            case 1:
            case 3:
                MeasurementParameter.i_block = 128;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MicPosReceived(SendPosition sendPosition)
    {
        StartCoroutine(RecordSignal(sendPosition));
    }

    private IEnumerator RecordSignal(SendPosition sendPosition)
    {
        //インテンシティオブジェクト(Server側)
        GameObject micPoint = new GameObject("measurementPoint" + Num);
        micPoint.transform.parent = copyStandard.transform;
        micPoint.transform.localPosition = sendPosition.sendPos;
        micPoint.transform.localRotation = sendPosition.sendRot;


        //音声再生
        asiocsharpdll.StartSound();
        //録音のlengthbit分待つ
        //yield return new WaitForSeconds(4096f / 44100f);
        //録音開始
        var soundSignals = asiocsharpdll.GetAsioSoundSignals(MeasurementParameter.SampleNum, MeasurementParameter.CalibValue);

        List<Vector3> intensityList = new List<Vector3>();
        //時間変化する音響インテンシティを指定したアルゴリズムを元に計算
        switch (algorithmList.value)
        {
            case 0://直接法
                intensityList.AddRange(AcousticSI.DirectMethod(soundSignals, MeasurementParameter.AtmDensity, MeasurementParameter.MInterval));
                break;
            case 1://STFTを使った時間周波数領域での計算処理
                intensityList.AddRange(MathFFTW.STFTmethod(soundSignals, MeasurementParameter.i_block / 2, MeasurementParameter.i_block, MeasurementParameter.Fs, MeasurementParameter.FreqMin, MeasurementParameter.FreqMax, MeasurementParameter.AtmDensity, MeasurementParameter.MInterval));
                break;
            case 2://アンビソニックマイクを使った時間領域のpsudoIntensityの推定
                intensityList.AddRange(MathAmbisonics.TdomMethod(soundSignals, MeasurementParameter.AtmDensity, 340));
                break;
            case 3://アンビソニックマイクを使った時間周波数領域のpsudoIntensityの推定
                intensityList.AddRange(MathAmbisonics.TFdomMethod(soundSignals, MeasurementParameter.i_block / 2, MeasurementParameter.i_block, MeasurementParameter.Fs, MeasurementParameter.FreqMin, MeasurementParameter.FreqMax, MeasurementParameter.AtmDensity, 340));
                break;
        }
        var intensityDirection = intensityList.ToArray();
        //直接法計算
        var sumIntensity = AcousticSI.SumIntensity(intensityDirection);
        var sumIntensityLv = MathFFTW.CalcuIntensityLevel(sumIntensity);
        yield return null;

        //PCがわ表示
        float[] intensityLv = new float[intensityDirection.Length];
        for (int i = 0; i < intensityDirection.Length; i++)
        {
            intensityLv[i] = MathFFTW.CalcuIntensityLevel(intensityDirection[i]);
        }    
        yield return null;


        //オブジェクト作成
        var vectorObj = Instantiate(Cone) as GameObject;
        vectorObj.transform.localScale = new Vector3(MeasurementParameter.objSize, MeasurementParameter.objSize, MeasurementParameter.objSize * 4);
        vectorObj.transform.parent = micPoint.transform;
        vectorObj.transform.localPosition = Vector3.zero;
        vectorObj.transform.localRotation = Quaternion.LookRotation(10000000000 * sumIntensity);
        var vecColor = ColorBar.DefineColor(MeasurementParameter.colormapID, sumIntensityLv, MeasurementParameter.MinIntensity, MeasurementParameter.MaxIntensity);
        vectorObj.transform.GetComponent<Renderer>().material.color = vecColor;
        vectorObj.name = "IntensityObj";
        var parameter = vectorObj.AddComponent<ParameterStorage>();
        parameter.PutIntensity(intensityDirection, intensityLv);
        intensities.Add(Num, vectorObj);

        //データそのものを保管
        DataStorage data = new DataStorage(Num, sendPosition.sendPos, sendPosition.sendRot, soundSignals, sumIntensity);
        dataStorages.Add(Num, data);

        //送信データを作成
        var sendData = new TransIntensityPackage(sendPosition, sumIntensity,intensityDirection, Num);
        tServerManager.SendIntensity(sendData);
        Num++;
        yield return null;
    }

    public async Task<ReCalcTransientDataPackage> RecalcTransientIntensity()
    {
        //データの更新
        ReCalcTransientDataPackage recalcTransientIntensity = await Task.Run(() => AsyncReCalc());

        //送信データの更新
        StartCoroutine("UpdateEditor", recalcTransientIntensity);

        return recalcTransientIntensity;
    }

    private ReCalcTransientDataPackage AsyncReCalc()
    {
        ReCalcTransientDataPackage data = new ReCalcTransientDataPackage(dataStorages.Count);
        List<Vector3> intensityList = new List<Vector3>();
        foreach (DataStorage dataStorage in dataStorages.Values)
        {
            intensityList.Clear();
            //時間変化する音響インテンシティを指定したアルゴリズムを元に計算
            switch (algorithmList.value)
            {
                case 0://直接法
                    intensityList.AddRange(AcousticSI.DirectMethod(dataStorage.soundSignal, MeasurementParameter.AtmDensity, MeasurementParameter.MInterval));
                    break;
                case 1://STFTを使った時間周波数領域での計算処理
                    intensityList.AddRange(MathFFTW.STFTmethod(dataStorage.soundSignal, MeasurementParameter.i_block / 2, MeasurementParameter.i_block, MeasurementParameter.Fs, MeasurementParameter.FreqMin, MeasurementParameter.FreqMax, MeasurementParameter.AtmDensity, MeasurementParameter.MInterval));
                    break;
                case 2://アンビソニックマイクを使った時間領域のpsudoIntensityの推定
                    intensityList.AddRange(MathAmbisonics.TdomMethod(dataStorage.soundSignal, MeasurementParameter.AtmDensity, 340));
                    break;
                case 3://アンビソニックマイクを使った時間周波数領域のpsudoIntensityの推定
                    intensityList.AddRange(MathAmbisonics.TFdomMethod(dataStorage.soundSignal, MeasurementParameter.i_block / 2, MeasurementParameter.i_block, MeasurementParameter.Fs, MeasurementParameter.FreqMin, MeasurementParameter.FreqMax, MeasurementParameter.AtmDensity, 340));
                    break;
            }
            var intensityDirection = intensityList.ToArray();
            //平均インテンシティ計算
            var sumIntensity = AcousticSI.SumIntensity(intensityDirection);

            data.sendNums.Add(dataStorage.measureNo);
            data.intensities.Add(sumIntensity);
            data.iintensityList.Add(intensityDirection);
        }

        return data;
    }

    /// <summary>
    /// 更新インテンシティデータをエディタ上でも反映させていくようにする
    /// </summary>
    /// <param name="reCalcDataPackage">更新データ</param>
    /// <returns></returns>
    private IEnumerator UpdateEditor(ReCalcTransientDataPackage reCalcTransientDataPackage)
    {
        for (int i = 0; i < reCalcTransientDataPackage.storageNum; i++)
        {
            if (intensities.ContainsKey(reCalcTransientDataPackage.sendNums[i]))
            {
                var pushObj = intensities[reCalcTransientDataPackage.sendNums[i]];
                //色変更を行う
                pushObj.transform.localRotation = Quaternion.LookRotation(10000000000 * reCalcTransientDataPackage.intensities[i]);
                var lv = AcousticMathNew.CalcuIntensityLevel(reCalcTransientDataPackage.intensities[i]);
                pushObj.transform.GetComponent<Renderer>().material.color = ColorBar.DefineColor(MeasurementParameter.colormapID, lv, MeasurementParameter.MinIntensity, MeasurementParameter.MaxIntensity);
                //瞬時音響インテンシティの変更
                var intensityDirection = reCalcTransientDataPackage.iintensityList[i];
                float[] intensityLv = new float[intensityDirection.Length];
                for (int j = 0; j < intensityDirection.Length; j++)
                {
                    intensityLv[j] = MathFFTW.CalcuIntensityLevel(intensityDirection[j]);
                }

                var parameter = pushObj.GetComponent<ParameterStorage>();
                parameter.PutIntensity(intensityDirection, intensityLv);
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
        await Task.Run(() => SettingSave());
        Debug.Log(MeasurementParameter.SaveDir);
        //ディレクトリなかったら作成
        SafeCreateDirectory(MeasurementParameter.SaveDir);
        int index = 0;
        //録音＆マイク位置バイナリファイル保存
        foreach (var dataStorage in dataStorages.Values)
        {
            int dataIndex = index++;
            string pathName = MeasurementParameter.SaveDir + @"\measurepoint_" + dataIndex.ToString() + ".bytes";
            await Task.Run(() =>
            {
                FileStream fs = new FileStream(pathName, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);

                for (int micID = 0; micID < 4; micID++)
                {
                    for (int sample = 0; sample < dataStorage.soundSignal[micID].Length; sample++)
                    {
                        bw.Write(dataStorage.soundSignal[micID][sample]);
                    }
                }

                bw.Write((double)dataStorage.micLocalPos.x);
                bw.Write((double)dataStorage.micLocalPos.y);
                bw.Write((double)dataStorage.micLocalPos.z);

                bw.Write((double)dataStorage.micLocalRot.x);
                bw.Write((double)dataStorage.micLocalRot.y);
                bw.Write((double)dataStorage.micLocalRot.z);
                bw.Write((double)dataStorage.micLocalRot.w);

                bw.Close();
                fs.Close();
            });
        }
        MeasurementParameter.plotNumber = dataStorages.Count;
        
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

    public void DeleteIntensity(DeleteData dData)
    {
        var dNum = dData.intensityID;
        if (intensities.ContainsKey(dNum))
        {
            Destroy(intensities[dNum].transform.parent.gameObject);
            intensities.Remove(dNum);
            dataStorages.Remove(dNum);
        }
    }
}
