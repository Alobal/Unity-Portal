using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrePortalArea : MonoBehaviour
{
    PortalDoor portal;

    private void Awake()
    {
        portal = GetComponentInParent<PortalDoor>();
    }
    private void OnTriggerEnter(Collider other)
    {
        PortalTraveller traveller = other.GetComponent<PortalTraveller>();
        if (traveller != null)
        {
            portal.PreTeleport(traveller);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PortalTraveller traveller = other.GetComponent<PortalTraveller>();
        if (traveller != null)
        {
            portal.LeaveTeleport(traveller);
        }
    }
}
