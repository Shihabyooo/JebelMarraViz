using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrecipitationManager : MonoBehaviour
{
    public RasterManager precRaster;
    public float precipitationModifier = 200.0f; //TODO recheck the max values of all the rainfall rasters (the mm rainfall value corrosponding to fully-white pixels) and set
                                                //its value here. No need for anything else, since total black corrosponds to 0mm precipitation in rasters.
    public static PrecipitationManager precMan;
    public Texture2D[] monthelyRainfallRaster = new Texture2D[12];
    public int currentMonth; //1 to 12
    public UnityEngine.UI.Slider slider;
    RainParticleNode[] rainNodes;
    CloudParticleNode[] cloudNodes;

    void Awake()
    {
        if (precMan == null)
            precMan = this;
        else
            Destroy(this.gameObject);

        PopulateNodesHolders();
        SwitchMonth(1);
    }

    void PopulateNodesHolders()
    {
        Transform rainArray = this.transform.Find("RainArray");
        Transform cloudArray = this.transform.Find("CloudArray");

        //Assuming that different resultion is used for rain arrays and cloud arrays
        rainNodes = new RainParticleNode[rainArray.childCount];
        cloudNodes = new CloudParticleNode[cloudArray.childCount];

        for (int i = 0; i < rainNodes.Length; i++)
            rainNodes[i] = rainArray.GetChild(i).gameObject.GetComponent<RainParticleNode>();

        for (int i = 0; i < cloudNodes.Length; i++)
            cloudNodes[i] = cloudArray.GetChild(i).gameObject.GetComponent<CloudParticleNode>();
    }

    public void UpdateVisualization()
    {
        // foreach (Transform node in this.transform)
        //     node.gameObject.GetComponent<RainParticleNode>().UpdateRate(GetPrecipitationRate(node.position) * rainfallModifier);

        foreach (RainParticleNode node in rainNodes)
            node.UpdateRate(GetPrecipitationRate(node.transform.position) * precipitationModifier);

        foreach (CloudParticleNode node in cloudNodes)
            node.UpdateRate(GetPrecipitationRate(node.transform.position) * precipitationModifier);
    }

    float GetPrecipitationRate(Vector3 position)
    {
        float rate = 0.0f;
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
        SwitchMonth((int)slider.value);
    }

}
