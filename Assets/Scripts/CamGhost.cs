using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class CamGhost : PortalGhost
{
    public Camera cam;
    PlayerController player_controller = null;



    /// <summary>
    /// align camera's near plane to portal 
    /// </summary>
    public void AdjustCamNearPlane()
    {
        // Learning resource:
        // http://www.terathon.com/lengyel/Lengyel-Oblique.pdf

    }

    public override void Init(PortalDoor source, PortalDoor target,PortalTraveller target_traveller)
    {
        base.Init(source,target,target_traveller);
        cam = GetComponentInChildren<Camera>();
        player_controller = traveller.GetComponent<PlayerController>();
        SetRenderTexture();
        SyncPlayerCamera();
    }
    /// <summary>
    /// compute the relative transform  to the target_door, which is same relative transform like player to source_door. 
    /// While the ghost transform should be rotated after copy transform.
    /// </summary>
    /// <returns></returns>
    public override void SyncStatus()
    {
        base.SyncStatus();
        SyncPlayerCamera();
    }

    void SyncPlayerCamera()
    {
        Quaternion player_cam_rot = player_controller.cam_local_rot;
        cam.transform.localRotation = player_cam_rot;
    }

    void SetRenderTexture()
    {
        int layer_mask = ~(1 << target_portal.gameObject.layer);//culling target portal layer
        cam.cullingMask &= layer_mask;
        cam.targetTexture = source_portal.rt;
    }
}
