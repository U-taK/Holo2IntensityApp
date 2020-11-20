using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml;
using System.Threading.Tasks;
using FFTWSharp;
using UnityEngine;

class MathFFTW
{
    public enum WindowFunc
    {
        Hann,
        Hanning,
        Blackman,
        Rectangular
    }
    /// <summary>
    /// FFTWを使ったクロススペクトル法
    /// FFTWの返り値の配列サイズは入力信号の倍になる(偶数が実数、奇数が虚数)
    /// </summary>
    /// <param name="sound_signals">入力信号(4ch)</param>
    /// <param name="fs">サンプリング周波数//</param>
    /// <param name="freq_range_min">周波数帯域下限</param>
    /// <param name="freq_range_max">周波数帯域上限</param>
    /// <param name="atmDensity">大気密度</param>
    /// <param name="dr">マイク間距離</param>
    /// <returns></returns>
    public static Vector3 CrossSpectrumMethod(double[][] sound_signals, int fs, float freq_range_min, float freq_range_max, double atmDensity, float dr)
    {
        //サンプル長を求める
        int sampleLength = sound_signals[0].Length;

        double[][] fft = new double[4][];

        for (int micID = 0; micID < 4; micID++)
        {
            //虚数を含めるから2倍の長さを確保
            fft[micID] = new double[sampleLength*2];
            fft[micID] = FFT(sound_signals[micID], true);
            //FFT結果を平均化
            for (int fftIndex = 0; fftIndex < fft[micID].Length; fftIndex++)
                fft[micID][fftIndex] /= sampleLength;

        }

        //FFTした時のサンプル範囲を求める
        float df = (float)fs / sampleLength;
        int fftIndexMin = Mathf.CeilToInt(freq_range_min / df)*2;
        int fftIndexMax = Mathf.FloorToInt(freq_range_max / df)*2;

        //インテンシティの積分範囲を求める
        double sig01 = 0d;
        double sig02 = 0d;
        double sig03 = 0d;
        double sig12 = 0d;
        double sig13 = 0d;
        double sig23 = 0d;
        for (int fftIndex = fftIndexMin; fftIndex <= fftIndexMax; fftIndex+=2)
        {
            //ナイキスト周波数の時だけ計算(虚数がfftの結果に含まれるのでsampleLength*2/2)のイメージ)
            if (fftIndex <= sampleLength)
            {
                //両側クロススペクトルの虚部
                double imS01 = fft[0][fftIndex] * fft[1][fftIndex+1] - fft[1][fftIndex] * fft[0][fftIndex+1];
                double imS02 = fft[0][fftIndex] * fft[2][fftIndex+1] - fft[2][fftIndex] * fft[0][fftIndex+1];
                double imS03 = fft[0][fftIndex] * fft[3][fftIndex+1] - fft[3][fftIndex] * fft[0][fftIndex+1];
                double imS12 = fft[1][fftIndex] * fft[2][fftIndex+1] - fft[2][fftIndex] * fft[1][fftIndex+1];
                double imS13 = fft[1][fftIndex] * fft[3][fftIndex+1] - fft[3][fftIndex] * fft[1][fftIndex+1];
                double imS23 = fft[2][fftIndex] * fft[3][fftIndex+1] - fft[3][fftIndex] * fft[2][fftIndex+1];

                //両側クロススペクトル虚部 -> 片側クロススペクトル虚部
                double imG01;
                double imG02;
                double imG03;
                double imG12;
                double imG13;
                double imG23;
                if (fftIndex == 0 || fftIndex == sampleLength)
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
                sig01 += imG01 / (fftIndex * df / 2);
                sig02 += imG02 / (fftIndex * df / 2);
                sig03 += imG03 / (fftIndex * df / 2);
                sig12 += imG12 / (fftIndex * df / 2);
                sig13 += imG13 / (fftIndex * df / 2);
                sig23 += imG23 / (fftIndex * df / 2);
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
    /// Computed fft of 1D array of real or complex numbers
    /// </summary>
    /// <param name="data">Input data</param>
    /// <param name="real">Real or complex flag</param>
    /// <returns>Return the fft.</returns>
    public static double[] FFT(double[] data, bool real)
    {
        //if the input is real, make it complex
        if (real)
            data = ToComplex(data);
        //Get the length of the array
        int n = data.Length;

        /* Allocate an unmanaged memory block for the input and output data.
         * (The input and output are of the same length in this case, so we can use just one memory block.)*/
        IntPtr ptr = fftw.malloc(n * sizeof(double)); //or n * sizeof(double)
        //Pass the managed input data to the unmanged memory block
        Marshal.Copy(data, 0, ptr, n);
        //Plan the FFT and execute it (n/2 because complex numbers are stored as pairs of doubles)
        IntPtr plan = fftw.dft_1d(n / 2, ptr, ptr, fftw_direction.Forward, fftw_flags.Estimate);
        fftw.execute(plan);
        //Create an array to store the output values
        var fft = new double[n];
        //Pass the unmanaged output data to the managed array
        Marshal.Copy(ptr, fft, 0, n);
        //Do some cleaning
        fftw.destroy_plan(plan);
        fftw.free(ptr);
        fftw.cleanup();
        //Return the FFT output
        return fft;
    }

    public static double[][] FFT2D(double[][] data, bool real)
    {
        double[][] fft = new double[data.Length][];
        for(int i = 0; i < data.Length; i++)
        { 
            fft[i] = FFT(data[i], real);
        }
        return fft;
    }

    public static double[][] STFT(double[] data, int overlap, int nfft, WindowFunc windowFunc)
    {
        var size = data.Length;
        double[][] stft = new double[size / overlap - 1][];
        double[] clip = new double[nfft];
        var k = 0;
        for (int idx = 0; idx <= size-nfft; idx += overlap)
        {
            //信号から切り抜き
            for (int j = 0; j < nfft; j++)
                clip[j] = data[idx + j];
            //window
            clip = Windowing(clip, WindowFunc.Hann);

            stft[k++] = FFT(clip, true);
        }

        return stft;
    }

    public static Vector3[] STFTmethod(double[][] sound_signals, int overlap, int nfft, int fs, float freq_range_min, float freq_range_max, double atmDensity, float dr)
    {
        
        //FFTした時のサンプル範囲を求める
        float df = (float)fs / nfft;
        int fftIndexMin = Mathf.CeilToInt(freq_range_min / df) * 2;
        int fftIndexMax = Mathf.FloorToInt(freq_range_max / df) * 2;

        //stft
        var stft0 = STFT(sound_signals[0], overlap, nfft, WindowFunc.Hann);
        var stft1 = STFT(sound_signals[1], overlap, nfft, WindowFunc.Hann);
        var stft2 = STFT(sound_signals[2], overlap, nfft, WindowFunc.Hann);
        var stft3 = STFT(sound_signals[3], overlap, nfft, WindowFunc.Hann);
        var size = stft0.Length;
        Vector3[] intensities = new Vector3[size];
        Parallel.For(0, size, i => 
        //for (int i = 0; i < size; i++)
        {
            var stft0_ins = stft0[i];
            var stft1_ins = stft1[i];
            var stft2_ins = stft2[i];
            var stft3_ins = stft3[i];

            //FFT結果を平均化
            for (int fftIndex = 0; fftIndex < nfft * 2; fftIndex++)
            {
                stft0_ins[fftIndex] /= nfft;
                stft1_ins[fftIndex] /= nfft;
                stft2_ins[fftIndex] /= nfft;
                stft3_ins[fftIndex] /= nfft;
            }

            //インテンシティの積分範囲を求める
            double sig01 = 0d;
            double sig02 = 0d;
            double sig03 = 0d;
            double sig12 = 0d;
            double sig13 = 0d;
            double sig23 = 0d;
            for (int fftIndex = fftIndexMin; fftIndex <= fftIndexMax; fftIndex += 2)
            {
                //ナイキスト周波数の時だけ計算(虚数がfftの結果に含まれるのでsampleLength*2/2)のイメージ)
                if (fftIndex <= nfft)
                {
                    //両側クロススペクトルの虚部
                    double imS01 = stft0_ins[fftIndex] * stft1_ins[fftIndex + 1] - stft1_ins[fftIndex] * stft0_ins[fftIndex + 1];
                    double imS02 = stft0_ins[fftIndex] * stft2_ins[fftIndex + 1] - stft2_ins[fftIndex] * stft0_ins[fftIndex + 1];
                    double imS03 = stft0_ins[fftIndex] * stft3_ins[fftIndex + 1] - stft3_ins[fftIndex] * stft0_ins[fftIndex + 1];
                    double imS12 = stft1_ins[fftIndex] * stft2_ins[fftIndex + 1] - stft2_ins[fftIndex] * stft1_ins[fftIndex + 1];
                    double imS13 = stft1_ins[fftIndex] * stft3_ins[fftIndex + 1] - stft3_ins[fftIndex] * stft1_ins[fftIndex + 1];
                    double imS23 = stft2_ins[fftIndex] * stft3_ins[fftIndex + 1] - stft3_ins[fftIndex] * stft2_ins[fftIndex + 1];

                    //両側クロススペクトル虚部 -> 片側クロススペクトル虚部
                    double imG01;
                    double imG02;
                    double imG03;
                    double imG12;
                    double imG13;
                    double imG23;
                    if (fftIndex == 0 || fftIndex == nfft)
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
                    sig01 += imG01 / (fftIndex * df / 2);
                    sig02 += imG02 / (fftIndex * df / 2);
                    sig03 += imG03 / (fftIndex * df / 2);
                    sig12 += imG12 / (fftIndex * df / 2);
                    sig13 += imG13 / (fftIndex * df / 2);
                    sig23 += imG23 / (fftIndex * df / 2);
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
            intensities[i] = new Vector3((float)ix, (float)iy, (float)iz);
        //}
        });
        return intensities;
    }

    /// <summary>
    /// Computes the ifft of 1D array of complex numbers
    /// </summary>
    /// <param name="data">Input data.</param>
    /// <returns>Returns the normalizedIFFT.</returns>
    public static double[] IFFT(double[] data)
    {      
        //Get the length of the array
        int n = data.Length;

        /* Allocate an unmanaged memory block for the input and output data.
         * (The input and output are of the same length in this case, so we can use just one memory block.)*/
        IntPtr ptr = fftw.malloc(n * sizeof(double));
        //Pass the managed input data to the unmanged memory block
        Marshal.Copy(data, 0, ptr, n); //or n * sizeof(double)
        //Plan the IFFT and execute it (n/2 because complex numbers are stored as pairs of doubles)
        IntPtr plan = fftw.dft_1d(n / 2, ptr, ptr, fftw_direction.Backward, fftw_flags.Estimate);
        fftw.execute(plan);
        //Create an array to store the output values
        var ifft = new double[n];
        //Pass the unmanaged output data to the managed array
        Marshal.Copy(ptr, ifft, 0, n);
        //Do some cleaning
        fftw.destroy_plan(plan);
        fftw.free(ptr);
        fftw.cleanup();
        //Scale the output values
        for (int i = 0, nh = n / 2; i < n; i++)
            ifft[i] /= nh; 
        //Return the IFFT output
        return ifft;
    }
    /// <summary>
    /// Interfaces an array with zeros to match the FFTW convention of representing complex numbers.
    /// </summary>
    /// <param name="real">An array of real numbers.</param>
    /// <returns>Returns an array of complex numbers.</returns>
    static double[] ToComplex(double[] real)
    {
        int n = real.Length;
        var comp = new double[n * 2];
        for (int i = 0; i < n; i++)
            comp[2 * i] = real[i];
        return comp;
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

    public static double[] Windowing(double[] data, WindowFunc windowFunc)
    {
        int size = data.Length;
        double[] windata = new double[size];

        for (int i = 0; i < size; i++)
        {
            double winValue = 0;

            //各々の窓関数
            switch (windowFunc)
            {
                case WindowFunc.Hann:
                    winValue = 0.5 - 0.5 * Math.Cos(2 * Math.PI * i / (size - 1));
                    break;
                case WindowFunc.Hanning:
                    winValue = 0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (size - 1));
                    break;
                case WindowFunc.Blackman:
                    winValue = 0.42 - 0.5 * Math.Cos(2 * Math.PI * i / (size - 1))
                        + 0.08 * Math.Cos(4 * Math.PI * i / (size - 1));
                    break;
                case WindowFunc.Rectangular:
                    winValue = 1.0;
                    break;
                default:
                    winValue = 1.0;
                    break;
            }
            //窓関数を掛け算
            windata[i] = data[i] * winValue;
        }
        return windata;
    }
}
