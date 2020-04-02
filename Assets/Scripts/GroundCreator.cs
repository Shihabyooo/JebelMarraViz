using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCreator : MonoBehaviour
{

    public RasterManager groundPlane;
    public GameObject groundTile; //to see that the reader works
    public float elevationModifier = 1.0f;
    //GameObject groundTilesContainer;
    //public float[][] elevationsGrid { get; private set; }
    public float[][] elevationsGrid;

    public float resolution = 10.0f; //tile size
    public UnityEngine.UI.Slider resoSlider;

    MeshCreator meshCreator;

    public static GroundCreator grCreator;

    // Start is called before the first frame update
    void Awake()
    {
        if (grCreator == null)
        {
            grCreator = this;
            meshCreator = this.gameObject.GetComponent<MeshCreator>();
            Vector3 swCorner = new Vector3(groundPlane.planeSpecs.minX, 0.0f, groundPlane.planeSpecs.minY);
            meshCreator.CreateGroundMesh(swCorner, resolution, groundPlane.planeSpecs.width * 10.0f, groundPlane.planeSpecs.height * 10.0f); //creates a flat plane but with required resolution
            resoSlider.value = (int)resolution;
            //UpdateTopography();
        }
        else
            Destroy(this.gameObject);
    }

    public void UpdateTopography()
    {
        int pointsCounterX, pointsCounterY;
        float pixelSize = groundPlane.planeSpecs.spacingBetweenPixels;

        ////assumes square pixels;
        pointsCounterX = Mathf.RoundToInt(groundPlane.planeSpecs.width * 10.0f / resolution);
        pointsCounterY = Mathf.RoundToInt(groundPlane.planeSpecs.height * 10.0f / resolution);

        elevationsGrid = new float[pointsCounterX][];
        for (int k = 0; k < pointsCounterX; k++)
        {
            elevationsGrid[k] = new float[pointsCounterY];
        }

        Vector3 cornerXYZ = new Vector3(groundPlane.planeSpecs.minX, 0.0f, groundPlane.planeSpecs.minY);
        Vector3 topoXYZ = cornerXYZ;

        for (int i = 0; i < pointsCounterX; i ++)
        {
            for (int j = 0; j < pointsCounterY; j++)
            {
                topoXYZ = cornerXYZ;
                topoXYZ.x += i * resolution;
                topoXYZ.z += j * resolution;

                topoXYZ.y = (groundPlane.SampleRasterForRegion(topoXYZ, pixelSize * 2.0f) * 2004.0f) + 1000.0f; 

                elevationsGrid[i][j] = topoXYZ.y;
            }
        }

        //print("Attempting to recreate Mesh with new elevations");
        
        meshCreator.CreateGroundMesh(cornerXYZ, resolution, groundPlane.planeSpecs.width * 10.0f, groundPlane.planeSpecs.height * 10.0f, ref elevationsGrid);

        //modify vertical scale with elevationModifier
        Vector3 tempRescalerVec = this.transform.localScale;
        tempRescalerVec.y = elevationModifier;
        this.transform.localScale = tempRescalerVec;
    }

    public void SetResolutionFromSlider()
    {
        resolution = (float)resoSlider.value;
    }

    public void SwitchResolutionOffset(int offset)
    {
        resolution = Mathf.Clamp((float)(Mathf.FloorToInt(resolution) + offset), resoSlider.minValue, resoSlider.maxValue);
        //consider setting public global consts in this thread that resoSlider updates its min/max values from at start\

        resoSlider.value = (int)resolution;
    }

}
