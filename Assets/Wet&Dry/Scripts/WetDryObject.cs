using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class WetDryObject : MonoBehaviour
{

    [Header("Wet Object - Material Glossiness affect by water contact")]
    [Space(10)]

    //Use this option for best performance
    //Put only one LiquidumWetObject script only in the parent/root object/character and set UniqueScriptInParent true
    //Use DiscardsMaterialsByName strings to discard material that you do not want to be influenced by the script
    public bool UniqueScriptInParent = true;
    [Range(0, 4)]
    public float WetMultipler = 1.2f;
    [Range(0,1)]
    public float MaxWet = 1;
    //Use DiscardsMaterialsByID to discard material that you do not want to be influenced by the script
    //Use DiscardsMaterialsByName if you use "UniqueScriptInParent" 
    public int[] DiscardsMaterialsByID = new int[] { };

    //Use DiscardsMaterialsByName strings to discard material that you do not want to be influenced by the script (for example little dettails)
    //Less materials you will have in AllMat and better performance
    public string[] DiscardsMaterialsByName = new string[] { };

    public bool AffectByRain;

    public bool AffectByWater;
    public bool WetUseOcclusionAreas;

    public bool UseEmissionMaterial = true; //Useful to simulate soggy cloths, they darken a bit

    [Range(0, 2)]
    public float WetSpeed = 0.5f;
    [Range(0, 2)]
    public float DryingSpeed = 0.5f;
    [Header("Drip Options")]
    public ParticleSystem DripEffect;
    public Vector3 DripEffectPosition;
    [Range(1f, 20)]
    public float dripQuantity = 5f;
    [Range(0.1f,1)]
    public float dripForWet=0.3f;
    #region privarte vars
    float ActualWetQuant;

    float OrGlossiness;
    float OrMapGlass;
    float OrMetallic;
    Color OrEmission;
    [HideInInspector]
    public List<Material> AllMat;
    [HideInInspector]
    public bool dryIn;
    [HideInInspector]
    public bool wetIn;
    [HideInInspector]
    public bool UnderOcclusion;
    [HideInInspector]
    public bool UnderWater;
    [HideInInspector]
    public SphereCollider triggerArea;
    static public bool RainEmit = true;
    ParticleSystem _drip;
    ParticleSystem.EmissionModule _dripEmitter;
    Rigidbody thisBody;

    Color baseEmission = new Color(1, 1, 1, 0.01f);
    #endregion
    

    void Start()
    {
        Initialize();


    }

    void Initialize()
    {



        //Try to get a rigidbody fot this object
        thisBody = GetComponent<Rigidbody>();
        //If there is not a rigidBody, add it (necessary to detect trigger areas, if use WetUseOcclusionAreas)
        if (!thisBody) GetComponentInParent<Rigidbody>();
        if (!thisBody) transform.root.GetComponent<Rigidbody>();
        if (!thisBody) { thisBody = gameObject.AddComponent<Rigidbody>(); thisBody.isKinematic = true; }
        if (GetComponent<SphereCollider>() && GetComponent<SphereCollider>().isTrigger)
            triggerArea = GetComponent<SphereCollider>();


        AllMat = new List<Material>();


        #region Materials in Childrens
        if (UniqueScriptInParent)
        {

            for (int i = 0; i < GetComponentsInChildren<Renderer>().Length; i++)
            {
                for (int n = 0; n < GetComponentsInChildren<Renderer>()[i].GetComponent<Renderer>().materials.Length; n++)
                {
                    AllMat.Add(GetComponentsInChildren<Renderer>()[i].GetComponent<Renderer>().materials[n]);
                }
            }

            for (int i = 0; i < GetComponentsInChildren<SkinnedMeshRenderer>().Length; i++)
            {
                for (int n = 0; n < GetComponentsInChildren<SkinnedMeshRenderer>()[i].GetComponent<SkinnedMeshRenderer>().materials.Length; n++)
                {
                    AllMat.Add(GetComponentsInChildren<SkinnedMeshRenderer>()[i].GetComponent<SkinnedMeshRenderer>().materials[n]);
                }
            }

        }
        else
        {
            if (!GetComponent<Renderer>()&& !GetComponent<SkinnedMeshRenderer>())
                Debug.LogError("<color=#4455FF>Wet</color><color=#AAAAAA>&</color><color=#22FF11>Dry</color><color=#AAAAAA> message: No renderer in " + gameObject.name+ ".</color>"
                + "<color=#AAAAAA> Check</color><color=#22FF11> UniqueScriptInParent</color><color=#AAAAAA> on true to affect his childrens by </color><color=#4455FF>Wet</color><color=#AAAAAA>&</color><color=#22FF11>Dry</color><color=#AAAAAA> script.</color>");

        }

        #endregion





        #region Parent Transform
        if (GetComponent<Renderer>())
        {

            for (int i = 0; i < GetComponent<Renderer>().materials.Length; i++)
            {
                if (DiscardsMaterialsByID.Length > 0)
                {
                    if (!DiscardsMaterialsByID.Contains(i))
                        AllMat.Add(GetComponent<Renderer>().materials[i]);
                }
                else
                { AllMat.Add(GetComponent<Renderer>().materials[i]); }

            }



        }
        if (GetComponent<SkinnedMeshRenderer>())
        {

            for (int i = 0; i < GetComponent<SkinnedMeshRenderer>().materials.Length; i++)
            {
                if (DiscardsMaterialsByID.Length > 0)
                {
                    if (!DiscardsMaterialsByID.Contains(i))
                        AllMat.Add(GetComponent<SkinnedMeshRenderer>().materials[i]);
                }
                else
                { AllMat.Add(GetComponent<SkinnedMeshRenderer>().materials[i]); }


            }

        }



        //After Adding, remove all DiscardsMaterialsByName in AllMat
        for (int i = 0; i < AllMat.Count; i++)
            for (int a = 0; a < DiscardsMaterialsByName.Length; a++)
            {

                if (AllMat[i].name == DiscardsMaterialsByName[a] + " (Instance)")
                {

                    AllMat.Remove(AllMat[i]);


                }


            }

        #endregion



        if (!triggerArea)
        {
            triggerArea = gameObject.AddComponent<SphereCollider>() as SphereCollider;
            triggerArea.isTrigger = true;
            triggerArea.radius = 0.1f;
            triggerArea.center = Vector3.zero;
        }



        for (int i = 0; i < AllMat.Count; i++)
        {


            if (AllMat[i].HasProperty("_Glossiness"))
            {
                OrGlossiness = AllMat[i].GetFloat("_Glossiness");
                AllMat[i].EnableKeyword("_Glossiness_OFF");
            }
            if (AllMat[i].HasProperty("_GlossMapScale"))
            {
                OrMapGlass = AllMat[i].GetFloat("_GlossMapScale");
                AllMat[i].EnableKeyword("_GlossMapScale_OFF");
            }
            if (AllMat[i].HasProperty("_Metallic"))
            {
                OrMetallic = AllMat[i].GetFloat("_Metallic");
                AllMat[i].EnableKeyword("_Metallic_OFF");
            }
            if (UseEmissionMaterial && AllMat[i].HasProperty("_EmissionColor"))
            {
                OrEmission = AllMat[i].GetColor("_EmissionColor");
                AllMat[i].EnableKeyword("_EmissionColor_OFF");
            }


        }

        if (DripEffect)
        {
            _drip = ParticleSystem.Instantiate(DripEffect, transform.position + DripEffectPosition, transform.rotation, transform);
            _dripEmitter = _drip.emission;
            _dripEmitter.rateOverTime = dripQuantity;
            _dripEmitter.enabled = false;
       
        }

    }


    public void Wet()
    {
        if (_drip)
        {
            _dripEmitter.enabled = false;
      
        }

        wetIn = true;
        dryIn = false;

        ActualWetQuant = Mathf.Lerp(ActualWetQuant, MaxWet, (WetSpeed * Time.deltaTime / 6));

        float gloss = OrGlossiness + ActualWetQuant * WetMultipler;
        gloss = Mathf.Clamp01(gloss) * 0.9f;

        for (int i = 0; i < AllMat.Count; i++)
        {
            AllMat[i].SetFloat("_Glossiness", gloss);
            AllMat[i].SetFloat("_GlossMapScale", Mathf.Clamp((OrMapGlass + ActualWetQuant * WetMultipler), 0, 0.95f));
            AllMat[i].SetFloat("_Metallic", Mathf.Clamp(OrMetallic - (ActualWetQuant / 3), 0, 0.95f));
            AllMat[i].SetColor("_EmissionColor", (OrEmission + (baseEmission * ActualWetQuant * 0.05f)));
        }


        if (ActualWetQuant >= MaxWet * 0.99f)
        {
            wetIn = false;
            ActualWetQuant = MaxWet;
        }
    }

    void Dry()
    {

        wetIn = false;
        dryIn = true;

        ActualWetQuant = Mathf.Lerp(ActualWetQuant, 0, (DryingSpeed * Time.deltaTime) / 8);

        float gloss = OrGlossiness + ActualWetQuant * WetMultipler;
        gloss = Mathf.Clamp01(gloss) * 0.95f;

        for (int i = 0; i < AllMat.Count; i++)
        {
            AllMat[i].SetFloat("_Glossiness", gloss);
            AllMat[i].SetFloat("_GlossMapScale", Mathf.Clamp((OrMapGlass + ActualWetQuant * WetMultipler), 0, 0.95f));
            AllMat[i].SetFloat("_Metallic", Mathf.Clamp(OrMetallic - (ActualWetQuant / 3), 0, 0.95f));
            AllMat[i].SetColor("_EmissionColor", (OrEmission + (baseEmission * ActualWetQuant * 0.1f)));
        }


        if (ActualWetQuant <= 0.01)
        {
            dryIn = false;
            ActualWetQuant = 0;

        }


        if (_drip)
        {
            if (ActualWetQuant <= (1-dripForWet))
                _dripEmitter.enabled = false;
            else
                _dripEmitter.enabled = true;

        }
        

    }


    public void WetNow()
    {
        dryIn = false;
        wetIn = true;
    }
    public void DryNow()
    {
        dryIn = true;
        wetIn = false;
    }


    void OnTriggerExit(Collider other)
    {


        //Occlusion Area Exit
        if (WetUseOcclusionAreas)
            if (other.GetComponent<RainOcclusionArea>())
                UnderOcclusion = false;

        //Water Area Exit
        if (AffectByWater)
            if (other.GetComponent<WetWaterArea>())
                UnderWater = false;

    }

    void OnTriggerStay(Collider other)
    {

        //Occlusion Area Enter
        if (WetUseOcclusionAreas)
            if (other.GetComponent<RainOcclusionArea>())
                UnderOcclusion = true;

        //Water Area Enter
        if (AffectByWater)
            if (other.GetComponent<WetWaterArea>())
                UnderWater = true;
    }

    void Update()
    {

        if (!triggerArea) return;

        if (!AffectByWater) UnderWater = false;



        if (AffectByRain )
        {
            if(RainEmit && !UnderOcclusion)
            WetNow();
            else
            if (!UnderWater)
               DryNow();
        }


        if (AffectByWater)
        {
            if (UnderWater) //If UnderWater, always wet 
            {
                WetNow();
            }
            else
            {
                if (AffectByRain && RainEmit)
                {
                    if (!UnderOcclusion)
                        WetNow();

                    if (UnderOcclusion)
                        DryNow();
                }
                else
                {
                    DryNow();
                }

            }
        }

        if (dryIn)
            Dry();

        if (wetIn)
            Wet();

    }
}
