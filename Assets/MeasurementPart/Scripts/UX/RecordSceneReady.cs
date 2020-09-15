///////////////////////
///Asioの挙動確認用
///////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordSceneReady : MonoBehaviour
{
    [SerializeField]
    RecordSceneManager calibManager;

    Text disp_text;

    // Start is called before the first frame update
    void Start()
    {
        disp_text = GetComponent<Text>();
        calibManager.OnAsioReady += AsioReady;
        calibManager.AsioNotReady += AsioNotReady;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void AsioReady(EventArgs args)
    {
        disp_text.text = "Asio Ready";
        disp_text.color = Color.green;
    }
    void AsioNotReady(EventArgs args)
    {
        disp_text.text = "Failed to start Asio.Confirm the value of fs or sampling num";
        disp_text.color = Color.yellow;

    }
}
