using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

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

        List<IntensityPanel> panels = new List<IntensityPanel>();
        List<Color> colors = new List<Color>();
        List<Vector3> scales = new List<Vector3>();
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
            var interact = VectorObj.GetComponent<Interactable>();
            if (interact != null)
                interact.IsEnabled = !UIManager._measure;

            var panel = VectorObj.GetComponentInChildren<IntensityPanel>();
            if (panel != null)
            {
                panel.UpdatePanel(this, intensity, No, micPos);
                panels.Add(panel);
            }
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
            var panel = VectorObj.GetComponentInChildren<IntensityPanel>();
            if (panel != null)
            {
                panel.UpdatePanel(this, package);
                panels.Add(panel);
            }
            intensities.Add(package.num, VectorObj);
            measureNo = package.num;
            return msPoint;
        }

        public GameObject CreateInstantIIntensityObj(TransIntensityPackage package, Color vecColor, float objSize)
        {
            //時間平均したインテンシティのobjectを一時的に表示
            var msPoint = CreateInstantObj(package.num, package.sendPos, package.sendRot, package.sumIntensity, vecColor, objSize);
            var storage = msPoint.AddComponent<IntensityObject>();
            storage.child = msPoint.transform.GetChild(0).gameObject;
            //瞬時音響インテンシティをリストに保持させる
            colors.Clear();
            scales.Clear();

            foreach (var iintensiy in package.IIntensities)
            {
                var intensityLv = AIMath.CalcuIntensityLevel(iintensiy);
                colors.Add(ColorBar.DefineColor(Holo2MeasurementParameter.ColorMapID, intensityLv, Holo2MeasurementParameter.LevelMin, Holo2MeasurementParameter.LevelMax));
                scales.Add(DefineSize(intensityLv, Holo2MeasurementParameter.LevelMin, Holo2MeasurementParameter.LevelMax));
            }
            storage.PushSumIntensity(package.sumIntensity, vecColor);
            storage.PushInstantIntensity(package.IIntensities, colors, scales);

            return msPoint;
        }

        /// <summary>
        /// 色変更
        /// </summary>
        /// <param name="No"></param>
        /// <param name="newIntensity"></param>
        /// <param name="color"></param>
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
                Destroy(intensities[dNum].transform.parent.gameObject);
                panels.RemoveAt(dNum);
                intensities.Remove(dNum);
            }
        }

        //削除申請
        public void DeleteAnnounceIntensity(int dNum)
        {
            var clientManager = this.GetComponent<Holo2ClientManager>();
            if (clientManager != null)
            {
                DeleteVectorObj(dNum);
                clientManager.SendDeleteData(dNum);
            }
        }

        //Hand Menuアイコン
        //IntensityPanelのプレートを一括非表示
        public void InactiveIntensityPanel()
        {
            foreach (var panel in panels)
            {
                if (panel.gameObject != null)
                    panel.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 大きさをインテンシティレベルのレンジに即して更新(瞬時音響インテンシティ時に使用)
        /// </summary>
        /// <param name="level"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private Vector3 DefineSize(float level, float min, float max)
        {
            float colorScale = (level - min) / (max - min);
            if (colorScale > 1)
            {
                return new Vector3(1f, 1f, 4f);
            }
            else if (colorScale >= 0 && colorScale <= 1)
            {
                return new Vector3(colorScale, colorScale, colorScale * 4f);
            }
            else
            {
                return Vector3.zero;
            }
        }
        /// <summary>
        /// インテンシティの情報を含むパネルを計測時以外にonにする
        /// </summary>
        public void PanelFocusSwitch()
        {
            foreach(var intensity in intensities.Values)
            {
                var interact = intensity.GetComponent<Interactable>();
                interact.IsEnabled = !UIManager._measure;
            }
        }
    }
}
