using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yx
{
    public class MeshConvexHull3D : TransformConvexHull3D
    {
        public override void Generate()
        {
            int sumCnt = 0;
            var meshFilters = _root.GetComponentsInChildren<MeshFilter>();
            var subVs = new List<List<Vertex>>(meshFilters.Length);
            foreach (var mf in meshFilters)
            {
                var child = mf.transform;
                var matrix = GetLocalMatrix(child);
                var mesh = mf.mesh;
                var cnt = mesh.vertexCount;
                var vertices = new List<Vertex>(cnt);
                var vs = mesh.vertices;
                for (int i = 0; i < cnt; i++)
                {
                    var mv = vs[i];
                    vertices.Add(matrix.MultiplyPoint(mv));
                }
                subVs.Add(vertices);
                sumCnt += vertices.Count;
            }
            var sumVs = new List<Vertex>(sumCnt);
            foreach (var sub in subVs)
            {
                sumVs.AddRange(sub);
            }

            SetData(sumVs.ToArray(), true);
            BaseGenerate();
        }
    }
}
