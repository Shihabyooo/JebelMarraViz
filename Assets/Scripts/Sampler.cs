using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sampler : MonoBehaviour
{

    public RasterManager[] dataSetList;

    // Start is called before the first frame update
    public virtual void Start()
    {
        
    }

    // Update is called once per frame
    public virtual void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    GetSample();
        //}
    }


    public virtual void GetSample()
    {
        foreach (RasterManager raster in dataSetList)
        {
           Vector4 sampledColor = raster.SampleRaster(this.transform.position);
           print("Sampled for layer" + raster.planeSpecs.level + " is: " + sampledColor);
        }
    }

}
