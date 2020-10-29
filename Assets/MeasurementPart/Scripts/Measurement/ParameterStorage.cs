using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParameterStorage : MonoBehaviour {

        //計算した音響インテンシティの値を座標ごとに保持しておく
        Vector3[] soundIntensity;
        float[] intensityLevel;
        int i = 0;
        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            if (Input.GetKey(KeyCode.O))
            {
                ParameterChange();
                CallIntensity();
            }
            if (Input.GetKey(KeyCode.P))
            {
                ParameterChange();
                CallIntensityBack();
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                Vector3 sum = Vector3.zero;
                for (int count = 0; count < soundIntensity.Length; count++)
                {
                    sum += soundIntensity[count];
                }
                var sumLevel = AcousticMathNew.CalcuIntensityLevel(sum);
                Debug.Log("Sound intensity of average is " + sumLevel);
                transform.localRotation = Quaternion.LookRotation(sum * 10000000000);
                transform.localScale = new Vector3(MeasurementParameter.objSize, MeasurementParameter.objSize, MeasurementParameter.objSize*4);
                Color vecObjColor = ColorBar.DefineColor(1, sumLevel, MeasurementParameter.lvMin, MeasurementParameter.lvMax);
                gameObject.GetComponent<Renderer>().material.color = vecObjColor;
                i = 0;
            }

        }

        public void PutIntensity(Vector3[] intensity, float[] intensityLv)
        {
            soundIntensity = intensity;
            intensityLevel = intensityLv;
        }

        public Vector3[] PushIntensity()
        {
            return soundIntensity;
        }

        public void CallIntensity()
        {
            Debug.Log("No." + i + "intensity is" + soundIntensity[i] + ",intensityLevel is" + intensityLevel[i]);
            if (i < soundIntensity.Length)
                i++;
            else
                i = 0;

        }

        public void CallIntensityBack()
        {
            Debug.Log("No." + i + "intensity is" + soundIntensity[i] + ",intensityLevel is" + intensityLevel[i]);
            if (i > 0)
                i--;
            else
                i = 0;

        }

        void ParameterChange()
        {
            transform.localRotation = Quaternion.LookRotation(soundIntensity[i] * 10000000000);
            transform.localScale = new Vector3(MeasurementParameter.objSize, MeasurementParameter.objSize, MeasurementParameter.objSize*4);
            Color vecObjColor = ColorBar.DefineColor(1, intensityLevel[i], MeasurementParameter.lvMin, MeasurementParameter.lvMax);
            gameObject.GetComponent<Renderer>().material.color = vecObjColor;
        }
}
