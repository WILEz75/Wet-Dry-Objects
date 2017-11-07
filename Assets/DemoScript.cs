using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class DemoScript : MonoBehaviour
{



    public ParticleSystem  RainEmitter;
    public GameObject Monk, SpeedBall, Ethan;
    public CameraFollowCharacter cam;
    public bool HideNonActiveAvatrs=true;
    public ParticleSystem.EmissionModule RainEmitterModule;

    void Start()
    {
        ChangeToSpeedBall();
        RainEmitterModule = RainEmitter.emission;
    }



    public void ChangeToMonk()
    {


        cam.target = Monk.transform;
        Monk.SetActive(true);

        if (!HideNonActiveAvatrs) return;    
        SpeedBall.SetActive(false);
        Ethan.SetActive(false);
 
    }

    public void ChangeToSpeedBall()
    {
        cam.target = SpeedBall.transform;
        SpeedBall.SetActive(true);

        if (!HideNonActiveAvatrs) return;
        Monk.SetActive(false);
        Ethan.SetActive(false);

    }

    public void ChangeToEthan()
    {
        cam.target = Ethan.transform;
        Ethan.SetActive(true);

        if (!HideNonActiveAvatrs) return;
        Monk.SetActive(false);
        SpeedBall.SetActive(false);

    }


    public void StopPlayRainEmitter()
    {
        RainEmitterModule.enabled = !RainEmitterModule.enabled;
    }

    private void Update()
    {
            WetDryObject.RainEmit = RainEmitterModule.enabled;

        if (RainEmitter.emissionRate > 0)
            WetDryObject.RainEmit = true;
        else
            WetDryObject.RainEmit = false;

    }
}
