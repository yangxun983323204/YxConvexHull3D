using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yx;
// 从网格数据生成凸包
public class TestMeshConvexHull : MonoBehaviour {

    public MeshConvexHull3D Convex;
	// Use this for initialization
	void Start () {
        var t0 = System.DateTime.Now.Ticks;
        Convex = new MeshConvexHull3D();
        Convex.SetTransform(transform);
        Convex.Generate();
        var delta = (System.DateTime.Now.Ticks - t0);
        Debug.LogFormat("[TestMeshConvexHull]用时:{0}ms", (delta / 10000f).ToString());
        Debug.LogFormat("[TestMeshConvexHull]完成,顶点数:{0},面数:{1}", Convex.VertexCount, Convex.ConvexFacesCount);
        var gizmo = gameObject.AddComponent<ConvexHullGizmo>();
        gizmo.Convex = Convex;
        gizmo.Tran = Convex.Root;

        var renderers = gameObject.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = false;
        }
    }
}
