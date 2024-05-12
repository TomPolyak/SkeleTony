using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitFps : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
    }

    void Update()
    {
        
    }
}
