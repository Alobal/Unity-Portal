using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PortalGhost : MonoBehaviour
{
    public PortalDoor source_portal = null;//source portal is binded with the ghost camera render texture, which is the enter portal
    public PortalDoor target_portal = null;//the target portal is the out portal 
    public Vector3 move_dir;
    protected PortalTraveller traveller = null;
    MeshFilter mf;
    MeshRenderer mr;
    Vector3 last_pos;

    private void Awake()
    {
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
    }
    private void Update()
    {
        SyncStatus();
    }
    virtual public void Init(PortalDoor source_portal, PortalDoor target_portal,PortalTraveller target_traveller)
    {
        traveller = target_traveller;
        this.source_portal = source_portal;
        this.target_portal = target_portal;
        if (mf == null)
        {
            mf= gameObject.AddComponent<MeshFilter>();
            mf.mesh= target_traveller.GetComponent<MeshFilter>().mesh;
        }
        if (mr == null)
        {
            mr = gameObject.AddComponent<MeshRenderer>();
            mr.material = target_traveller.GetComponent<MeshRenderer>().material;

        }

        SyncTransform();
    }

    public virtual void SyncStatus()
    {
        SyncTransform();
    }


    public void SyncTransform()
    {
        last_pos = transform.position;
        transform.position = PortalManager.CopyRelativePos(traveller.gameObject, source_portal.gameObject, target_portal.gameObject);
        transform.rotation = PortalManager.CopyRelativeRotation(traveller.gameObject, source_portal.gameObject, target_portal.gameObject);
        transform.RotateAround(target_portal.transform.position, target_portal.transform.up, 180);
        move_dir = (transform.position - last_pos).normalized;

    }
    public void SetClipMaterial()
    {
        Vector3 clip_pos = target_portal.transform.position;
        Vector3 out_dir = clip_pos- transform.position;
        Vector3 clip_normal = Vector3.Dot(out_dir, target_portal.transform.forward) > 0 ? target_portal.transform.forward : -target_portal.transform.forward;
        mr.material.SetVector("clip_pos", clip_pos);
        mr.material.SetVector("clip_normal", clip_normal);
    }

}
