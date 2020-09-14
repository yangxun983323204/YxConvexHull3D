using System;
using System.Collections.Generic;
using UnityEngine;
using Yx;
// 核心算法测试
public class Test : MonoBehaviour
{
    public bool DrawGizmo = true;

    public ConvexHull3D ConvexHell = new ConvexHull3D();

    void Start()
    {
        var t0 = System.DateTime.Now.Ticks;
        List<ConvexHull3D.Vertex> vertices;
        //
        var mf = gameObject.GetComponent<MeshFilter>();
        if (mf == null)
        {
            vertices = new List<ConvexHull3D.Vertex>(1000);
            for (int i = 0; i < 1000; i++)
            {
                var v = new ConvexHull3D.Vertex() { x = UnityEngine.Random.Range(-100, 100), y = UnityEngine.Random.Range(-100, 100), z = UnityEngine.Random.Range(-100, 100) };
                vertices.Add(v);
            }

        }
        else
        {
            var mesh = mf.mesh;
            var cnt = mesh.vertexCount;
            vertices = new List<ConvexHull3D.Vertex>(cnt);
            var vs = mesh.vertices;
            var scale = transform.lossyScale;
            for (int i = 0; i < cnt; i++)
            {
                var mv = vs[i];
                var v = new ConvexHull3D.Vertex() { x = mv.x * scale.x, y = mv.y * scale.y, z = mv.z * scale.z };
                vertices.Add(v);
            }
        }
        
        ConvexHell.SetData(vertices.ToArray(), true);
        Debug.LogFormat("开始,顶点数:{0}", ConvexHell.VertexCount);
        ConvexHell.Generate();
        var delta = (System.DateTime.Now.Ticks - t0);
        //
        Debug.LogFormat("用时:{0}ms",(delta/10000f).ToString());
        Debug.LogFormat("完成,面数:{0}", ConvexHell.ConvexFacesCount);

        var gizmo = gameObject.AddComponent<ConvexHullGizmo>();
        gizmo.Convex = ConvexHell;
    }
}