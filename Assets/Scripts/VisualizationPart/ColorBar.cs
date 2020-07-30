using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ColorBar : MonoBehaviour
{

    //parula.csvのパス(Resourcesフォルダ内)
    const string PARULA_PATH = "ColorCsv/parula";
    const string JET_PATH = "ColorCsv/jet";

    //parulaの値が入る行列
    private static Color[] parulaArray;
    private static Color[] jetArray;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        parulaArray = ReadColorCsv(PARULA_PATH);
        jetArray = ReadColorCsv(JET_PATH);
        Debug.Log("カラーバー読み込みOK!");
    }

    /// <summary>
    /// カラーバーのRGB情報が入ったcsvを1行ずつ読み込んでColorの行列を作る
    /// </summary>
    public static Color[] ReadColorCsv(string csvPath)
    {
        //csvを読み込み
        TextAsset resource = Resources.Load(csvPath) as TextAsset;
        StringReader sr = new StringReader(resource.text);

        //string行列に一時的に格納
        List<string[]> colorRgbStr = new List<string[]>();
        int rowCount = 0;
        while (sr.Peek() > -1)
        {
            string line = sr.ReadLine();
            colorRgbStr.Add(line.Split(','));
            rowCount++;
        }

        //string行列 -> Color行列
        Color[] colorArray = new Color[rowCount];
        for (int row = 0; row < rowCount; row++)
        {
            colorArray[row].r = float.Parse(colorRgbStr[row][0]);
            colorArray[row].g = float.Parse(colorRgbStr[row][1]);
            colorArray[row].b = float.Parse(colorRgbStr[row][2]);
            colorArray[row].a = 1f;
        }
        return colorArray;
    }

    /// <summary>
    /// インテンシティレベルから色を定義する
    /// </summary>
    /// <param name="min">インテンシティレベル領域の最低値</param>
    /// <param name="max">インテンシティレベル領域の最高値</param>
    /// <returns></returns>
    public static Color DefineColor(int colorMapId, float intensityLevel, float min, float max)
    {
        float colorScale = (intensityLevel - min) / (max - min);
        if (colorMapId == 0) //GrayScale
        {
            if (colorScale < 0)
            {
                return new Color(0, 0, 0, 0.3f);
            }
            if (colorScale > 1)
            {
                return new Color(1, 1, 1, 0.3f);
            }
            return new Color(colorScale, colorScale, colorScale, 1f);
        }
        else if (colorMapId == 1) //parula
        {
            if (colorScale < 0)
            {
                return parulaArray[0];
            }
            if (colorScale > 1)
            {
                return parulaArray[parulaArray.Length - 1];
            }
            int paruraIndex = Mathf.RoundToInt((parulaArray.Length - 1) * colorScale);
            return parulaArray[paruraIndex];
        }
        else //jet
        {
            if (colorScale < 0)
            {
                return jetArray[0];
            }
            if (colorScale > 1)
            {
                return jetArray[jetArray.Length - 1];
            }
            int jetIndex = Mathf.RoundToInt((jetArray.Length - 1) * colorScale);
            return jetArray[jetIndex];
        }
    }
}
