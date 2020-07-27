using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicPositionMirror : MonoBehaviour
{
    [SerializeField]//マイクマーカによって取得する計測点位置
    GameObject micPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = micPos.gameObject.transform.position;
        this.transform.rotation = micPos.gameObject.transform.rotation;
    }

    //計測点の座標情報を逐次送信
    public void GetSendInfo(out Vector3 vector, out Quaternion quaternion) 
    {
        vector = this.transform.localPosition;
        quaternion = this.transform.localRotation;
    }
}
