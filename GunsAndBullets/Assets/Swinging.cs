using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swinging : MonoBehaviour
{
    [Header("Input")]
    public KeyCode swingKey = KeyCode.Mouse0;

    void Update()
    {
        if(Input.GetKeyDown(swingKey))StartSwing();
        if(Input.GetKeyUp(swingKey))StartSwing();
    }
    private void StartSwing()
    {

    }

    private void StopSwing()
    {

    }
}
