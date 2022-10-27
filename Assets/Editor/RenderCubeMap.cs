using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RenderCubeMap : ScriptableWizard
{
    public Transform render_from_position;
    public Cubemap cube_map;

    private void OnWizardUpdate()
    {
        helpString = "选择一个位置，用于生成cube map";
        isValid=(render_from_position != null) && (cube_map != null);
    }

    private void OnWizardCreate()
    {
        GameObject go = new GameObject("Cube map Camera");
        go.AddComponent<Camera>();
        go.transform.position = render_from_position.position;
        go.GetComponent<Camera>().RenderToCubemap(cube_map);

        DestroyImmediate(go);
    }

    [MenuItem("GameObject/Render into Cubemap")]
    static void RenderCubemap()
    {
        ScriptableWizard.DisplayWizard<RenderCubeMap>("Render CubeMap", "Render!");
    }
}
