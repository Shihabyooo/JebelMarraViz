using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Slider monthSlider;
    public Text monthText;
    public Text rainfallText;
    public Text elevationText;

    public Text meanElevationText;
    public Text stdElevationText;
    public Text meanRainfallText;
    public Text stdRainfallText;

    public Slider resoSlider;
    public static UIManager uiMan;

    void Awake()
    {
        if (uiMan == null)
        {
            uiMan = this;
        }
        else
            Destroy(this.gameObject);
    }

    public void UpdateMonthTextFromSlider()
    {
        monthText.text = GetMonthName((int)monthSlider.value);
    }

    public void UpdateElevation(float elevation)
    {
        elevationText.text = Mathf.RoundToInt(elevation).ToString();
    }

    public void UpdateRainfall(float rainfall)
    {
        rainfallText.text = Mathf.RoundToInt(rainfall).ToString();
    }

    string GetMonthName(int order)
    {
        string name;

        switch (order)
        {
            case 1:
                name = "January";
                break;
            case 2:
                name = "February";
                break;
            case 3:
                name = "March";
                break;
            case 4:
                name = "April";
                break;
            case 5:
                name = "May";
                break;
            case 6:
                name = "June";
                break;
            case 7:
                name = "July";
                break;
            case 8:
                name = "August";
                break;
            case 9:
                name = "September";
                break;
            case 10:
                name = "October";
                break;
            case 11:
                name = "November";
                break;
            case 12:
                name = "December";
                break;
            default:
                name = "ERROR!";
                break;
        }
        return name;
    }
    
    public void UpdateMapStatistics()
    {
        meanElevationText.text = Mathf.RoundToInt((GroundCreator.grCreator.groundPlane.planeSpecs.averageValue * 2004.0f)+1000.0f).ToString();
        stdElevationText.text = Mathf.RoundToInt((GroundCreator.grCreator.groundPlane.planeSpecs.valueStDev * 2004.0f) + 1000.0f).ToString();

        meanRainfallText.text = Mathf.RoundToInt(PrecipitationManager.precMan.precRaster.planeSpecs.averageValue * 1000.0f).ToString();
        stdRainfallText.text = Mathf.RoundToInt(PrecipitationManager.precMan.precRaster.planeSpecs.valueStDev * 1000.0f).ToString();
    }

}
