using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yx
{
    public class ConvexHull3D
    {
        #region 基础定义
        public class Vertex
        {
            private bool _dirty = false;
            private double _x, _y, _z;
            public double x { get { return _x; } set { _x = value; _dirty = true; } }
            public double y { get { return _y; } set { _y = value; _dirty = true; } }
            public double z { get { return _z; } set { _z = value; _dirty = true; } }

            private double _len;
            public double Length() {
                if(_dirty)
                    _len = Math.Sqrt(x * x + y * y + z * z);
                return _len;
            }

            public static bool AreEqual(Vertex A,Vertex B) { return A.x == B.x && A.y == B.y && A.z == B.z; }

            public static Vertex operator -(Vertex A, Vertex B) { return new Vertex() { x = A.x - B.x, y = A.y - B.y, z = A.z - B.z }; }
            public static Vertex operator *(Vertex A, Vertex B) { return new Vertex() { x = A.y * B.z - A.z * B.y, y = A.z * B.x - A.x * B.z, z = A.x * B.y - A.y * B.x }; }
            public static double operator &(Vertex A, Vertex B) { return A.x * B.x + A.y * B.y + A.z * B.z; }

            public override string ToString()
            {
                return string.Format("({0},{1},{2})", x.ToString(), y.ToString(), z.ToString());
            }

            public static implicit operator Vector3(Vertex node)
            {
                return new Vector3((float)node.x, (float)node.y, (float)node.z);
            }

            public static implicit operator Vertex(Vector3 v3)
            {
                return new Vertex() { x = v3.x, y = v3.y, z = v3.z };
            }
        }

        public class Face
        {
            private Vertex[] _shareVs;
            private int[] _idx;

            public Face(int idx0,int idx1,int idx2,Vertex[] vs)
            {
                _idx = new int[3] { idx0, idx1, idx2 };
                _shareVs = vs;
            }

            public int GetIdx(int i)
            {
                return _idx[i];
            }

            public Vertex GetVert(int i)
            {
                return _shareVs[_idx[i]];
            }

            private Vertex _normal = null;
            public Vertex Normal()
            {
                if(_normal == null)
                    _normal = (GetVert(1) - GetVert(0)) * (GetVert(2) - GetVert(0));

                return _normal;
            }

            private double _area = -1;
            public double Area() {
                if(_area <0)
                    _area = Normal().Length() / 2.0;
                return _area;
            }
            public void ClearCache()
            {
                _normal = null;
                _area = -1;
            }
            public bool Contains(Vertex p)
            {
                if ((Normal()&p)!=0)
                    return false;

                var v0 = GetVert(2) - GetVert(0);
                var v1 = GetVert(1) - GetVert(0);
                var v2 = p - GetVert(0);
                var d00 = v0 & v0;
                var d01 = v0 & v1;
                var d02 = v0 & v2;
                var d11 = v1 & v1;
                var d12 = v1 & v2;
                var inverDeno = 1 / (d00 * d11 - d01 * d01);
                var u = (d11 * d02 - d01 * d12) * inverDeno;
                if (u<0 || u>1)
                    return false;

                var v = (d00 * d12 - d01 * d02) * inverDeno;
                if (v < 0 || v > 1)
                    return false;

                return u + v <= 1;
            }

            public override string ToString()
            {
                return string.Format("Face:{0},{1},{2}", GetVert(0).ToString(), GetVert(1).ToString(), GetVert(2).ToString());
            }
        }
        #endregion
        static System.Random _rand = new System.Random();
        
        public double Eps = 1e-5;
        public int VertexCount { get { return _vertices.Length; } }
        public Face[] ConvexFaces { get { return _convexFaces; } }
        public int ConvexFacesCount { get { return _faceCnt; } }

        int _faceCnt;
        bool[,] _sawTable;
        Vertex[] _vertices;
        Face[] _convexFaces;
        // 包围盒
        Vertex _boundsMin = new Vertex() { x = double.MaxValue,y = double.MaxValue,z = double.MaxValue };
        Vertex _boundsMax = new Vertex() { x = double.MinValue,y = double.MinValue,z = double.MinValue };

        public void SetData(Vertex[] data,bool shake = true)
        {
            _faceCnt = 0;
            _vertices = data;
            _sawTable = new bool[VertexCount, VertexCount];
            int facesMax = VertexCount * 10;
            _convexFaces = new Face[facesMax];
            if (shake)
            {
                for (int i = 0; i < VertexCount; i++)
                {
                    Shake(_vertices[i]);
                }
            }
        }

        static int _randRange = 10000000;
        static double Rand()
        {
            return _rand.Next(0, _randRange) / (double)_randRange;
        }
        double Reps()
        {
            return (Rand() - 0.5) * Eps;
        }
        void Shake(Vertex v)
        {
            v.x += Reps();
            v.y += Reps();
            v.z += Reps();
        }
        double See(Face face,Vertex vert)
        {
            var v = (vert - face.GetVert(0)) & face.Normal();
            return v;
        }

        //对于一个已知凸包，新增一个点P
        //将P视作一个点光源，向凸包做射线
        //可以知道，光线的可见面和不可见面一定是由若干条棱隔开的
        //将光的可见面删去，并新增由其分割棱与P构成的平面
        //重复此过程即可，复杂度O(n2)
        public void Generate()
        {
            for (int i = 0; i < 4; i++)
                Shake(_vertices[i]);

            _convexFaces[_faceCnt] = new Face(0, 1, 2, _vertices); _faceCnt++;
            _convexFaces[_faceCnt] = new Face(2, 1, 0, _vertices); _faceCnt++;

            for (int i = 3; i < VertexCount; i++)
            {
                int retain = 0;// 需要保留的面的计数
                double saw;
                var vert = _vertices[i];
                for (int j = 0; j < _faceCnt; j++)
                {
                    var face = _convexFaces[j];
                    saw = See(face, vert);
                    if (saw == 0 && face.Contains(vert))
                        goto END_PASS;

                    if (saw<=0)// 如果该面对vert点来说不可见，可保留
                    {
                        _convexFaces[retain] = face;// 覆盖写
                        retain++;
                    }
                    for (int k = 0; k < 3; k++)// 记录该面三条边可见性
                    {
                        var x = face.GetIdx(k);
                        var y = face.GetIdx((k + 1) % 3);
                        _sawTable[x, y] = saw>0;
                    }
                }

                if (retain == 0 || retain == _faceCnt)
                    goto END_PASS;

                var cnt = retain;
                for (int j = 0; j < cnt; j++)// 所有不可见面（可保留的面）
                {
                    var face = _convexFaces[j];
                    for (int k = 0; k < 3; k++)
                    {
                        int x = face.GetIdx(k);
                        int y = face.GetIdx((k + 1) % 3);
                        if (!_sawTable[x, y] && _sawTable[y, x])// 用分界边与vert点构建新三角形
                        {
                            var nf = new Face(y, x, i, _vertices);
                            _convexFaces[retain] = nf;
                            retain++;
                        }
                    }
                }
                _faceCnt = retain;
            END_PASS:;
            }

            UpdateBounds();
        }

        public bool IsContain(Vertex v)
        {
            if (v.x < _boundsMin.x || v.x > _boundsMax.x || 
                v.y < _boundsMin.y || v.y > _boundsMax.y ||
                v.z < _boundsMin.z || v.z > _boundsMax.z)
            {
                return false;
            }
            double saw;
            for (int i = 0; i < _faceCnt; i++)
            {
                var face = _convexFaces[i];
                saw = See(face, v);
                if (saw > 0)
                    return false;
            }

            return true;
        }

        private void UpdateBounds()
        {
            for (int i = 0; i < ConvexFacesCount; i++)
            {
                var face = ConvexFaces[i];
                for (int j = 0; j < 3; j++)
                {
                    var v = face.GetVert(j);
                    _boundsMin.x = Math.Min(_boundsMin.x, v.x);
                    _boundsMin.y = Math.Min(_boundsMin.y, v.y);
                    _boundsMin.z = Math.Min(_boundsMin.z, v.z);

                    _boundsMax.x = Math.Max(_boundsMax.x, v.x);
                    _boundsMax.y = Math.Max(_boundsMax.y, v.y);
                    _boundsMax.z = Math.Max(_boundsMax.z, v.z);
                }
            }
        }
    }
}
