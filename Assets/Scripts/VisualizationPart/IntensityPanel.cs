using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uOSC;

public class IntensityPanel : MonoBehaviour
{
    InstanceManager instanceManager;

    [SerializeField]
    TextMesh textMesh;
    //表示するUIの位置
    [SerializeField]
    float interval = 0.05f;
    //インテンシティ番号
    int ID;
    public void UpdatePanel(InstanceManager instanceManager,IntensityPackage package)
    {
        var intensityLv = AcousticMathNew.CalcuIntensityLevel(package.intensity);
        ID = package.num;
        this.instanceManager = instanceManager;
        textMesh.text = string.Format("num: {0}\n pos({1:F1},{2:F1},{3:F1})\n" +
            "intensity({4:F1},{5:F1},{6:F1})\n intensity Lv:{7:F1}dB", 
            package.num,package.sendPos.x, package.sendPos.y, package.sendPos.z,
            package.intensity.x,package.intensity.y,package.intensity.z,intensityLv);
        
        if (transform.position.y > 2)
        {
            transform.Translate(Vector3.down * interval, Space.World);
        }
        else
        {
            transform.Translate(Vector3.up * interval, Space.World);
        }
        this.gameObject.transform.parent = null;
        transform.localScale = Vector3.one * 0.8f;
        this.gameObject.SetActive(false);
    }

    public void UpdatePanel(InstanceManager instanceManager, Vector3 intensity, int num,  Vector3 sendPos)
    {
        var intensityLv = AcousticMathNew.CalcuIntensityLevel(intensity);
        ID = num;
        this.instanceManager = instanceManager;
        textMesh.text = string.Format("num: {0}\n pos({1:F1},{2:F1},{3:F1})\n" +
            "intensity({4:F1},{5:F1},{6:F1})\n intensity Lv:{7:F1}dB",
            num, sendPos.x, sendPos.y, sendPos.z,
            intensity.x, intensity.y, intensity.z, intensityLv);

        if (transform.position.y > 2)
        {
            transform.Translate(Vector3.down * interval, Space.World);
        }
        else
        {
            transform.Translate(Vector3.up * interval, Space.World);
        }
        this.gameObject.transform.parent = null;
        transform.localScale = Vector3.one * 0.8f;
        this.gameObject.SetActive(false);
    }
    // Start is called before the first frame update
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       // if (this.isActiveAndEnabled)
            //CustomBillboard();
    }

    public void DeleteIntensity()
    {
        instanceManager.DeleteAnnounceIntensity(ID);
        Destroy(this.gameObject);
    }

    void CustomBillboard()
    {
        Vector3 p = Camera.main.transform.position;
        p.y = transform.position.y;
        transform.LookAt(p);
    }

}
