using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Device;

class TravellerData
{
    public PortalTraveller traveller;
    public PortalGhost ghost;
    public float last_dst;

    public TravellerData(PortalTraveller t, PortalGhost g, float d = 0)
    {
        traveller = t;
        ghost = g;
        last_dst = d;
    }

    public void Clear()
    {
        traveller.ResetClipMaterial();
        Object.Destroy(ghost.gameObject);
    }
}
public class PortalDoor : MonoBehaviour
{
    public GameObject portal_view;
    public GameObject ghost_prefab;
    public RenderTexture rt;
    public CamGhost cam_ghost;
    public PortalDoor target_portal;
    Dictionary<PortalTraveller, TravellerData> travel_data = new();
    int render_recursion_time = 10;
    public float thickness { get { return transform.localScale.z; } }
    Camera main_cam;
    PlayerController player;
    GameObject recursion_old_door;
    GameObject recursion_new_door;


    // Start is called before the first frame update
    void Awake()
    {
        main_cam = Camera.main;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        InitRenderTexture();
        InitDoorThickness();
        recursion_old_door = new GameObject("recursion_old_door");
        recursion_new_door = new GameObject("recursion_new_door");
        recursion_old_door.transform.parent = transform.parent;
        recursion_new_door.transform.parent = transform.parent;
    }

    private void Update()
    {
        CheckTraveller();
    }


    public void Render()
    {
        Camera cam = cam_ghost.cam;
        Vector3[] cam_recursion_pos = new Vector3[render_recursion_time];
        Quaternion[] cam_recursion_rot = new Quaternion[render_recursion_time];
        cam_recursion_pos[0] = cam_ghost.transform.position;
        cam_recursion_rot[0] = cam_ghost.transform.rotation;
        PortalManager.CopyPosRotLScale(recursion_old_door, target_portal.gameObject, transform.localScale);

        /* We want to get the relative transform of target portal to source portal, for making recursion doors behind target portal.
         * However, Notice target portal sit forward to the source door. 
         * if we use relative transform for new door, we will create it forward to target portal.
         * Thus, we need to make target portal transform turn 180 around the y-axis.
         * By this, when we create new door forward to the rotated target portal, actually we create it behind the true target portal, which is our purpose.
         not same forward as target_portal to source_portal*/
        recursion_old_door.transform.Rotate(new Vector3(0f, 180f, 0f));

        //compute relative pos offset of target portal related to source portal.
        Vector3 pos_relative = transform.InverseTransformPoint(recursion_old_door.transform.position);
        Quaternion rot_relative = recursion_old_door.transform.rotation * Quaternion.Inverse(transform.rotation);


        for (int i = 1; i < render_recursion_time; i++)
        {
            //generate new door with same pos offset and rotation offset
            recursion_new_door.transform.localScale = transform.localScale;
            recursion_new_door.transform.position = recursion_old_door.transform.TransformPoint(pos_relative);
            recursion_new_door.transform.rotation = recursion_old_door.transform.rotation * rot_relative;

            cam_recursion_pos[i] = PortalManager.CopyRelativePos(cam_recursion_pos[i - 1], recursion_old_door, recursion_new_door);
            cam_recursion_rot[i] = PortalManager.CopyRelativeRotation(cam_recursion_rot[i - 1], recursion_old_door, recursion_new_door);
            PortalManager.CopyPosRot(recursion_old_door, recursion_new_door);
        }

        //Calcuate the render pos for each recursion
        for (int i = render_recursion_time - 1; i >= 0; i--)
        {
            cam_ghost.transform.position = cam_recursion_pos[i];
            cam_ghost.transform.rotation = cam_recursion_rot[i];
            cam_ghost.AdjustCamNearPlane();
            cam.Render();
        }
    }


    // Use late update for check teleport instead of OnCollisionStay, which makes several repeated teleport once.
    void CheckTraveller()
    {
        AdjustPortalView();
        int lenth = travel_data.Count;
        for (int i = lenth - 1; i >= 0; i--)
        {
            PortalTraveller traveller = travel_data.Keys.ElementAt(i);
            float dst = Vector3.Dot((traveller.transform.position - transform.position), transform.forward);
            float last_dst = travel_data[traveller].last_dst;
            if (dst * last_dst < 0) //teleport 
            {
                Teleport(traveller, travel_data[traveller].ghost);
            }
            else
                travel_data[traveller].last_dst = dst;
        }
    }

