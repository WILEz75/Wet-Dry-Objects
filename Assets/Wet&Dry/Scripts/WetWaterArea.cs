using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WetWaterArea : MonoBehaviour {

    Collider triggerArea;
    public float Activedepth = 0.3f;
    void Start()
    {
        //Try to get a Collider fot this object
        triggerArea = GetComponent<Collider>();
        //If there is not a rigidBody, add it (necessary to detect trigger areas, if use WetUseOcclusionAreas)
        if (!triggerArea) GetComponentInParent<Collider>();
        if (!triggerArea) transform.root.GetComponent<Collider>();
        if (!triggerArea)
        {
            triggerArea = gameObject.AddComponent<BoxCollider>();
            GetComponent<BoxCollider>().isTrigger = true;
            GetComponent<BoxCollider>().size = new Vector3(GetComponent<BoxCollider>().size.x, 10, GetComponent<BoxCollider>().size.z);
            GetComponent<BoxCollider>().center = new Vector3(GetComponent<BoxCollider>().center.x,((-GetComponent<BoxCollider>().size.y/2) - Activedepth), GetComponent<BoxCollider>().center.z);
        }
        if (GetComponent<Collider>() && GetComponent<Collider>().isTrigger)
        {
            triggerArea = GetComponent<Collider>(); triggerArea.isTrigger = true;
        }
    }

}
