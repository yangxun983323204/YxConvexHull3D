using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yx;
// 从collider上生成凸包
public class TestColliderConvexHull : MonoBehaviour {

    public ColliderConvexHull3D Convex;
	// Use this for initialization
	void Start () {
        var t0 = System.DateTime.Now.Ticks;
        Convex = new ColliderConvexHull3D();
        Convex.SetTransform(transform);
        Convex.Generate();
        var delta = (System.DateTime.Now.Ticks - t0);
        Debug.LogFormat("[TestColliderConvexHull]用时:{0}ms", (delta / 10000f).ToString());
        Debug.LogFormat("[TestColliderConvexHull]完成,顶点数:{0},面数:{1}", Convex.VertexCount, Convex.ConvexFacesCount);
        var gizmo = gameObject.AddComponent<ConvexHullGizmo>();
        gizmo.Convex = Convex;
        gizmo.Tran = Convex.Root;
    }
}
