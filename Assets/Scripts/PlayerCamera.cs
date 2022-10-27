using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    GameObject[] portals;
    // Start is called before the first frame update
    void Start()
    {
        portals = GetComponentInParent<PortalManager>().portal_objects;
    }

    private void OnPreCull()
    {
        int length = portals.Length;
        for (int i = 0; i < length; i++)
            portals[i].GetComponent<PortalDoor>().Render();
    }
}
