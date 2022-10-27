using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PortalTraveller : MonoBehaviour
{
    Material mat;
    public bool clip_after_portal = true;
    public float last_portal_dst=0; //record the last dist to source portal
    // Start is called before the first frame update
    void Start()
    {
        OnStart();
    }
    
    protected virtual void OnStart()
    {
        mat = GetComponent<MeshRenderer>().material;
    }

    virtual public void Teleport(PortalGhost ghost)
    {
        ghost.SyncTransform();//must sync first, or ghost may be later than player and cause screen clip.
        transform.position = ghost.transform.position;
        transform.rotation = ghost.transform.rotation;
    }

    virtual public void PreTeleport(PortalDoor portal)
    {
        Vector3 enter_dir = transform.position - portal.transform.position;
        Vector3 clip_normal = Vector3.Dot(enter_dir, portal.transform.forward) > 0 ? portal.transform.forward : -portal.transform.forward;
        if (clip_after_portal)
        {
            SetClipMaterial(portal.transform.position, clip_normal);
        }
    }

    public void SetClipMaterial(Vector3 clip_pos,Vector3 clip_normal)
    {
        mat.SetVector("clip_pos", clip_pos);
        mat.SetVector("clip_normal", clip_normal);
    }

    public void ResetClipMaterial()
    {
        mat.SetVector("clip_pos", new Vector3(0, 0, 0));
        mat.SetVector("clip_normal", new Vector3(0, 0, 0));
    }

}
