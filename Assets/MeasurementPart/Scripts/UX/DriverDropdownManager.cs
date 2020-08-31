using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AsioCSharpDll;
using System;
using System.Runtime.InteropServices;

public class DriverDropdownManager : MonoBehaviour
{
    Dropdown driverDropdown;

    // Start is called before the first frame update
    void Start()
    {
        driverDropdown = GetComponent<Dropdown>();
        LoadAdioDriver();
        driverDropdown.onValueChanged.AddListener(AsioDriverChange);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LoadAdioDriver()
    {
        driverDropdown.ClearOptions();
        int asioDriverSum = asiocsharpdll.GetAsioDriverSum();
        for(int i = 0; i < asioDriverSum; i++)
        {
            IntPtr ptrAsioName = asiocsharpdll.GetAsioDriverNames(i);
            string tempAsioDriverName = Marshal.PtrToStringAnsi(ptrAsioName);
            driverDropdown.options.Add(new Dropdown.OptionData { text = tempAsioDriverName });
        }
        driverDropdown.RefreshShownValue();
    }

    private void AsioDriverChange(int ID)
    {
        MeasurementParameter.AsioDriverName = driverDropdown.options[ID].text;
        Debug.Log("Change to" + driverDropdown.options[ID].text);
    }
}
