using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManage : MonoBehaviour{

    public Scene scene;

	// Use this for initialization
	void Start () {
        scene = SceneManager.GetActiveScene();
        DontDestroyOnLoad(this);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Q))
            ReturnMenu();
    }
    private void ReturnMenu()
    {
        MoveScene("StartScene");
        Destroy(this.gameObject);
    }

    public void MoveScene(string sName)
    {
        SceneManager.LoadScene(sName);

    }
}
