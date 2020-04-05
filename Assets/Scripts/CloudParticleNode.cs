using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//TODO consider fixing the emission rate (at 1, though set to zero if bellow threshold) and starting to modify the timeBetweenEmissions based on recieved preciptation rates.
public class CloudParticleNode : MonoBehaviour
{
    //public bool isActive = true;
    public float rate = 0.0f; //this is for testing only (viewing rate in inspector)
    public int cloudEmissionRate = 1;
    ParticleSystem pSystem;
    float timeBetweenEmissions = 3.0f;
    float lowRainThreshold = 10.0f;
    float stormThreshold = 55.0f;
    int maxEmissionRate = 5;

    void Start()
    {
        pSystem = this.gameObject.GetComponent<ParticleSystem>();
    }

    public void UpdateRate(float newRate)
    {
        float range = stormThreshold - lowRainThreshold;
        rate = newRate;
        //cloudEmissionRate = newRate;
        CalculateCloudCoverParameters(newRate);

        if (cloudEmissionRate <= 0)
            {
                StopAllCoroutines();
                cloudEmitter = null;
            }
        else if (cloudEmitter == null)
            cloudEmitter = StartCoroutine(EmitCloud());
    }

    void CalculateCloudCoverParameters(float newRate)
    {
        if (newRate < lowRainThreshold)
            cloudEmissionRate = 0;
        else
        {
            float tempRate = Mathf.Clamp(newRate, lowRainThreshold, stormThreshold);
            //simple linear interpolation
            cloudEmissionRate =  Mathf.RoundToInt(1.0f + ( (tempRate - lowRainThreshold) * ((float)maxEmissionRate - 1.0f) / (stormThreshold - lowRainThreshold)));
        }

        //TODO darken the colours of the clouds (and prepare to include lightening generators) here if newRate > stormThreshold
    }

    Coroutine cloudEmitter = null;
    IEnumerator EmitCloud()
    {
        while (true)
        {
            pSystem.Emit(cloudEmissionRate);
            yield return new WaitForSeconds(timeBetweenEmissions);
        }   
    }
}
