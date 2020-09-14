using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Yx
{
    public class ConvexHullGizmo : MonoBehaviour
    {
        public ConvexHull3D Convex;
        public Transform Tran;
        public int DrawRedIdx = -1;
        public bool DrawRedNoCull = false;
        public bool DrawFrame = true;

        private void OnDrawGizmos()
        {
            if (DrawFrame)
                DrawLine();
            else
                DrawMesh();

            DrawRed();
        }

        private void DrawLine()
        {
            var matrix = Tran == null ? Matrix4x4.identity : Tran.localToWorldMatrix;
            for (int j = 0; j < Convex.ConvexFacesCount; j++)
            {
                var face = Convex.ConvexFaces[j];
                var a = matrix.MultiplyPoint(face.GetVert(0));
                var b = matrix.MultiplyPoint(face.GetVert(1));
                var c = matrix.MultiplyPoint(face.GetVert(2));
                Gizmos.DrawLine(a, b);
                Gizmos.DrawLine(b, c);
                Gizmos.DrawLine(c, a);
            }
            
        }

        Mesh _mesh = null;
        private void DrawMesh()
        {
            var matrix = Tran == null ? Matrix4x4.identity : Tran.localToWorldMatrix;
            if (_mesh == null)
            {
                _mesh = new Mesh();
                var vertices = new Vector3[Convex.VertexCount];
                var triangles = new int[Convex.ConvexFacesCount * 3];
                for (int i = 0; i < Convex.ConvexFacesCount; i++)
                {
                    var face = Convex.ConvexFaces[i];
                    vertices[face.GetIdx(0)] = face.GetVert(0);
                    vertices[face.GetIdx(1)] = face.GetVert(1);
                    vertices[face.GetIdx(2)] = face.GetVert(2);
                    triangles[i*3] = face.GetIdx(0);
                    triangles[i*3+1] = face.GetIdx(1);
                    triangles[i*3+2] = face.GetIdx(2);
                }
                _mesh.vertices = vertices;
                _mesh.triangles = triangles;
                _mesh.RecalculateNormals();
            }
            Gizmos.matrix = matrix;
            Gizmos.DrawMesh(_mesh);
            Gizmos.matrix = Matrix4x4.identity;
        }

        Mesh _redMesh = null;
        Vector3[] _redVertices = new Vector3[3];
        int[] _frontIdx = new int[] { 0, 1, 2 };
        int[] _backIdx = new int[] { 0, 2, 1 };
        private void DrawRed()
        {
#if UNITY_EDITOR
            if (DrawRedIdx>=0 && DrawRedIdx < Convex.ConvexFacesCount)
            {
                var matrix = Tran == null ? Matrix4x4.identity : Tran.localToWorldMatrix;
                if (_redMesh == null)
                {
                    _redMesh = new Mesh();
                }

                var face = Convex.ConvexFaces[DrawRedIdx];

                _redVertices[0] = face.GetVert(0);
                _redVertices[1] = face.GetVert(1);
                _redVertices[2] = face.GetVert(2);
                _redMesh.vertices = _redVertices;

                bool sawFront = Vector3.Dot(SceneView.lastActiveSceneView.camera.transform.forward, matrix.MultiplyVector(face.Normal())) <= 0;

                Gizmos.matrix = matrix;
                Gizmos.color = Color.red;

                if (sawFront)
                {
                    _redMesh.triangles = _frontIdx;
                    _redMesh.RecalculateNormals();
                    Gizmos.DrawMesh(_redMesh);
                }
                
                if (DrawRedNoCull && !sawFront)
                {
                    _redMesh.triangles = _backIdx;
                    _redMesh.RecalculateNormals();
                    Gizmos.DrawMesh(_redMesh);
                }

                Gizmos.color = Color.white;
                Gizmos.matrix = Matrix4x4.identity;
            }
#endif
        }
    }
}
