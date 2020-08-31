using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

 
public class StartSceneManager : MonoBehaviour
{
    private string DATA_SAVED_FOLDER = Application.streamingAssetsPath;

    //4点マイクのパラメータ管理
    [SerializeField]
    InputField[] calibs = new InputField[4];
    [SerializeField]
    InputField in_mInterval;
    [SerializeField]
    InputField SetName;
    [SerializeField]

    //保存された4点マイクパラメータの管理
    FileInfo[] files;
    [SerializeField]
    Dropdown mSetList;

    // Start is called before the first frame update
    //シーンを変更してもパラメータは変化されてないことを可視化
    void Start()
    {
        //UI、およびMeasurementParameterの更新
        UpdateParameter();
        //使用できるマイクセットの更新
        UpdateMicSetList();
    }

    /// <summary>
    /// 画面入力されたマイクセットのデータ(名前＋マイクロホン間隔＋キャリブレーションデータ)を.datにシリアライズして保存する
    /// ボタンに紐づけ
    /// </summary>
    public void SaveMicSet()
    {
        var bf = new BinaryFormatter();
        FileStream fileStream = null;

        var currentCalib = new double[4];

        try
        {
            //マイクセットの保存係数をセット
            //マイクの数だけ
            for (int i = 0; i < 4; i++)
            {
                currentCalib[i] = double.Parse(calibs[i].text);
            }
            var mInterval = float.Parse(in_mInterval.text);
            var setName = SetName.text;
            MicSetClass newSet = new MicSetClass(mInterval, currentCalib,setName);

            //フォルダにMicset.datを作成
            if (SetName.text == "")
                fileStream = File.Create(DATA_SAVED_FOLDER + "/dafault.dat");
            else if(SetName.text.Contains(".dat"))
                fileStream = File.Create(DATA_SAVED_FOLDER + "/" + setName);
            else
                fileStream = File.Create(DATA_SAVED_FOLDER + "/" + setName + ".dat");

            //ファイルにクラスを保存
            bf.Serialize(fileStream, newSet);
            
            UpdateMeasurementParameter(newSet);
            Debug.Log("新しいマイクセットを登録した");
        }
        catch (FormatException e)
        {
            Debug.Log("CalibrationSetの入力形式が間違えています"+e);
        }
        catch(IOException el)
        {
            Debug.Log("ファイルオープンエラー"+el);
        }
        finally
        {
            if(fileStream != null)
            {
                fileStream.Close();
            }
        }
        //DropDownの表示を更新
        UpdateMicSetList();
    }
    
    /// <summary>
    /// 保存済みのマイクセットデータをデシリアライズし、UI,MeasurementParameterに反映
    /// ボタンに紐づけ
    /// </summary>
    public void LoadMicSet()
    {
        var datapath = files[mSetList.value].FullName;
 
        Debug.Log("Selected Mic set is" + datapath);

        var bf = new BinaryFormatter();
        FileStream fileStream = null;

        try
        {
            //ファイル読み込み
            fileStream = File.Open(datapath, FileMode.Open);
            //読み込んだデータをデシリアライズ
            MicSetClass loadMSet = bf.Deserialize(fileStream) as MicSetClass;
            UpdateMeasurementParameter(loadMSet);
            UpdateParameter();
        }
        catch(Exception e) when (e is FileNotFoundException)
        {
            Debug.Log(datapath + "がありません"+e);
        }
        catch(Exception e2) when (e2 is IOException)
        {
            Debug.Log("ファイルオープンエラー"+e2);
        }
    }

    private void UpdateParameter()
    {
        SetName.text = MeasurementParameter.mSetName;
        var instant_calValue = MeasurementParameter.CalibValue;
        for (int i = 0; i < MeasurementParameter.CalibSize; i++)
        {
            calibs[i].text = instant_calValue[i].ToString();
        }
        in_mInterval.text = MeasurementParameter.MInterval.ToString();
    }

    private void UpdateMicSetList()
    {
        mSetList.ClearOptions();
        var di = new DirectoryInfo(DATA_SAVED_FOLDER);
        files = di.GetFiles("*.dat", SearchOption.TopDirectoryOnly);
        foreach (FileInfo f in files)
        {
            mSetList.options.Add(new Dropdown.OptionData { text = f.Name });
        }
        mSetList.RefreshShownValue();
    }

    private void UpdateMeasurementParameter(MicSetClass micSet)
    {
        MeasurementParameter.mSetName = micSet.name;
        MeasurementParameter.MInterval = micSet.micInterval;
        for(int i = 0; i < MeasurementParameter.CalibSize; i++)
        {
            MeasurementParameter.CalibValue[i] = micSet.calibData[i];
        }
    }
    #region debug_only
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Current Measurement Parameter : mSetName" + MeasurementParameter.mSetName + ",mInterval" + MeasurementParameter.MInterval +
                ", cal0" + MeasurementParameter.CalibValue[0] + ",cal1" + MeasurementParameter.CalibValue[1] +
                ",cal2" + MeasurementParameter.CalibValue[2] + ",cal3" + MeasurementParameter.CalibValue[3]);
        }

    }
    #endregion
}
