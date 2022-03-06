using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Collider2D coll;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        var a=coll.ClosestPoint(new Vector2(1, 0));
        Debug.Log(a);
        Debug.DrawLine(a,new Vector3(10, 0,0),Color.red);
    }
}
