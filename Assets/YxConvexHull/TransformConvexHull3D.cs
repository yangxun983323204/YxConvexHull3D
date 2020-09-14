using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yx
{
    /// <summary>
    /// 带变换信息的凸包
    /// </summary>
    public abstract class TransformConvexHull3D : ConvexHull3D
    {
        public Transform Root { get { return _root; } }

        protected Transform _root;

        public void SetTransform(Transform root)
        {
            _root = root;
        }

        public bool IsContain(Vector3 wpos)
        {
            var lpos = _root.InverseTransformPoint(wpos);
            return base.IsContain(lpos);
        }
        /// <summary>
        /// 获取child在root局部坐标系中的变换矩阵
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public Matrix4x4 GetLocalMatrix(Transform child)
        {
            var path = GetChildPath(child);
            Matrix4x4 matrix = Matrix4x4.identity;
            foreach (var tran in path)
            {
                matrix = matrix * Matrix4x4.TRS(tran.localPosition, tran.localRotation, tran.localScale);
            }

            return matrix;
        }

        private List<Transform> GetChildPath(Transform child)
        {
            var path = new List<Transform>(5);
            var p = child;
            while (p!=null)
            {
                if (p == _root)
                {
                    path.Reverse();
                    return path;
                }
                path.Add(p);
                p = p.parent;
            }
            throw new System.NotSupportedException("不是root的子物体");
        }

        public new abstract void Generate();

        protected void BaseGenerate()
        {
            base.Generate();
        }
    }
}
