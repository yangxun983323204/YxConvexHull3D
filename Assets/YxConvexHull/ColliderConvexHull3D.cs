using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yx
{
    public class ColliderConvexHull3D : TransformConvexHull3D
    {
        public override void Generate()
        {
            int sumCnt = 0;
            var colliders = _root.GetComponentsInChildren<Collider>();
            var subVs = new List<List<Vertex>>(colliders.Length);
            foreach (var collider in colliders)
            {
                var child = collider.transform;
                var matrix = GetLocalMatrix(child);
                var vs = GetColliderVertices(collider);
                var cnt = vs.Count;
                var vertices = new List<Vertex>(cnt);
                
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

        private List<Vertex> GetColliderVertices(Collider collider)
        {
            if (collider is BoxCollider)
                return GetBoxColliderVertices(collider as BoxCollider);
            else if (collider is SphereCollider)
                return GetSphereColliderVertices(collider as SphereCollider);
            else if (collider is CapsuleCollider)
                return GetCapsuleColliderVertices(collider as CapsuleCollider);
            else if (collider is MeshCollider)
                return GetMeshColliderVertices(collider as MeshCollider);
            else
                throw new System.NotSupportedException("不支持类型:" + collider.GetType().ToString());
        }

        private List<Vertex> GetBoxColliderVertices(BoxCollider collider)
        {
            var list = new List<Vertex>(8);
            var cent = collider.center;
            var half = collider.size / 2f;
            int[] dir = new int[] { -1, 1 };
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        var h = half;
                        h.Scale(new Vector3(dir[i],dir[j],dir[k]));
                        list.Add(cent + h);
                    }
                }
            }
            return list;
        }
        // https://www.zhihu.com/question/26602579?sort=created
        private List<Vertex> GetSphereColliderVertices(SphereCollider collider)
        {
            int step = 10;
            var list = new List<Vertex>(step * step);
            var cent = collider.center;
            var r = collider.radius * 1.07;
            for (int i = 0; i <= step; i++)
            {
                for (int j = 0; j <= step; j++)
                {
                    double u = i / (double)step;
                    double v = j / (double)step;
                    double phi = 2 * Mathf.PI * u;
                    double cosTheta = 1 - 2 * v;
                    double sinTheta = Mathf.Sqrt((float)(1 - cosTheta * cosTheta));
                    var x = sinTheta * Mathf.Cos((float)phi);
                    var y = sinTheta * Mathf.Sin((float)phi);
                    var z = cosTheta;
                    list.Add(
                        new Vertex() {
                            x = x * r + cent.x,
                            y = y * r + cent.y,
                            z = z * r + cent.z });
                }
            }

            return list;
        }
        /// <summary>
        /// 先用box近似胶囊体
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private List<Vertex> GetCapsuleColliderVertices(CapsuleCollider collider)
        {
            var list = new List<Vertex>(8);
            var cent = collider.center;
            var half = new Vector3(collider.radius, collider.height / 2f, collider.radius);
            int[] dir = new int[] { -1, 1 };
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        var h = half;
                        h.Scale(new Vector3(dir[i], dir[j], dir[k]));
                        list.Add(cent + h);
                    }
                }
            }
            return list;
        }

        private List<Vertex> GetMeshColliderVertices(MeshCollider collider)
        {
            var mesh = collider.sharedMesh;
            var cnt = mesh.vertexCount;
            var list = new List<Vertex>(cnt);
            var vs = mesh.vertices;
            for (int i = 0; i < cnt; i++)
            {
                var mv = vs[i];
                list.Add(mv);
            }
            return list;
        }
    }
}
