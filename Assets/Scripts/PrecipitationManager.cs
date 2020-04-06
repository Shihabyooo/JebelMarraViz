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
    
    Transform rainArray;
    Transform cloudArray;
    [SerializeField] GameObject rainParticleSystemPrefab;
    [SerializeField] GameObject cloudParticleSystemPrefab;
    RainParticleNode[] rainNodes;
    CloudParticleNode[] cloudNodes;

    void Awake()
    {
        if (precMan == null)
            precMan = this;
        else
            Destroy(this.gameObject);

        rainArray = this.transform.Find("RainArray");
        cloudArray = this.transform.Find("CloudArray");

        GenerateEffectGrids();

        PopulateNodesHolders();
        SwitchMonth(1);
    }

    void GenerateEffectGrids()
    {
        //Assuming that: A- Rain nodes and Cloud nodes aren't of similar size, and B- both nodes aren't necessarily squared.
        float rainNodeWidth = rainParticleSystemPrefab.GetComponent<ParticleSystem>().shape.scale.x;
        float rainNodeHeight = rainParticleSystemPrefab.GetComponent<ParticleSystem>().shape.scale.z;
        float cloudNodeWidth = cloudParticleSystemPrefab.GetComponent<ParticleSystem>().shape.scale.x;
        float cloudNodeHeight = cloudParticleSystemPrefab.GetComponent<ParticleSystem>().shape.scale.z;

        //TODO figure out where this 10.0f multiplier you're using all over this project came from! :|
        float totalWidth = precRaster.planeSpecs.width * 10.0f;
        float totalHeight = precRaster.planeSpecs.height * 10.0f;

        Vector3 swCorner = precRaster.transform.position - new Vector3(totalWidth/2.0f, 0.0f, totalHeight/2.0f);

        int requiredRainNodesHorizontally = Mathf.CeilToInt(totalWidth / rainNodeWidth);
        int requiredRainNodesvertically = Mathf.CeilToInt(totalHeight / rainNodeHeight);
        int requiredCloudNodesHorizontally = Mathf.CeilToInt(totalWidth / cloudNodeWidth);
        int requiredCloudNodesvertically = Mathf.CeilToInt(totalHeight / cloudNodeHeight);

        // print ("rainNode: " + rainNodeWidth + " x " + rainNodeHeight);
        // print ("cloudNode: " + cloudNodeWidth + " x " + cloudNodeHeight);
        // print ("Total: " + totalWidth + " x " + totalHeight);
        // print ("required rain: " + requiredRainNodesHorizontally + " x " + requiredRainNodesvertically);
        // print ("required cloud: " + requiredCloudNodesHorizontally + " x " + requiredCloudNodesvertically);

        swCorner.y = this.transform.position.y; //The rain node's elevation is the same as the PrecipitationEffect's
        for (int i = 0; i < requiredRainNodesHorizontally; i ++)
        {
            for (int j = 0; j < requiredRainNodesvertically; j++)
            {
                Vector3 nodeLocation = swCorner + new Vector3(i * rainNodeWidth, 0.0f, j * rainNodeHeight) + new Vector3 (rainNodeWidth / 2.0f, 0.0f, rainNodeHeight/2.0f);
                //The second newVector is to make the corner of the first node match the corner of the plane. Else the first node would be spawned with its centre at the plane's corner.
                
                GameObject instance = GameObject.Instantiate(rainParticleSystemPrefab, nodeLocation, this.transform.rotation);
                instance.name = "RainNode" + i.ToString() + j.ToString();
                instance.transform.SetParent (rainArray);
            }
        }

        swCorner.y = this.transform.position.y + rainNodeHeight / 2.0f; //Clouds need to be slightly higher than the rain nodes (so rain appears to be falling from within clouds)
        for (int i = 0; i < requiredCloudNodesHorizontally; i ++)
        {
            for (int j = 0; j < requiredCloudNodesvertically; j++)
            {
                Vector3 nodeLocation = swCorner + new Vector3(i * cloudNodeWidth, 0.0f, j * cloudNodeHeight) + new Vector3 (cloudNodeWidth / 2.0f, 0.0f, cloudNodeHeight/2.0f);
                GameObject instance = GameObject.Instantiate(cloudParticleSystemPrefab, nodeLocation, this.transform.rotation);
                instance.name = "CloudNode" + i.ToString() + j.ToString();
                instance.transform.SetParent (cloudArray);
            }
        }
    }

    void PopulateNodesHolders()
    {
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
