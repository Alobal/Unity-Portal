using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class CamGhost : PortalGhost
{
    public Camera cam;
    PlayerController player_controller = null;
    Matrix4x4 original_proj;



    /// <summary>
    /// align  camera's near plane to the oblique portal plane
    /// </summary>
    public void AdjustCamNearPlane()
    {
        Transform portal_plane = target_portal.transform;
        int behind_portal = System.Math.Sign(Vector3.Dot(portal_plane.forward, portal_plane.position - cam.transform.position));
        Vector3 camspace_portal_pos = cam.worldToCameraMatrix.MultiplyPoint(portal_plane.position);
        Vector3 camspace_portal_normal = cam.worldToCameraMatrix.MultiplyVector(portal_plane.forward) * behind_portal;
        float camspace_portal_dst = -Vector3.Dot(camspace_portal_pos, camspace_portal_normal);
        if (Mathf.Abs(camspace_portal_dst) > 0.2)
        {
            Vector4 camspace_portal_plane = new Vector4(camspace_portal_normal.x, camspace_portal_normal.y, camspace_portal_normal.z, camspace_portal_dst);
            cam.projectionMatrix = cam.CalculateObliqueMatrix(camspace_portal_plane);
        }
        else
            cam.projectionMatrix = original_proj;
    }

    public override void Init(PortalDoor source, PortalDoor target, PortalTraveller target_traveller)
    {
        base.Init(source, target, target_traveller);
        cam = GetComponentInChildren<Camera>();
        original_proj = cam.projectionMatrix;
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
        //AdjustCamNearPlane();
    }

    void SetRenderTexture()
    {
        int layer_mask = ~(1 << target_portal.gameObject.layer);//culling target portal layer
        cam.cullingMask &= layer_mask;
        cam.targetTexture = source_portal.rt;
    }
}
