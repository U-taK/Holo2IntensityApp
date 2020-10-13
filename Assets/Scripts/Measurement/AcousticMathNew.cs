///<summary>
///計算方法をここに示す
///計算方法をfloatからdoubleに変更(new)
///キャリブレーションの反映を行う(new)
/// </summary>

using UnityEngine;
using System.Numerics;
using System;

class AcousticMathNew
{
    ///<summary>
    ///クロススペクトル法で三次元音響インテンシティ計算
    /// </summary>
    /// <param name="sound_signals">音圧情報</param>
    /// <param name="length_bit">サンプル数の乗数</param>
    /// <param name="fs">サンプリング周波数</param>
    /// <param name="freq_range_min">周波数範囲下限</param>
    /// <param name="freq_range_max">周波数範囲上限</param>
    /// <param name="atmDensity">大気密度</param>
    /// <param name="dr">マイク間距離</param>
    public static UnityEngine.Vector3 CrossSpectrumMethod(double[][] sound_signals, int fs, int length_bit, float freq_range_min, float freq_range_max, double atmDensity, float dr)
    {
        //サンプル長を求める
        int sampleLength = 1 << length_bit;
        //FFT
        Complex[][] fft = new Complex[4][];
        Complex[][] input = new Complex[4][];


        for (int micID = 0; micID < 4; micID++)
        {
            input[micID] = new Complex[sound_signals[micID].Length];
            for (int k = 0; k < input[micID].Length; k++)
                input[micID][k] = new Complex(sound_signals[micID][k], 0.0);
            fft[micID] = new Complex[sampleLength];

            FFT(length_bit, input[micID], out fft[micID]);
            //FFT結果を平均化
            Complex samplelength = new Complex(sampleLength, 0);
            for (int fftIndex = 0; fftIndex < sampleLength; fftIndex++)
                fft[micID][fftIndex] = Complex.Divide(fft[micID][fftIndex], samplelength);

        }

        //FFTした時のサンプル範囲を求める
        float df = fs / sampleLength;
        int fftIndexMin = Mathf.CeilToInt(freq_range_min / df);
        int fftIndexMax = Mathf.FloorToInt(freq_range_max / df);

        //インテンシティの積分範囲を求める
        double sig01 = 0d;
        double sig02 = 0d;
        double sig03 = 0d;
        double sig12 = 0d;
        double sig13 = 0d;
        double sig23 = 0d;
        for (int fftIndex = fftIndexMin; fftIndex <= fftIndexMax; fftIndex++)
        {
            //ナイキスト周波数の時だけ計算
            if (fftIndex <= sampleLength / 2)
            {
                //両側クロススペクトルの虚部
                double imS01 = fft[0][fftIndex].Real * fft[1][fftIndex].Imaginary - fft[1][fftIndex].Real * fft[0][fftIndex].Imaginary;
                double imS02 = fft[0][fftIndex].Real * fft[2][fftIndex].Imaginary - fft[2][fftIndex].Real * fft[0][fftIndex].Imaginary;
                double imS03 = fft[0][fftIndex].Real * fft[3][fftIndex].Imaginary - fft[3][fftIndex].Real * fft[0][fftIndex].Imaginary;
                double imS12 = fft[1][fftIndex].Real * fft[2][fftIndex].Imaginary - fft[2][fftIndex].Real * fft[1][fftIndex].Imaginary;
                double imS13 = fft[1][fftIndex].Real * fft[3][fftIndex].Imaginary - fft[3][fftIndex].Real * fft[1][fftIndex].Imaginary;
                double imS23 = fft[2][fftIndex].Real * fft[3][fftIndex].Imaginary - fft[3][fftIndex].Real * fft[2][fftIndex].Imaginary;

                //両側クロススペクトル虚部 -> 片側クロススペクトル虚部
                double imG01;
                double imG02;
                double imG03;
                double imG12;
                double imG13;
                double imG23;
                if (fftIndex == 0 || fftIndex == sampleLength / 2)
                {
                    imG01 = imS01;
                    imG02 = imS02;
                    imG03 = imS03;
                    imG12 = imS12;
                    imG13 = imS13;
                    imG23 = imS23;
                }
                else
                {
                    imG01 = 2 * imS01;
                    imG02 = 2 * imS02;
                    imG03 = 2 * imS03;
                    imG12 = 2 * imS12;
                    imG13 = 2 * imS13;
                    imG23 = 2 * imS23;
                }

                //積分範囲
                sig01 += imG01 / fftIndex;
                sig02 += imG02 / fftIndex;
                sig03 += imG03 / fftIndex;
                sig12 += imG12 / fftIndex;
                sig13 += imG13 / fftIndex;
                sig23 += imG23 / fftIndex;
            }
        }

        //インテンシティ計算
        double i01 = sig01 / (2d * Math.PI * atmDensity * dr);
        double i02 = sig02 / (2d * Math.PI * atmDensity * dr);
        double i03 = sig03 / (2d * Math.PI * atmDensity * dr);
        double i12 = sig12 / (2d * Math.PI * atmDensity * dr);
        double i13 = sig13 / (2d * Math.PI * atmDensity * dr);
        double i23 = sig23 / (2d * Math.PI * atmDensity * dr);

        //左手系ワールド座標(x,y,z)に変換
        double ix = -(i01 - i02 + i23 - i13 - (2 * i12)) / 4;
        double iy = -(i01 + i02 + i03) / Math.Sqrt(6d);
        double iz = -(-i01 - i02 + (2 * i03) + (3 * i13) + (3 * i23)) / (4d * Math.Sqrt(3d));
        return new UnityEngine.Vector3((float)ix, (float)iy, (float)iz);
    }

