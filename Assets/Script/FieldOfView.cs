using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FieldOfView : MonoBehaviour
{
    [Range(0, 360)] public float viewAngle;
    public float viewRadius;

    public Camera mainCam;
    public LayerMask obstacleMask;
    //public LayerMask targetMask;

    //Haven't used
    // [HideInInspector] public List<Transform> visiableTargets = new List<Transform>();

    public float rayCountRatio;
    //To find the right edge when two raycasts hit the different obstacles 
    public int edgeResolveIterations;
    public float edgeDstThreshold;

    public MeshFilter viewMeshFilter;
    Mesh viewMesh;

    private void Start()
    {
        viewMesh = new Mesh();
        viewMeshFilter.mesh = viewMesh;
        viewMeshFilter.name = "View Mesh";
        StartCoroutine(CheckVisiableTarget(0.2f));
    }

    private void Update()
    {
        DrawFieldOfView();
        Vector3 mousePos = mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,-mainCam.transform.position.z));
        var pos = transform.position;
        float a = Mathf.Atan2(mousePos.y-pos.y, mousePos.x-pos.x)*Mathf.Rad2Deg;
        transform.rotation=Quaternion.Euler(0,0,90-a);
    }

    IEnumerator CheckVisiableTarget(float delay)
    {
        while (true)
        {
            //FindVisibleTargets();
            yield return new WaitForSeconds(delay);
        }
    }

    // private void FindVisibleTargets()
    // {
    //     visiableTargets.Clear();
    //     var targets = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
    //     foreach (var t in targets)
    //     {
    //         Transform t_trans = t.transform;
    //         Vector3 dirToTarget = (t_trans.position - transform.position).normalized;
    //         if (Vector3.Angle(dirToTarget, transform.forward) < viewAngle / 2)
    //         {
    //             float dstToTarget = Vector3.Distance(transform.position, t_trans.position);
    //             if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
    //             {
    //                 visiableTargets.Add(t_trans);
    //             }
    //         }
    //     }
    // }

    private void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * rayCountRatio);
        float stepAngSize = viewAngle / stepCount;
        List<Vector2> viewPoints = new List<Vector2>();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        var trans = transform;
        var pos = (Vector2)trans.position;
        for (int i = 0; i <= stepCount; i++)
        {
            
            float angle = trans.eulerAngles.z - viewAngle / 2 + i * stepAngSize;
            
            ViewCastInfo newViewCat = ViewCast(angle, pos);

            if (i>0)
            {
                bool edgeIsFar = Mathf.Abs(oldViewCast.dst - newViewCat.dst) > edgeDstThreshold;
                if (newViewCat.isHit!=oldViewCast.isHit||(newViewCat.isHit&&oldViewCast.isHit&& edgeIsFar))
                {
                    var edge = FindEdge(oldViewCast, newViewCat);
                    if (edge.pointA!=Vector2.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB!=Vector2.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }
            
            viewPoints.Add(newViewCat.point);
            oldViewCast = newViewCat;
        }

        int verticeCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[verticeCount];
        int[] triangleIndex = new int[(verticeCount - 2) * 3];
        vertices[0] = Vector3.zero;
        for (int i = 0; i < verticeCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(new Vector3(viewPoints[i].x,viewPoints[i].y,0));
            //i=viewPoints.Count-1即最后一个元素时无法形成三角形所以跳过
            if (i < verticeCount- 2)
            {
                triangleIndex[i * 3] = 0;
                triangleIndex[i * 3 + 1] = i + 1;
                triangleIndex[i * 3 + 2] = i + 2;
            }
        }

        for (int i = 0; i < viewPoints.Count-1; i++)
        {
            Debug.DrawLine(pos,viewPoints[i], Color.red);
            Debug.DrawLine(viewPoints[i],viewPoints[i+1], Color.red);
        }
        Debug.DrawLine(pos,viewPoints[viewPoints.Count-1], Color.red);
        
        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangleIndex;
        viewMesh.RecalculateNormals();
    }

    public Vector3 DirFromAngle(float angle, bool isGlobal = false)
    {
        if (!isGlobal) angle += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad), 0);
    }

    private ViewCastInfo ViewCast(float globalAngle, Vector2 playerPosition)
    {
        Vector2 dir = DirFromAngle(globalAngle, true);
        // /* 3D game used follow code */
        // if (Physics.Raycast(playerPosition, dir, out var hit, viewRadius, obstacleMask))
        // {
        //     return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        // }
        // else return new ViewCastInfo(false, playerPosition + dir * viewRadius, viewRadius, globalAngle);
        var hit = Physics2D.Raycast(playerPosition, dir, viewRadius, obstacleMask);
        if (hit.collider != null)
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else return new ViewCastInfo(false, playerPosition + dir * viewRadius, viewRadius, globalAngle);
    }

    private EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector2 minPoint = Vector2.zero;
        Vector2 maxPoint = Vector2.zero;
        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast= ViewCast(angle,transform.position);
            bool edgeIsFar = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
            if (newViewCast.isHit==minViewCast.isHit&&!edgeIsFar)
            {
                minAngle = newViewCast.angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = newViewCast.angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    public struct ViewCastInfo
    {
        public bool isHit;
        public Vector2 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _isHit, Vector2 _point, float _dst, float _angle)
        {
            isHit = _isHit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }
    
    public struct EdgeInfo
    {
        public Vector2 pointA;
        public Vector2 pointB;

        public EdgeInfo(Vector2 a,Vector2 b)
        {
            pointA = a;
            pointB = b;
        }
    }
}