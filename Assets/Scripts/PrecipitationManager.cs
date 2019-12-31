using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrecipitationManager : MonoBehaviour
{
    public RasterManager precRaster;
    public float rainfallModifier = 10.0f;

    public static PrecipitationManager precMan;
    public Texture2D[] monthelyRainfallRaster = new Texture2D[12];
    public int currentMonth; //1 to 12

    public UnityEngine.UI.Slider slider;


    void Start()
    {
        if (precMan == null)
        {
            precMan = this;
            SwitchMonth(1);
        }
        else
            Destroy(this.gameObject);
    }


    void Update()
    {




    }


    public void UpdateVisualization()
    {
        foreach (Transform node in this.transform)
        {
            //node.gameObject.GetComponent<ParticleSystem>().emission.rateOverTime = GetPrecipitationRate(node.position);
            node.gameObject.GetComponent<RainParticleNode>().UpdateRate(GetPrecipitationRate(node.position) * rainfallModifier);
        }

    }


    float GetPrecipitationRate(Vector3 position)
    {
        float rate = 0.0f;

        //Vector4 rasterVal = precRaster.SampleRaster(this.transform.position);
        //if (rasterVal.w <= 0.01f) //SampleRaster returns Vector4 with 0 alpha value when sample is out of scope
        //{
        //    return -999.9f;
        //}

        rate = precRaster.SampleRasterForRegion(position, 1.0f); //TODO modify radius to be calculated form rainfall cells size
        return rate;
    }

    public void SwitchMonth(int month) //1 to 12, else clamped.
    {
        month -= 1;
        month = Mathf.Clamp(month, 0, 11);
        currentMonth = month + 1;
        if (monthelyRainfallRaster[month] != null)
        {
            precRaster.raster = monthelyRainfallRaster[month];
            slider.value = month + 1;
            UpdateVisualization();
        }
        else
        {
            print("ERROR: not rainfall raster is set for month: " + month + 1);
        }
        precRaster.UpdateRasterHolder();
        UIManager.uiMan.UpdateMonthTextFromSlider();
        UIManager.uiMan.UpdateMapStatistics();
    }

    public void SwitchMonthOffset(int offset)
    {
        SwitchMonth(currentMonth + offset);
    }

    public void UpdateMonthFromUI()
    {
        //print(slider.value);
        SwitchMonth((int)slider.value);
    }

}
