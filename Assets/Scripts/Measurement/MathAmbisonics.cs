using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

class MathAmbisonics
{
    /// <summary>
    /// アンビソニックマイクを使った時間領域のpsudoIntensityの推定法
    /// </summary>
    /// <param name="signals">入力信号</param>
    /// <param name="atmDensity">大気圧</param>
    /// <param name="c">音速</param>
    /// <returns></returns>
    public static Vector3[] TdomMethod(double[][] signals, float atmDensity, float c)
    {
        if(signals == null)
        {
            Debug.LogError("No sound signals");
            return null;
        }
        var size = signals[0].Length;
        double W, X, Y, Z;

        Vector3[] instant_intensities = new Vector3[size];

        //FOA(First order ambisonics)
        for(int i = 0; i < size; i++)
        {
            //0次
            W = signals[0][i] + signals[1][i] + signals[2][i] + signals[3][i];
            //1次
            Z = signals[0][i] + signals[1][i] - signals[2][i] - signals[3][i];
            X = -(signals[0][i] - signals[1][i] + signals[2][i] - signals[3][i]);
            Y = signals[0][i] - signals[1][i] - signals[2][i] + signals[3][i];
            var ui = -W * X / (atmDensity * c);
            var vi = -W * Y / (atmDensity * c);
            var wi = -W * Z / (atmDensity * c);
            instant_intensities[i] = new Vector3((float)ui, (float)vi, (float)wi);
        }
        return instant_intensities;
    }

    public static Vector3[] TFdomMethod(double[][] signals,int overlap, int nfft, int fs, float freqmin, float freqmax, double atmDensity, float c)
    {
        //FFTした時のサンプル範囲を求める
        float df = (float)fs / nfft;
        int fftIndexMin = Mathf.CeilToInt(freqmin / df) * 2;
        int fftIndexMax = Mathf.FloorToInt(freqmax / df) * 2;

        var size = signals[0].Length;
        double[] W = new double[size];
        double[] X = new double[size];
        double[] Y = new double[size];
        double[] Z = new double[size];

        //FOA(First order ambisonics)
        for (int i = 0; i < size; i++)
        {
            //0次
            W[i] = signals[0][i] + signals[1][i] + signals[2][i] + signals[3][i];
            //1次
            Z[i] = signals[0][i] + signals[1][i] - signals[2][i] - signals[3][i];
            X[i] = -(signals[0][i] - signals[1][i] + signals[2][i] - signals[3][i]);
            Y[i] = signals[0][i] - signals[1][i] - signals[2][i] + signals[3][i];
        }

        //STFT
        var stftW = MathFFTW.STFT(W, overlap, nfft, MathFFTW.WindowFunc.Hann);
        var stftX = MathFFTW.STFT(X, overlap, nfft, MathFFTW.WindowFunc.Hann);
        var stftY = MathFFTW.STFT(Y, overlap, nfft, MathFFTW.WindowFunc.Hann);
        var stftZ = MathFFTW.STFT(Z, overlap, nfft, MathFFTW.WindowFunc.Hann);
        size = stftW.Length;
        Vector3[] intensities = new Vector3[size];
        
        Parallel.For(0, size, i => 
        {
            var stftW_ins = stftW[i];
            var stftX_ins = stftX[i];
            var stftY_ins = stftY[i];
            var stftZ_ins = stftZ[i];
            //FFT結果の平均化
            for (int fftIndex = 0; fftIndex < nfft * 2; fftIndex++)
            {
                stftW_ins[fftIndex] /= nfft;
                stftX_ins[fftIndex] /= nfft;
                stftY_ins[fftIndex] /= nfft;
                stftZ_ins[fftIndex] /= nfft;
            }

            float iix = 0;
            float iiy = 0;
            float iiz = 0;
            for (int fftindex = fftIndexMin; fftindex <= fftIndexMax; fftindex += 2)
            {
                //conj(W)*[X,Y,Z]の実部
                double sx = 4 * stftW_ins[fftindex] * stftX_ins[fftindex] + stftW_ins[fftindex + 1] * stftX_ins[fftindex + 1];
                double sy = 4 * stftW_ins[fftindex] * stftY_ins[fftindex] + stftW_ins[fftindex + 1] * stftY_ins[fftindex + 1];
                double sz = 4 * stftW_ins[fftindex] * stftZ_ins[fftindex] + stftW_ins[fftindex + 1] * stftZ_ins[fftindex + 1];

                iix -= (float)sx / ((float)atmDensity * c * 2);
                iiy -= (float)sy / ((float)atmDensity * c * 2);
                iiz -= (float)sz / ((float)atmDensity * c * 2);
            }
            intensities[i] = new Vector3(iix, iiy, iiz);
        });

        return intensities;
    }
