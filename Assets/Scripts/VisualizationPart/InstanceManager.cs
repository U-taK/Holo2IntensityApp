using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uOSC
{
    public class InstanceManager : MonoBehaviour
    {

        //表示するプレファブ
        public GameObject cone;
        //空間基準マーカ
        [SerializeField]
        GameObject standardMarker;
        static int measureNo = 0;
        //計測番号と計測結果の連想配列
        Dictionary<int, GameObject> intensities = new Dictionary<int, GameObject>();

        //オブジェクト生成
        public GameObject CreateInstantObj(int No, Vector3 micPos, Quaternion micRot, Vector3 intensity, Color vecColor, float objSize)
        {
            GameObject msPoint = new GameObject("measurepoint" + No);
            msPoint.transform.parent = standardMarker.transform;
            msPoint.transform.localPosition = micPos;
            msPoint.transform.localRotation = micRot;

            GameObject VectorObj = Instantiate(cone) as GameObject;
            VectorObj.transform.localScale = new Vector3(objSize, objSize, objSize * 4f);
            VectorObj.transform.parent = msPoint.transform;
            VectorObj.transform.localPosition = Vector3.zero;
            VectorObj.transform.localRotation = Quaternion.LookRotation(10000000000 * intensity);
            VectorObj.transform.GetComponent<Renderer>().material.color = vecColor;
            VectorObj.name = "IntensityObject";
            intensities.Add(No, VectorObj);
            measureNo = No;
            return msPoint;
        }

        public GameObject CreateInstantObj(IntensityPackage package, Color vecColor, float objSize)
        {
            GameObject msPoint = new GameObject("measurepoint" + package.num);
            msPoint.transform.parent = standardMarker.transform;
            msPoint.transform.localPosition = package.sendPos;
            msPoint.transform.localRotation = package.sendRot;

            GameObject VectorObj = Instantiate(cone) as GameObject;
            VectorObj.transform.localScale = new Vector3(objSize, objSize, objSize * 4f);
            VectorObj.transform.parent = msPoint.transform;
            VectorObj.transform.localPosition = Vector3.zero;
            VectorObj.transform.localRotation = Quaternion.LookRotation(10000000000 * package.intensity);
            VectorObj.transform.GetComponent<Renderer>().material.color = vecColor;
            VectorObj.name = "IntensityObject";
            intensities.Add(package.num, VectorObj);
            measureNo = package.num;
            return msPoint;
        }

        //色変更
        public void ChangeIntensityObj(int No, Vector3 newIntensity, Color color)
        {
            if (intensities.ContainsKey(No))
            {
                var pushObj = intensities[No];
                //色変更を行う
                pushObj.transform.localRotation = Quaternion.LookRotation(10000000000 * newIntensity);
                pushObj.transform.GetComponent<Renderer>().material.color = color;
            }
        }

        //削除
        public void DeleteVectorObj(int dNum)
        {
            if (intensities.ContainsKey(dNum))
            {
                intensities.Remove(dNum);
            }
        }
    }
}
