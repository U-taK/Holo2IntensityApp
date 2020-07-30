using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uOSC
{
    public class IntensityServer : MonoBehaviour
    {
        [SerializeField]
        UIPanelManager manager;
        List<int> measureID = new List<int>();
        //送信されたデータを格納するためのList
        List<string> transformData = new List<string>();
        int i = 0;

        InstanceManager instanceManager;
        Vector3 micPos;
        Quaternion micRot;

        [SerializeField]
        PositionSender positionSender;
        // Use this for initialization
        void Start()
        {
            var server = GetComponent<uOscServer>();
            instanceManager = GetComponent<InstanceManager>();
            server.onDataReceived.AddListener(OnDataReceived);
        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnDataReceived(Message message)
        {
            StartCoroutine(DataProcess(message));
        }

        private IEnumerator DataProcess(Message message)
        {
            //PCからマイクの座標＋回転＋三次元インテンシティの10要素を取得
            //address
            var msg = message.address;
            Debug.Log("Catch the address: " + msg);
            yield return null;
            if (msg == "ResultSend" && UIManager._instance)
            {
                //value
                foreach (var value in message.values)
                {
                    transformData.Add(value.ToString());
                }

                Vector3 Intensity = new Vector3(float.Parse(transformData[i + 7]), float.Parse(transformData[i + 8]), float.Parse(transformData[i + 9]));

                float intensityLv = AIMath.CalcuIntensityLevel(Intensity);

                //インテンシティレベルが指定した範囲内なら
                if (intensityLv >= UIManager.LevelMin || intensityLv <= UIManager.LevelMax)
                {
                    //コーンの色を指定
                    Color vecObjColor = ColorBar.DefineColor(UIManager.ColorMapID, intensityLv, UIManager.LevelMin, UIManager.LevelMax);
                    micPos = new Vector3(float.Parse(transformData[i]), float.Parse(transformData[i + 1]), float.Parse(transformData[i + 2]));
                    micRot = new Quaternion(float.Parse(transformData[i + 3]), float.Parse(transformData[i + 4]), float.Parse(transformData[i + 5]), float.Parse(transformData[i + 6]));
                    instanceManager.CreateInstantObj(int.Parse(transformData[i + 10]), micPos, micRot, Intensity, vecObjColor, UIManager.ObjSIze);
                    measureID.Add(int.Parse(transformData[i + 10]));
                    Debug.Log(DateTime.Now.ToString("MM/dd/HH:mm:ss.fff") + "Display measurement point No." + transformData[i + 10]);

                }
                // positionSender.delaySend(int.Parse(transformData[i + 10]), DateTime.Now);
                i += 11;
                yield return null;
            }
            else if (msg == "SharingStart")
            {
                UIManager.ColorMapID = int.Parse(message.values[0].ToString());
                UIManager.LevelMin = float.Parse(message.values[1].ToString());
                UIManager.LevelMax = float.Parse(message.values[2].ToString());
                UIManager.ObjSIze = float.Parse(message.values[3].ToString());
                manager.ReadyShare();
                yield return null;
            }
            else if (msg == "ReSend")
            {
                //value
                foreach (var value in message.values)
                {
                    transformData.Add(value.ToString());
                }
                if (!measureID.Exists(x => x == int.Parse(transformData[i + 10])))
                {
                    Vector3 Intensity = new Vector3(float.Parse(transformData[i + 7]), float.Parse(transformData[i + 8]), float.Parse(transformData[i + 9]));

                    float intensityLv = AIMath.CalcuIntensityLevel(Intensity);

                    //インテンシティレベルが指定した範囲内なら
                    if (intensityLv >= UIManager.LevelMin && intensityLv <= UIManager.LevelMax)
                    {
                        //コーンの色を指定
                        Color vecObjColor = ColorBar.DefineColor(UIManager.ColorMapID, intensityLv, UIManager.LevelMin, UIManager.LevelMax);
                        micPos = new Vector3(float.Parse(transformData[i]), float.Parse(transformData[i + 1]), float.Parse(transformData[i + 2]));
                        micRot = new Quaternion(float.Parse(transformData[i + 3]), float.Parse(transformData[i + 4]), float.Parse(transformData[i + 5]), float.Parse(transformData[i + 6]));
                        instanceManager.CreateInstantObj(int.Parse(transformData[i + 10]), micPos, micRot, Intensity, vecObjColor, UIManager.ObjSIze);
                        measureID.Add(int.Parse(transformData[i + 10]));
                        Debug.Log(DateTime.Now.ToString("MM/dd/HH:mm:ss.fff") + "Redisplay measurement point No." + transformData[i + 10]);

                    }

                }
                else
                {
                    Debug.Log("No." + transformData[i + 10] + "already display");
                }
                i += 11;
                yield return null;
            }
            else if (msg == "Delete")
            {
                int deleteNum = int.Parse(message.values[0].ToString());
                if (measureID.Exists(x => x == deleteNum))
                {
                    Destroy(GameObject.Find("measurepoint" + deleteNum.ToString()));
                    measureID.RemoveAll(x => x == deleteNum);
                    instanceManager.DeleteVectorObj(deleteNum);
                }
                yield return null;
            }
            else if (msg == "ColorChange")
            {
                List<Vector3> transInt = new List<Vector3>();
                List<Color> colors = new List<Color>();
                List<Vector3> scales = new List<Vector3>();

                foreach (var value in message.values)
                {
                    transformData.Add(value.ToString());
                }

                for (; i < transformData.Count;)
                {
                    Vector3 Intensity = new Vector3(float.Parse(transformData[i]), float.Parse(transformData[i + 1]), float.Parse(transformData[i + 2]));
                    int Num = int.Parse(transformData[i + 3]);
                    float intensityLv = AIMath.CalcuIntensityLevel(Intensity);
                    //コーンの色を指定
                    Color ObjColor = ColorBar.DefineColor(UIManager.ColorMapID, intensityLv, UIManager.LevelMin, UIManager.LevelMax);
                    instanceManager.ChangeIntensityObj(Num, Intensity, ObjColor);
                    i += 4;
                    yield return null;
                }
            }
            else if (msg == "Reproduct")
            {
                //value
                foreach (var value in message.values)
                {
                    transformData.Add(value.ToString());
                }
                for (; i < transformData.Count;)
                {
                    if (!measureID.Exists(x => x == int.Parse(transformData[i + 10])))
                    {
                        Vector3 Intensity = new Vector3(float.Parse(transformData[i + 7]), float.Parse(transformData[i + 8]), float.Parse(transformData[i + 9]));

                        float intensityLv = AIMath.CalcuIntensityLevel(Intensity);

                        //インテンシティレベルが指定した範囲内なら
                        if (intensityLv >= UIManager.LevelMin || intensityLv <= UIManager.LevelMax)
                        {
                            //コーンの色を指定
                            Color vecObjColor = ColorBar.DefineColor(UIManager.ColorMapID, intensityLv, UIManager.LevelMin, UIManager.LevelMax);
                            micPos = new Vector3(float.Parse(transformData[i]), float.Parse(transformData[i + 1]), float.Parse(transformData[i + 2]));
                            micRot = new Quaternion(float.Parse(transformData[i + 3]), float.Parse(transformData[i + 4]), float.Parse(transformData[i + 5]), float.Parse(transformData[i + 6]));
                            instanceManager.CreateInstantObj(int.Parse(transformData[i + 10]), micPos, micRot, Intensity, vecObjColor, UIManager.ObjSIze);
                            measureID.Add(int.Parse(transformData[i + 10]));
                        }
                    }
                    i += 11;
                    // positionSender.delaySend(int.Parse(transformData[i + 10]), DateTime.Now);
                    yield return null;
                }

            }
        }
    }
}

