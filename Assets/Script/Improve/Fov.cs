using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Fov : MonoBehaviour
{
    [Range(0, 360)] public float viewAngle;
    public float viewRadius;
    public Vector2 forward;
    public Camera mainCam;

    public LayerMask obstacleMask;
    //public LayerMask targetMask;

    //Haven't used
    // [HideInInspector] public List<Transform> visiableTargets = new List<Transform>();

    public float rayCountRatio;

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
        Vector3 mousePos = mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
            -mainCam.transform.position.z));
        var pos = transform.position;
        forward = new Vector2(mousePos.x - pos.x,mousePos.y - pos.y);
        float a = Mathf.Atan2(forward.y,forward.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, 90 - a);
        DrawFieldOfView();
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
        List<ViewCastInfo> viewCastInfos = new List<ViewCastInfo>();
        List<Collider2D> obstacles = new List<Collider2D>();
        var trans = transform;
        var pos = (Vector2) trans.position;
        
        //正常投射光线并记录碰撞点
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = trans.eulerAngles.z - viewAngle / 2 + i * stepAngSize;
            ViewCastInfo newViewCat = ViewCast(angle, pos);
            viewCastInfos.Add(newViewCat);
        }
        
        //记录碰撞体端点的光线
        obstacles.AddRange(Physics2D.OverlapCircleAll(pos,viewRadius,obstacleMask));
        foreach (var other in obstacles)
        {
            float dis =Mathf.Max(other.bounds.size.x, other.bounds.size.y)+ 1000;
            var center = (Vector2)other.transform.position;
            Vector2 dir = Vector3.Cross((Vector3)center - transform.position, Vector3.back);
            
            var upPoint = other.ClosestPoint(center + dir * dis);
            upPoint=CheckPoint(upPoint,Vector3.back, other, dis);
            Debug.Log(upPoint);
            float angle = Vector2.Angle(upPoint-(Vector2)transform.position, new Vector2(0,1));
            if (Vector3.Dot(Vector2.right, upPoint-(Vector2)transform.position) < 0)
            {
                angle = 360 - angle;
            }
            
            ViewCastInfo hit = ViewCast(angle-0.1f, pos);
            ViewCastInfo hit2 = ViewCast(angle+0.1f, pos);
            
            var downPoint = other.ClosestPoint(center - dir * dis);
            downPoint=CheckPoint(downPoint,Vector3.forward, other, dis);
            Debug.Log(downPoint);
            float angle2 = Vector2.Angle(downPoint-(Vector2)transform.position, new Vector2(0,1));
            if (Vector3.Dot(Vector2.right, downPoint-(Vector2)transform.position) < 0)
            {
                angle2 = 360 - angle2;
            }
            
            ViewCastInfo hit3 = ViewCast(angle2-0.05f, pos);
            ViewCastInfo hit4 = ViewCast(angle2+0.05f, pos);
            
            viewCastInfos.Add(hit);
            viewCastInfos.Add(hit2);
            viewCastInfos.Add(hit3);
            viewCastInfos.Add(hit4);
        }
        
        viewCastInfos.Sort((a, b) =>
        {
            if (a.angle == b.angle) return 0;
            return a.angle > b.angle ? 1 : -1;
        });

        foreach (var t in viewCastInfos)
        {
            viewPoints.Add(t.point);
        }


        int verticeCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[verticeCount];
        int[] triangleIndex = new int[(verticeCount - 2) * 3];
        vertices[0] = Vector3.zero;
        for (int i = 0; i < verticeCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(new Vector3(viewPoints[i].x, viewPoints[i].y, 0));
            //i=viewPoints.Count-1即最后一个元素时无法形成三角形所以跳过
            if (i < verticeCount - 2)
            {
                triangleIndex[i * 3] = 0;
                triangleIndex[i * 3 + 1] = i + 1;
                triangleIndex[i * 3 + 2] = i + 2;
            }
        }

        for (int i = 0; i < viewPoints.Count - 1; i++)
        {
            Debug.DrawLine(pos, viewPoints[i], Color.red);
            Debug.DrawLine(viewPoints[i], viewPoints[i + 1], Color.red);
        }

        Debug.DrawLine(pos, viewPoints[viewPoints.Count - 1], Color.red);

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangleIndex;
        viewMesh.RecalculateNormals();
    }
    
    private Vector3 CheckPoint(Vector2 point,Vector3 z, Collider2D other, float checkDis)
    {
        Vector2 dir = Vector3.Cross((Vector3)point - transform.position, z);
        var newPoint = other.ClosestPoint(point + dir.normalized * checkDis);
        if (Vector2.Distance(newPoint, point) < 0.05f)
        {
            return point;
        }
        else
        {
            return CheckPoint(newPoint,z,other, checkDis);
        }
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

        public EdgeInfo(Vector2 a, Vector2 b)
        {
            pointA = a;
            pointB = b;
        }
    }
}