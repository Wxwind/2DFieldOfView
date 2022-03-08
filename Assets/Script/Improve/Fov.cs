using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Fov : MonoBehaviour
{
    private const float viewAngle = 360;
    public float viewRadius;
    public Vector2 forward;
    public Camera mainCam;

    public LayerMask obstacleMask;
    public Material show_Material;
    public Material hide_Material;

    [HideInInspector] public List<Transform> visiableTargets = new List<Transform>();

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
        var pos = transform.position;
        DrawFieldOfView();
    }

    IEnumerator CheckVisiableTarget(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
        }
    }

    private float FixAngle(float angle)
    {
        while (angle < 0) angle += 360;
        return angle;
    }

    List<Vector2> viewPoints = new List<Vector2>();
    List<ViewCastInfo> viewCastInfos = new List<ViewCastInfo>();

    List<Collider2D> obstacles = new List<Collider2D>();

    //正常光线碰撞到的障碍物。用于shader变化显示
    List<Collider2D> showObstacles = new List<Collider2D>();

    private void DrawFieldOfView()
    {
        //隐藏障碍物(变灰色)
        foreach (var o in showObstacles)
        {
            o.GetComponent<SpriteRenderer>().material = hide_Material;
        }

        showObstacles.Clear();
        viewPoints.Clear();
        viewCastInfos.Clear();
        obstacles.Clear();

        int stepCount = Mathf.RoundToInt(viewAngle * rayCountRatio);
        float stepAngSize = viewAngle / stepCount;

        var trans = transform;
        var pos = (Vector2) trans.position;

        //正常投射光线并记录碰撞点
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = FixAngle(trans.eulerAngles.z - viewAngle / 2 + i * stepAngSize);
            ViewCastInfo newViewCat = ViewCast(angle, pos);
            viewCastInfos.Add(newViewCat);
        }

        //显示碰撞体(正常颜色)
        foreach (var o in showObstacles)
        {
            o.GetComponent<SpriteRenderer>().material = show_Material;
        }

        //记录碰撞体端点的光线
        obstacles.AddRange(Physics2D.OverlapCircleAll(pos, viewRadius, obstacleMask));
        foreach (var other in obstacles)
        {
            //dis只要远大于碰撞体的尺寸就行
            float dis =  1000;
            var center = (Vector2) other.transform.position;
            Vector2 dir = Vector3.Cross((Vector3) center - transform.position, Vector3.back);

            var upPoint = other.ClosestPoint(center + dir * dis);
            upPoint = CheckPoint(upPoint, Vector3.back, other, dis);
            //upPoint不一定在碰撞体边缘上所以只能进行不怎么精确的修正
            upPoint += (center - upPoint).normalized * 0.012f;
            float angle = Vector2.Angle(upPoint - pos, new Vector2(0, 1));
            if (Vector3.Dot(Vector2.right, upPoint - pos) < 0)
            {
                angle = 360 - angle;
            }

            var upPointHit = new ViewCastInfo{angle=angle,dst = (upPoint-pos).magnitude,point = upPoint};
            
            //这里的angle后面也是对角度进行修正
            ViewCastInfo hit = ViewCast(angle - 0.12f, pos);
            AddHitInfo(upPointHit, hit);

            var downPoint = other.ClosestPoint(center - dir * dis);
            downPoint = CheckPoint(downPoint, Vector3.forward, other, dis);
            downPoint += (center - downPoint).normalized * 0.012f;
            float angle2 = Vector2.Angle(downPoint - pos, new Vector2(0, 1));
            if (Vector3.Dot(Vector2.right, downPoint - pos) < 0)
            {
                angle2 = 360 - angle2;
            }
            
            var downPointHit = new ViewCastInfo{angle=angle2,dst = (downPoint-pos).magnitude,point = downPoint};
            ViewCastInfo hit4 = ViewCast(angle2 + 0.12f, pos);
            AddHitInfo(downPointHit, hit4);
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

        Vector3[] vertices;
        int[] triangleIndex;


        int verticeCount = viewPoints.Count + 1;
        vertices = new Vector3[verticeCount];
        triangleIndex = new int[(verticeCount - 1) * 3];
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
            else if (i == verticeCount - 2)
            {
                triangleIndex[i * 3] = 0;
                triangleIndex[i * 3 + 1] = i + 1;
                triangleIndex[i * 3 + 2] = 1;
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

    private Vector3 CheckPoint(Vector2 point, Vector3 z, Collider2D other, float checkDis)
    {
        Vector2 dir = Vector3.Cross((Vector3) point - transform.position, z);
        var newPoint = other.ClosestPoint(point + dir.normalized * checkDis);
        if (Vector2.Distance(newPoint, point) < 0.001f)
        {
            return point;
        }
        else
        {
            return CheckPoint(newPoint, z, other, checkDis);
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
            if (!showObstacles.Contains(hit.collider)) showObstacles.Add(hit.collider);
            return new ViewCastInfo( hit.point, hit.distance, globalAngle);
        }
        else return new ViewCastInfo( playerPosition + dir * viewRadius, viewRadius, globalAngle);
    }
    
    public void AddHitInfo(ViewCastInfo hit,ViewCastInfo offsetHit)
    {
        if (offsetHit.dst < hit.dst)
        {
            hit.point += (Vector2)DirFromAngle(hit.angle,true) * (offsetHit.dst-hit.dst);
            hit.dst = offsetHit.dst;
        }
        viewCastInfos.Add(hit);
        viewCastInfos.Add(offsetHit);
    }

    public struct ViewCastInfo
    {
        public Vector2 point;
        public float dst;
        public float angle;

        public ViewCastInfo(Vector2 _point, float _dst, float _angle)
        {
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }
    
}