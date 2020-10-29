///<summary>
///瞬時音響インテンシティ導出
/// </summary>



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AcousticSI{

    /// <summary>
    /// 瞬時音響インテンシティを直接法と同様に算出
    /// </summary>
    /// 

    public static Vector3[] DirectMethod(double[][] sound_signals, float atmDensity, float dr)
    {
        double[] u01 = ParticleVer(sound_signals[0], sound_signals[1], atmDensity, dr);
        double[] u02 = ParticleVer(sound_signals[0], sound_signals[2], atmDensity, dr);
        double[] u03 = ParticleVer(sound_signals[0], sound_signals[3], atmDensity, dr);
        double[] u12 = ParticleVer(sound_signals[1], sound_signals[2], atmDensity, dr);
        double[] u13 = ParticleVer(sound_signals[1], sound_signals[3], atmDensity, dr);
        double[] u23 = ParticleVer(sound_signals[2], sound_signals[3], atmDensity, dr);

        //インテンシティ計算
        double[] i01 = InstantSI(sound_signals[0], sound_signals[1], u01);
        double[] i02 = InstantSI(sound_signals[0], sound_signals[2], u02);
        double[] i03 = InstantSI(sound_signals[0], sound_signals[3], u03);
        double[] i12 = InstantSI(sound_signals[1], sound_signals[2], u12);
        double[] i13 = InstantSI(sound_signals[1], sound_signals[3], u13);
        double[] i23 = InstantSI(sound_signals[2], sound_signals[3], u23);

        //左手系ワールド座標(x,y,z)に変換
        var intensity = new Vector3[i01.Length];

        for (int j = 0; j < i01.Length; j++)
        {
            var ix = (i01[j] - i02[j] + i23[j] - i13[j] - (2 * i12[j])) / 4;
            var iy = (i01[j] + i02[j] + i03[j]) / Mathf.Sqrt(6);
            var iz = (-i01[j] - i02[j] + (2 * i03[j]) + (3 * i13[j]) + (3 * i23[j])) / (4d * Mathf.Sqrt(3));
            intensity[j] = new Vector3((float)ix, (float)iy, (float)iz);
        }
        return intensity;
    }
    // Use this for initialization

    //粒子速度の算出
    private static double[] ParticleVer(double[] mic0, double[] mic1,float lo, float dr)
    {
        var particle = new double[mic0.Length];
        float t = 1f / 44100f;
        //音圧差の総和を残していく
        double sumDif = 0;
        for (int i = 0; i < mic0.Length; i++)
        {
            sumDif += mic1[i] - mic0[i];
            particle[i] = -sumDif * t / (lo * dr);
        }
        return particle;
    }
    //インテンシティ計算
    private static double[] InstantSI(double[] mic0, double[] mic1, double[] u)
    {
        var intensity = new double[u.Length];
        for (int i = 0; i < u.Length; i++)
        {
            intensity[i] = (mic0[i] + mic1[i]) * u[i] / 2;
        }
        return intensity;
    }

    //瞬時音響インテンシティの時間積分によって直接法の結果を得る
    public static Vector3 SumIntensity(Vector3[] instance)
    {
        Vector3 sum = Vector3.zero;
        for (int count = 0; count < instance.Length; count++)
        {
            sum += instance[count];
        }
        return sum/instance.Length;
    }
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
