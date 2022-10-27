using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicTraveller : PortalTraveller
{
    
    public override void Teleport(PortalGhost ghost)
    {
        base.Teleport(ghost);
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.velocity=PortalManager.CopyRelativeVelocity(rb, ghost.source_portal.gameObject, ghost.target_portal.gameObject);
    }
}
