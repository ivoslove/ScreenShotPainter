using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGraffitiComponent : MonoBehaviour
{
    public bool isFullScreen;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void FullScreenGraffiti()
    {
        ScreenShotPainter.instance.SwitchOn(true);

    }
    public void FreeScreenGraffiti()
    {
        ScreenShotPainter.instance.SwitchOn(false);
    }

}
