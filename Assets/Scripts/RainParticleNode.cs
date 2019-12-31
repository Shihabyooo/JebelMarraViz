using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainParticleNode : MonoBehaviour
{
    bool isActive = true;
    float ratePerSecond = 10.0f;
    ParticleSystem pSystem;
    // Start is called before the first frame update
    void Start()
    {
        pSystem = this.gameObject.GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive)
            return;

        pSystem.Emit(Mathf.RoundToInt(ratePerSecond * Time.deltaTime));
    }

    public void UpdateRate(float newRate)
    {
        if (ratePerSecond <= 1.0f)
            isActive = false;
        else
            isActive = true;

        ratePerSecond = newRate;

        //print("Updating Node: " + this.gameObject.name + " with rate: " + newRate);
    }
}
