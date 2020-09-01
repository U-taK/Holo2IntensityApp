using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LogPanelManager: MonoBehaviour{
    //ログ表示
    [SerializeField]
    Text log;
    List<string> logList = new List<string>();
    int i = 0;
    [SerializeField]
    Text logIntensity;

    public void Writelog(string logElem)
    {
        if (logElem != null)
        {
            logList.Add(logElem);
            switch (logList.Count())
            {
                case 1:
                    log.text = logList[0];
                    break;
                case 2:
                    log.text = logList[1] + "\n" + logList[0];
                    break;
                default:
                    log.text = logList[logList.Count() - 1] + "\n" + logList[logList.Count() - 2] + "\n" + logList[logList.Count() - 3];
                    break;

            }
        }
    }

    public void Writelog()
    {
        switch (logList.Count() - i)
        {
            case 1:
                log.text = logList[0];
                break;
            case 2:
                log.text = logList[1] + "\n" + logList[0];
                break;
            default:
                log.text = logList[logList.Count() - i - 1] + "\n" + logList[logList.Count() - i - 2] + "\n" + logList[logList.Count() - i - 3];
                break;

        }
    }

    public void DownLog()
    {
        if (logList.Count() - i > 3)
            i += 3;
        Writelog();
    }

    public void UpLog()
    {
        if (i > 0)
            i -= 3;
        Writelog();
    }

    public void WriteConsole(int num, Vector3 sendPos, Vector3 intensity, float intensityLevel)
    {
        logIntensity.text =
            "Send Position is x:" + sendPos.x.ToString("f2") + " y:" + sendPos.y.ToString("f2") + " z:" + sendPos.z.ToString("f2") +
            "\n Intensity Data (" + num.ToString() + "is \n x: " + intensity.x.ToString("F12") +
                            "\n y: " + intensity.y.ToString("F12") +
                            "\n z: " + intensity.z.ToString("F12") +
                            "\n Intensity level: " + intensityLevel.ToString("f6") + "[dB]";
    }
}
