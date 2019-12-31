using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGauges : MonoBehaviour
{

    public RasterManager precipitationLayer;
    public RasterManager topographyLayer;


    [System.Serializable]
    public struct WeatherParameters
    {
        public float rainfall;
        public float elevation;
        public float posX;
        public float posY;
    };

    [SerializeField]
    WeatherParameters weatherParam = new WeatherParameters();

    // Start is called before the first frame update
    void Start()
    {
        UpdateWeatherParametersAtPlayer();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateWeatherParametersAtPlayer();
    }

    public void UpdateWeatherParametersAtPlayer()
    {
        //sample raw value
        weatherParam.rainfall = precipitationLayer.SampleRasterForRegion(this.transform.position, 2.0f * precipitationLayer.planeSpecs.spacingBetweenPixels);
        weatherParam.elevation = topographyLayer.SampleRasterForRegion(this.transform.position, 2.0f * topographyLayer.planeSpecs.spacingBetweenPixels);

        //convert to actuall value (conversion depends on how the geotiffs were rendered (e.g. in QGIS), i.e. what the lowest and heighest values represent
        weatherParam.rainfall = weatherParam.rainfall * 1000.0f;
        weatherParam.elevation = (weatherParam.elevation * 2004.0f) + 1000.0f;

        if (UIManager.uiMan != null)
        {
            UIManager.uiMan.UpdateElevation(weatherParam.elevation);
            UIManager.uiMan.UpdateRainfall(weatherParam.rainfall);
        }
    }




}
