using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Compass : MonoBehaviour
{
    public static Compass compass;

    Quaternion originalRot;
    void Awake()
    {
        if (compass == null)
        {
            compass = this;
            originalRot = this.transform.rotation;
            //camera = Camera.main.transform;
            //this.transform.rotation = camera.rotation;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //this.transform.rotation = Quaternion camera.rotation;
        
    }

    public void RotateCompass(float angle)
    {
        this.transform.Rotate(Vector3.forward, angle);
    }

    public void ResetRotation()
    {
        this.transform.rotation = originalRot;
    }
}
