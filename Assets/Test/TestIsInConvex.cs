using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yx;
// 凸包包围点判断
public class TestIsInConvex : MonoBehaviour {

    public bool IsIn=false;
    public TestColliderConvexHull TCC;
    public TestMeshConvexHull TMC;
    public Renderer Renderer;

    Transform _tran;
    List<TransformConvexHull3D> _list = new List<TransformConvexHull3D>(2);
	// Use this for initialization
	IEnumerator Start () {
        yield return 0;
        _tran = transform;
        _list.Add(TCC.Convex);
        _list.Add(TMC.Convex);
        Debug.Log("在场景窗口的顶视图中移动物体以观察与凸包的位置关系");
	}

    private void Update()
    {
        foreach (var c in _list)
        {
            IsIn = c.IsContain(_tran.position);
            if (IsIn)
            {
                Renderer.material.color = Color.red;
                return;
            }
            Renderer.material.color = Color.white;
        }
    }
}
