using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sampler : MonoBehaviour
{
    public RasterManager[] dataSetList;

    public virtual void GetSample()
    {
        foreach (RasterManager raster in dataSetList)
        {
           Vector4 sampledColor = raster.SampleRaster(this.transform.position);
           print("Sampled for layer" + raster.planeSpecs.level + " is: " + sampledColor);
        }
    }

}
