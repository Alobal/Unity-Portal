using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.Device;

public class PortalManager : MonoBehaviour
{
    public GameObject[] portal_objects;
    PortalDoor[] portals; 
    public PlayerController player;

    private void Start()
    {
        portal_objects = GameObject.FindGameObjectsWithTag("Portal");
        int portal_count= portal_objects.Length;
        portals=new PortalDoor[portal_count];
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        for (int i = 0; i < portal_count; i+=2)
        {
            PortalDoor source_door = portal_objects[i].GetComponent<PortalDoor>();
            PortalDoor target_door = portal_objects[(i + 1) % portal_count].GetComponent<PortalDoor>();
            portals[i] = source_door;
            portals[i + 1] = target_door;
            source_door.Init(target_door,player);
            target_door.Init(source_door, player);
        }
    }

    private void OnPreCull()
    {
        int length = portal_objects.Length;
        for (int i = 0; i < length; i++)
            portals[i].Render();
    }


    ///get the relative velocity that copy_rb to source_center, and then copy it based on target_center.
    static public Vector3 CopyRelativeVelocity(Rigidbody copy_rb,GameObject source_center,GameObject target_center)
    {
        Vector3 velocity_relative = source_center.transform.InverseTransformVector(copy_rb.velocity);
        Vector3 velocaity_new= target_center.transform.TransformVector(-velocity_relative);
        return velocaity_new;
    }

    ///get the relative position that copy_object to source_center, and then copy it based on target_center.
    static public Vector3 CopyRelativePos(GameObject copy_object,GameObject source_center,GameObject target_center)
    {
        Vector3 position_offset = source_center.transform.InverseTransformPoint(copy_object.transform.position);
        Vector3 position_vice = target_center.transform.TransformPoint(position_offset);
        return position_vice;
    }

    static public Vector3 CopyRelativePos(Vector3 copy_pos, GameObject source_center, GameObject target_center)
    {
        Vector3 position_offset = source_center.transform.InverseTransformPoint(copy_pos);
        Vector3 position = target_center.transform.TransformPoint(position_offset);
        return position;
    }

    ///get the relative rotation that copy_object to source_center, and then copy it based on target_center.
    static public Quaternion CopyRelativeRotation(GameObject copy_object, GameObject source_center, GameObject target_center)
    {
        //align the player relative rotation to portal door.
        Quaternion rotation_vice = copy_object.transform.rotation * Quaternion.Inverse(source_center.transform.rotation);
        rotation_vice = target_center.transform.rotation * rotation_vice;

        return rotation_vice;
    }
    static public Quaternion CopyRelativeRotation(Quaternion copy_rotation, GameObject source_center, GameObject target_center)
    {
        //align the player relative rotation to portal door.
        Quaternion rotation = copy_rotation * Quaternion.Inverse(source_center.transform.rotation);
        rotation = target_center.transform.rotation * rotation;

        return rotation;
    }

    //Copy the position and rotation of source, to target.
    static public void CopyPosRot(GameObject target, GameObject source)
    {
        target.transform.position = source.transform.position;
        target.transform.rotation = source.transform.rotation;
    }
    static public void CopyPosRot(GameObject target, Vector3 pos, Quaternion rot)
    {
        target.transform.position = pos;
        target.transform.rotation = rot;
    }
    //Copy the position , rotation,localscale of source, to target.
    static public void CopyPosRotLScale(GameObject target, GameObject source, Vector3 local_scale)
    {
        target.transform.position = source.transform.position;
        target.transform.rotation = source.transform.rotation;
        target.transform.localScale = local_scale;
    }

    static public void CopyPosRotLScale(GameObject target, Vector3 pos,Quaternion rot,Vector3 local_scale)
    {
        target.transform.position = pos;
        target.transform.rotation = rot;
        target.transform.localScale = local_scale;
    }

}
