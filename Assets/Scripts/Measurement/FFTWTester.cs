using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using UnityEngine;

public class FFTWTester : MonoBehaviour
{
    double[][] signal = new double[4][];


    // Start is called before the first frame update
    void Start()
    {
        


    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("length(4)");
            var sw = new System.Diagnostics.Stopwatch();

            sw.Start();
            //Test
            double[] x = { 1.2, 3.4, 5.6, 7.8 };

            var dft = MathFFTW.FFT(x, true);

            var idft = MathFFTW.IFFT(dft);
            sw.Stop();
            Debug.Log("Test1: FFTW");
            DisplayCompex(dft);
            Debug.Log("IFFT = ");
            DisplayReal(idft);
            TimeSpan ts = sw.Elapsed;
            Debug.Log($"　{sw.ElapsedMilliseconds}ミリ秒");

            sw.Restart();
            Complex[] cx = new Complex[4];
            Complex[] cdft;
            double[] cidft;
            for (var i = 0; i < x.Length; i++)
                cx[i] = new Complex(x[i], 0);


            AcousticMathNew.FFT(2, cx, out cdft);
            AcousticMathNew.IFFT(2, cdft, out cidft);
            sw.Stop();
            Debug.Log("Test2: Acoustic Math New");
            DisplayCompex(dft);
            Debug.Log("IFFT = ");
            DisplayReal(idft);
            ts = sw.Elapsed;
            Debug.Log($"　{sw.ElapsedMilliseconds}ミリ秒");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("length(2048)");
            ReadTestData();

            var halfsig = new double[2048];
            for (var i = 0; i < halfsig.Length; i++)
                halfsig[i] = signal[0][i];

            var sw = new System.Diagnostics.Stopwatch();

            sw.Start();
            //Test

            var dft = MathFFTW.FFT(halfsig, true);

            var idft = MathFFTW.IFFT(dft);
            sw.Stop();
            Debug.Log("Test1: FFTW");
            //DisplayCompex(dft);
            //Debug.Log("IFFT = ");
            //DisplayReal(idft);
            TimeSpan ts = sw.Elapsed;
            Debug.Log($"　{sw.ElapsedMilliseconds}ミリ秒");

            sw.Restart();
            Complex[] cx = new Complex[halfsig.Length];
            Complex[] cdft;
            double[] cidft;
            for (var i = 0; i < halfsig.Length; i++)
                cx[i] = new Complex(halfsig[i], 0);


            AcousticMathNew.FFT(11, cx, out cdft);
            AcousticMathNew.IFFT(11, cdft, out cidft);
            sw.Stop();
            Debug.Log("Test2: Acoustic Math New");
            //DisplayCompex(dft);
            //Debug.Log("IFFT = ");
            //DisplayReal(idft);
            ts = sw.Elapsed;
            Debug.Log($"　{sw.ElapsedMilliseconds}ミリ秒");
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("length(4096)");
            ReadTestData();

            var sw = new System.Diagnostics.Stopwatch();

            sw.Start();
            //Test

            var dft = MathFFTW.FFT(signal[0], true);

            var idft = MathFFTW.IFFT(dft);
            sw.Stop();
            Debug.Log("Test1: FFTW");
            //DisplayCompex(dft);
            //Debug.Log("IFFT = ");
            //DisplayReal(idft);
            TimeSpan ts = sw.Elapsed;
            Debug.Log($"　{sw.ElapsedMilliseconds}ミリ秒");

            sw.Restart();
            Complex[] cx = new Complex[signal[0].Length];
            Complex[] cdft;
            double[] cidft;
            for (var i = 0; i < signal[0].Length; i++)
                cx[i] = new Complex(signal[0][i], 0);


            AcousticMathNew.FFT(12, cx, out cdft);
            AcousticMathNew.IFFT(12, cdft, out cidft);
            sw.Stop();
            Debug.Log("Test2: Acoustic Math New");
            //DisplayCompex(dft);
            //Debug.Log("IFFT = ");
            //DisplayReal(idft);
            ts = sw.Elapsed;
            Debug.Log($"　{sw.ElapsedMilliseconds}ミリ秒");
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("SoundIntensity");
            ReadTestData();

            var sw = new System.Diagnostics.Stopwatch();
            var fs = 44100;
            var fmin = 707f;
            var fmax = 1414f;
            var adensity = 1.1923f;
            var dr = 0.05f;
            sw.Start();
            //処理
            var intensity = MathFFTW.CrossSpectrumMethod(signal, fs, fmin, fmax, adensity, dr);
            var level = MathFFTW.CalcuIntensityLevel(intensity);        
            sw.Stop();
            Debug.Log("Test1: FFTW");

            Debug.Log($" Intensity:({intensity.x},{intensity.y},{intensity.z}) ");
            Debug.Log($" Intensity level is {level}");

            TimeSpan ts = sw.Elapsed;
            Debug.Log($"　{sw.ElapsedMilliseconds}ミリ秒");

            sw.Restart();
            intensity = AcousticMathNew.CrossSpectrumMethod(signal, fs, 12, fmin, fmax, adensity, dr);
            level = AcousticMathNew.CalcuIntensityLevel(intensity);
            sw.Stop();
            Debug.Log("Test2: Acoustic Math New");

            Debug.Log($" Intensity:({intensity.x},{intensity.y},{intensity.z}) ");
            Debug.Log($" Intensity level is {level}");

            ts = sw.Elapsed;
            Debug.Log($"　{sw.ElapsedMilliseconds}ミリ秒");
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("SoundIntensity (directMethod)");
            ReadTestData();

            var sw = new System.Diagnostics.Stopwatch();
            var adensity = 1.1923f;
            var dr = 0.05f;
            sw.Start();
            //処理
            var intensities = AcousticSI.DirectMethod(signal, adensity, dr);
            var sumIntensity = AcousticSI.SumIntensity(intensities);
            var level = MathFFTW.CalcuIntensityLevel(sumIntensity);
            sw.Stop();
            Debug.Log("Test1: Instant Intensity: ");

            Debug.Log($" Intensity:({sumIntensity.x},{sumIntensity.y},{sumIntensity.z}) ");
            Debug.Log($" Intensity level is {level}");

            TimeSpan ts = sw.Elapsed;
            Debug.Log($"　{sw.ElapsedMilliseconds}ミリ秒");
            
        }
    }

    void DisplayCompex(double[] x)
    {
        if(x.Length % 2 != 0)
        {
            throw new Exception("The number of elements must be even");
        }
        for(int i = 0, n = x.Length; i < n; i+= 2)
        {
            if (x[i + 1] < 0)
                Debug.Log(string.Format("{0} - {1}i", x[i], Math.Abs(x[i + 1])));
            else
                Debug.Log(string.Format("{0} + {1}i", x[i], Math.Abs(x[i + 1])));
        }
    }

    void DisplayCompex(Complex[] x)
    {      
        for (int i = 0, n = x.Length; i < n; i++)
        {                           
            Debug.Log(string.Format("{0} + {1}i", x[i].Real, x[i + 1].Imaginary));
        }
    }


    void DisplayReal(double[] x)
    {
        if (x.Length % 2 != 0)
        {
            throw new Exception("The number of elements must be even");
        }
        for (int i = 0, n = x.Length; i < n; i += 2)
        {
            Debug.Log(x[i]);
        }
    }

    void ReadTestData()
    {

        //byteファイル読み込み

        TextAsset asset1 = Resources.Load("TestData/mic1", typeof(TextAsset)) as TextAsset;
        TextAsset asset2 = Resources.Load("TestData/mic2", typeof(TextAsset)) as TextAsset;
        TextAsset asset3 = Resources.Load("TestData/mic3", typeof(TextAsset)) as TextAsset;
        TextAsset asset4 = Resources.Load("TestData/mic4", typeof(TextAsset)) as TextAsset;
        signal[0] = new double[asset1.bytes.Length / 8];
        signal[1] = new double[asset2.bytes.Length / 8];
        signal[2] = new double[asset3.bytes.Length / 8];
        signal[3] = new double[asset4.bytes.Length / 8];

        signal[0] = Bytes2array(asset1, asset1.bytes.Length / 8);
        signal[1] = Bytes2array(asset2, asset2.bytes.Length / 8);
        signal[2] = Bytes2array(asset3, asset3.bytes.Length / 8);
        signal[3] = Bytes2array(asset4, asset4.bytes.Length / 8);
    }


    /// <summary>
    /// bytesファイルをdouble型配列に
    /// </summary>
    /// <param name="asset">bytesファイル</param>
    /// <param name="leng">読み込み長さ</param>
    public static double[] Bytes2array(TextAsset asset, int leng)
    {
        double[] sound = new double[leng];
        using (Stream fs = new MemoryStream(asset.bytes))
        {
            using (BinaryReader br = new BinaryReader(fs))
            {
                int i = 0;
                while (i < leng)
                {
                    //符号付2byte読み込み
                    //sound[i] = (double)br.ReadInt16();
                    sound[i] = (double)br.ReadDouble();
                    i++;
                }
            }
        }
        return sound;
    }
    /// <summary>
    /// bytesファイルをshort型配列に
    /// </summary>
    /// <param name="asset">bytesファイル</param>
    /// <param name="leng">読み込み長さ</param>
    public static short[] Bytes2array_sh(TextAsset asset, int leng)
    {
        short[] sound = new short[leng];
        using (Stream fs = new MemoryStream(asset.bytes))
        {
            using (BinaryReader br = new BinaryReader(fs))
            {
                int i = 0;
                while (i < leng)
                {
                    sound[i] = br.ReadInt16();
                    i++;
                }
            }
        }
        return sound;
    }






}