    void Teleport(PortalTraveller traveller, PortalGhost ghost)
    {
        traveller.Teleport(ghost);
        ClearTraveller(traveller);
    }

    //Add traveller to traveller list
    public void PreTeleport(PortalTraveller traveller)
    {

        if (!travel_data.ContainsKey(traveller))
        {
            TravellerData data = new(traveller, GenerateGhost(traveller));
            travel_data[traveller] = data;
        }
        traveller.PreTeleport(this);
        travel_data[traveller].ghost.SetClipMaterial();
    }


    void AdjustPortalView()
    {
        bool to_back = IsDoorForwardSide(player.transform.position - transform.position);
        AdjustPortalView(to_back);
    }

    public void LeaveTeleport(PortalTraveller traveller)
    {
        ClearTraveller(traveller);
    }
    public void AdjustPortalView(bool to_back)
    {
        int dir = to_back ? 1 : -1;
        portal_view.transform.position = transform.position - 0.5f * thickness * transform.forward * dir;
    }

    void ClearTraveller(PortalTraveller traveller)
    {
        if (travel_data.ContainsKey(traveller))
        {
            travel_data[traveller].Clear();
            travel_data.Remove(traveller);
        }
    }

    public PortalGhost GetGhost(PortalTraveller traveller)
    {
        if (travel_data.ContainsKey(traveller))
            return travel_data[traveller].ghost;
        else
            return null;
    }
    /// <summary>
    /// Generate vice player at target_portal, according to the relative transform of main player to the source portal.
    /// </summary>
    /// <param name="source_portal"></param>
    /// <param name="target_portal"></param>
    public void GenerateCamGhost(PortalTraveller traveller)
    {
        cam_ghost = Instantiate(ghost_prefab, gameObject.transform).GetComponent<CamGhost>();
        cam_ghost.Init(this, target_portal, traveller);
    }

    public PortalGhost GenerateGhost(PortalTraveller traveller)
    {
        GameObject ghost_go = new GameObject($"{traveller.name}_ghost");
        PortalGhost ghost = ghost_go.AddComponent<PortalGhost>();
        ghost.Init(this, target_portal, traveller);
        return ghost;
    }


    public void Init(PortalDoor target_portal, PlayerController main_player)
    {
        if (this.target_portal == null)
            this.target_portal = target_portal;
        GenerateCamGhost(main_player);
    }
    /// <summary>
    /// Sets the thickness of the portal screen so as not to clip with camera near plane when player goes through
    /// Too large make camera stay in the render texture of target portal.
    /// Too small make camera through the source portal, and show the scene behind the portal.
    /// </summary>
    void InitDoorThickness()
    {
        float half_height = main_cam.nearClipPlane * Mathf.Tan(main_cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float half_width = half_height * main_cam.aspect;
        float dst_to_nearcorner = new Vector3(half_width, half_height, main_cam.nearClipPlane).magnitude;
        float thickness = dst_to_nearcorner * 1.1f;
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, thickness);
        return;
    }
    void InitRenderTexture()
    {
        int[] res = GetGameViewResolution();
        int width = res[0];
        int height = res[1];
        if (rt != null)
        {
            rt.Release();
            rt.width = width;
            rt.height = height;
        }
        else
            rt = new RenderTexture(width, height, 0);
        rt.Create();
        portal_view.GetComponent<Renderer>().material.mainTexture = rt;
    }

    /// whether door forward is same side with the test direction.
    bool IsDoorForwardSide(Vector3 test_dir)
    {
        return Vector3.Dot(transform.forward, test_dir) > 0 ? true : false;
    }

    int[] GetGameViewResolution()
    {
        string[] res_str = UnityStats.screenRes.Split('x');
        int[] res = new int[2] { int.Parse(res_str[0]), int.Parse(res_str[1]) };
        return res;
    }
}
