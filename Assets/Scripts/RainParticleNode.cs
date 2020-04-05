using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//TODO modify this class to emit similarily to CloudParticlesNode. Even better: make a parent class and have both inherit from it.
public class RainParticleNode : MonoBehaviour
{
    bool isActive = true;
    float ratePerSecond = 1.0f;
    ParticleSystem pSystem;

    void Start()
    {
        pSystem = this.gameObject.GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (!isActive)
            return;

        pSystem.Emit(Mathf.RoundToInt(ratePerSecond * Time.deltaTime));
    }

    public void UpdateRate(float newRate)
    {
        ratePerSecond = newRate;

        if (ratePerSecond <= 1.0f)
            isActive = false;
        else
            isActive = true;
    }
}
