using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RasterManager : MonoBehaviour
{

  
    public Texture2D raster;

    [System.Serializable]
    public struct RasterHolderSpecs
    {
        public float width;
        public float height;

        public float maxX;
        public float minX;
        public float maxY;
        public float minY;

        public float level;

        public float spacingBetweenPixels;

        public float averageValue; //based on raw raster value
        public float valueStDev; //based on raw raster value
    }

    public RasterHolderSpecs planeSpecs = new RasterHolderSpecs();
   

    // Start is called before the first frame update
    void Start()
    {
        UpdateRasterHolder();
    }

    // Update is called once per frame
    void Update()
    {
        //print("Image: " + raster.width + " x " + raster.height);
    }


    public void UpdateRasterHolder()
    {
        float ratio = 1.0f;

        if (raster != null)
            ratio = (float)raster.height / (float)raster.width;

        planeSpecs.width = 100.0f;
        planeSpecs.height = ratio * 100.0f;

        planeSpecs.maxX = 10.0f * (this.transform.position.x + planeSpecs.width / 2.0f);
        planeSpecs.minX = 10.0f * (this.transform.position.x - planeSpecs.width / 2.0f);

        planeSpecs.maxY = 10.0f * (this.transform.position.z + planeSpecs.height / 2.0f);
        planeSpecs.minY = 10.0f * (this.transform.position.z - planeSpecs.height / 2.0f);

        planeSpecs.spacingBetweenPixels = planeSpecs.width / raster.width;

        Vector3 newScale = new Vector3(planeSpecs.width,
                                       1.0f,
                                       planeSpecs.height);

        this.transform.localScale = newScale;

        this.gameObject.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", raster);


        planeSpecs.averageValue = CalculateRasterAverageValue();
        planeSpecs.valueStDev = CalculateRasterStDev();
    }

    void OnValidate()
    {
        UpdateRasterHolder();
    }
    


    public float SampleRasterForRegion(Vector3 centrePoint, float radius)
    {
        float averagedSample = 0.0f;

        //there should be a fitting process here, but since this is a very quick and dirty visualization...


        Vector3 swCorner = new Vector3(centrePoint.x - radius,
                                        0.0f,
                                        centrePoint.z - radius);
        int noOfPixelsSampledInAxis = Mathf.RoundToInt(radius * 2.0f / planeSpecs.spacingBetweenPixels);
        noOfPixelsSampledInAxis = Mathf.Clamp(noOfPixelsSampledInAxis, 1, noOfPixelsSampledInAxis);
        int counter = 0;

        for (int i = 0; i < noOfPixelsSampledInAxis; i++)
        {
            for (int j = 0; j < noOfPixelsSampledInAxis; j++)
            {
                Vector3 tempPos = swCorner + new Vector3(i * planeSpecs.spacingBetweenPixels, 0.0f, j * planeSpecs.spacingBetweenPixels);
                Vector4 tempVec = SampleRaster(tempPos);
                averagedSample += tempVec.x; //assumes a grayscale in which r = g = b
                //print("Magnitude: " + tempVec.magnitude);
                if (tempVec.magnitude >= 0.01f)
                    counter++;
            }
        }

        if (counter != 0) //else would retern NaN
            averagedSample /= (float)counter;

        //print("returning average sample of: " + averagedSample);

        //print("returning an averageSample: " + averagedSample);

        return averagedSample;
    }

    public Vector4 SampleRaster(Vector2 position)//, bool useBilinear = true)
    {
        if (raster == null)
            return Vector4.zero;

        //print("Sampling at: " + position);

        //if (useBilinear)
            return raster.GetPixelBilinear(position.x, position.y);

        //return raster.GetPixel(position.x, position.y);

    }

    public Vector4 SampleRaster(Vector3 position)//, bool useBilinear = true)
    {
        Vector2 texSpacePos = new Vector2();

        //test if position within plane coverage
        if (position.x > planeSpecs.maxX || position.x < planeSpecs.minX ||
            position.z > planeSpecs.maxY || position.z < planeSpecs.minY)
        {
            //print("Sample coords out of plane coverage");
            return Vector4.zero;
        }

        //convert world 3D coords to normalized 2D coords
        texSpacePos.x = ((position.x - planeSpecs.minX) / planeSpecs.width) / 10.0f;
        texSpacePos.y = ((position.z - planeSpecs.minY) / planeSpecs.height) / 10.0f;


        return SampleRaster(texSpacePos);//, false);
    }

    float CalculateRasterAverageValue()
    {
        float average = 0.0f;

        for (int i = 0; i < raster.width; i++)
        {
            for (int j = 0; j < raster.height; j++)
            {
                average += raster.GetPixel(i, j).r; //assumes monochromatic raster (i.e. r = g = b);
            }
        }

        average /= (raster.height * raster.width);

        return average;
    }

    float CalculateRasterStDev()
    {
        float std = 0.0f;

        for (int i = 0; i < raster.width; i++)
        {
            for (int j = 0; j < raster.height; j++)
            {
                std += Mathf.Pow((raster.GetPixel(i, j).r - planeSpecs.averageValue), 2.0f); //assumes monochromatic raster (i.e. r = g = b);
            }
        }

        std /= (raster.height * raster.width) - 1;
        std = Mathf.Sqrt(std);

        return std;
    }

}