    /// <summary>
    /// 三次元音響インテンシティ(vec3)からインテンシティレベル(float)を計算
    /// </summary>
    public static float CalcuIntensityLevel(UnityEngine.Vector3 intensity)
    {
        float intensityNorm = intensity.magnitude;
        float intensityLevel = 10f * Mathf.Log10(intensityNorm / Mathf.Pow(10f, -12f));
        return intensityLevel;
    }

    /// <summary>
    /// FFT
    /// </summary>
    /// <param name="bitSize">ビット数</param>
    /// <param name="input">入力</param>
    /// <param name="output">結果</param>
    public static void FFT(int bitSize, Complex[] input, out Complex[] output)
    {
        int dataSize = 1 << bitSize;
        //ビット反転
        int[] reverseBitArray = BitScrollArray(dataSize);
        output = new Complex[dataSize];

        //バタフライ演算のための置き換え
        for (int i = 0; i < dataSize; i++)
            output[i] = input[reverseBitArray[i]];

        //バタフライ演算
        for (int stage = 1; stage <= bitSize; stage++)
        {
            int butterflyDistance = 1 << stage;
            int numType = butterflyDistance >> 1;
            int butterflySize = butterflyDistance >> 1;
            Complex w = new Complex(1.0, 0.0);
            Complex u = new Complex(System.Math.Cos(System.Math.PI / butterflySize), -System.Math.Sin(System.Math.PI / butterflySize));

            for (int type = 0; type < numType; type++)
            {
                for (int j = type; j < dataSize; j += butterflyDistance)
                {
                    int jp = j + butterflySize;
                    Complex temp = new Complex(output[jp].Real * w.Real - output[jp].Imaginary * w.Imaginary, output[jp].Real * w.Imaginary + output[jp].Imaginary * w.Real);

                    output[jp] = new Complex(output[j].Real - temp.Real, output[j].Imaginary - temp.Imaginary);
                    output[j] += temp;

                }
                w = new Complex(w.Real * u.Real - w.Imaginary * u.Imaginary, w.Real * u.Imaginary + w.Imaginary * u.Real);
            }
        }
    }

    /// <summary>
    /// 1次元IFFT
    /// </summary>
    public static void IFFT(
        int bitSize,
        Complex[] input,
        out double[] outputReal
        )
    {
        int dataSize = 1 << bitSize;
        Complex[] output = new Complex[dataSize];
        outputReal = new double[dataSize];
        //複素共役をとってfft
        for (int i = 0; i < dataSize; i++)
        {
            input[i] = Complex.Conjugate(input[i]);
        }
        FFT(bitSize, input, out output);
        for (int i = 0; i < dataSize; i++)
        {
            outputReal[i] = Complex.Conjugate(output[i]).Real / (double)dataSize;
        }

    }

    /// <summary>
    /// Original filter function(2次のバンドパスフィルタ)
    /// 周波数を変更する場合はc_freqを変更
    /// </summary>
    /// <param name="input">入力信号</param>
    /// <param name="signals">出力信号</param>
    /// <param name="sampleLength">出力信号長</param>
    public static void BPFilter(double[] input, out double[] signals, int sampleLength)
    {
        int c_freq = 1000;
        float bw = 1f / 3f;
        int fs = 44100;
        float omega = 2 * Mathf.PI * c_freq / fs;
        float alpha = Mathf.Sin(omega) * (float)Math.Sinh(Mathf.Log(2) / 2 * bw * omega / Math.Sin(omega));

        float a0 = 1 + alpha;
        float b0 = alpha / a0;
        float b1 = 0;
        float b2 = -alpha / a0;
        float a1 = (-2 * Mathf.Cos(omega)) / a0;
        float a2 = (1 - alpha) / a0;

        //filter実装
        signals = new double[sampleLength];
        signals[0] = b0 * input[0];
        signals[1] = b0 * input[1] + b1 * input[0] - a1 * signals[0];
        for (int m = 2; m < sampleLength; m++)
        {

            //y[n] = b0 * x[n] + b1 * x[n - 1] + b2 * x[n - 2] - a1 * y[n - 1] - a2 * y[n - 2]

            signals[m] = b0 * input[m] + b1 * input[m - 1] + b2 * input[m - 2] -
            a1 * signals[m - 1] - a2 * signals[m - 2];
        }
    }



    /// <summary>
    /// ビットを左右反転した配列を返す
    /// </summary>
    /// <param name="arraySize"></param>
    /// <returns></returns>
    private static int[] BitScrollArray(int arraySize)
    {
        int[] reBitArray = new int[arraySize];
        int arraySizeHarf = arraySize >> 1;

        reBitArray[0] = 0;
        for (int i = 1; i < arraySize; i <<= 1)
        {
            for (int j = 0; j < i; j++)
                reBitArray[j + i] = reBitArray[j] + arraySizeHarf;
            arraySizeHarf >>= 1;
        }
        return reBitArray;
    }


    double RMS(double[] soundSignal)
    {
        int length = soundSignal.Length;
        double rms = 0;
        for (int s = 0; s < length; s++)
        {
            rms += Math.Pow(soundSignal[s], 2) / length;
        }
        Math.Sqrt(rms);
        return rms;
    }
}