using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class WetDryObject : MonoBehaviour
{
    
    [Header("Wet Object - Material Glossiness affect by water contact")]
    public string InspectorSpace1;
    public float WetMultipler = 1;

    //Use DiscardsMaterialsByID to discard material that you do not want to be influenced by the script
    //Use DiscardsMaterialsByName if you use "UniqueScriptInParent" 
    public int[] DiscardsMaterialsByID = new int[] { };

    //Use DiscardsMaterialsByName strings to discard material that you do not want to be influenced by the script (for example little dettails)
    //Less materials you will have in AllMat and better performance
    public string[] DiscardsMaterialsByName = new string[] { };

    public bool AffectByRain;
 
    public bool AffectByWater;
    public bool WetUseOcclusionAreas;
  //  [HideInInspector]
    public SphereCollider triggerArea;
    public bool UseEmissionMaterial = true; //Useful to simulate soggy cloths, they darken a bit
    public float MaxWet=1;
    public float WetByRainSpeed = 0.5f;
    public float DryingSpeed=0.5f;

    #region privarte vars
    float ActualWetQuant;
    
    float OrGlossiness;
    float OrMapGlass;
    float OrMetallic;
    Color OrEmission;
    public List<Material> AllMat;
    [HideInInspector]
    public bool dryIn;
    [HideInInspector]
    public bool wetIn;
    public bool UnderOcclusion;
    public bool UnderWater;


    //Use this option for best performance
    //Put only one LiquidumWetObject script only in the parent/root object/character and set UniqueScriptInParent true
    //Use DiscardsMaterialsByName strings to discard material that you do not want to be influenced by the script
    public bool UniqueScriptInParent = true;

    Rigidbody thisBody;

    Color baseEmission = new Color(1, 1, 1, 0.01f);
    #endregion
    static public bool RainEmit;

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




        # region Materials in Childrens
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



    }


    public void Wet()
    {
  

        wetIn = true;
        dryIn = false;

        ActualWetQuant = Mathf.Lerp(ActualWetQuant, MaxWet, (WetByRainSpeed * Time.deltaTime / 6));

        float gloss = OrGlossiness + ActualWetQuant * WetMultipler;
        gloss = Mathf.Clamp01(gloss) * 0.9f;

        for (int i = 0; i < AllMat.Count; i++)
        {
            AllMat[i].SetFloat("_Glossiness", gloss);
            AllMat[i].SetFloat("_GlossMapScale", Mathf.Clamp((OrMapGlass + ActualWetQuant * WetMultipler), 0, 0.95f));
            AllMat[i].SetFloat("_Metallic", Mathf.Clamp(OrMetallic - (ActualWetQuant / 2), 0, 0.95f));
            AllMat[i].SetColor("_EmissionColor", (OrEmission + (baseEmission * ActualWetQuant * 0.1f)));
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
        gloss = Mathf.Clamp01(gloss) * 0.9f;

        for (int i = 0; i < AllMat.Count; i++)
        {
            AllMat[i].SetFloat("_Glossiness", gloss);
            AllMat[i].SetFloat("_GlossMapScale", Mathf.Clamp((OrMapGlass + ActualWetQuant * WetMultipler), 0, 0.95f));
            AllMat[i].SetFloat("_Metallic", Mathf.Clamp(OrMetallic - (ActualWetQuant / 2), 0, 0.95f));
            AllMat[i].SetColor("_EmissionColor", (OrEmission + (baseEmission * ActualWetQuant * 0.1f)));
        }


        if (ActualWetQuant <= 0.01)
        {
            dryIn = false;
            ActualWetQuant = 0;
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

        if (AffectByRain && !UnderOcclusion)
        {
            wetIn =RainEmit;
            dryIn = !RainEmit;

           

        }

        if (AffectByRain && UnderOcclusion)
        {

            if (!UnderWater)
                DryNow();
        }

        if (AffectByWater)
        {
            if (UnderWater) //If UnderWater, wet always
            {
                WetNow();
            }
            else
            {
                if (AffectByRain)
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
