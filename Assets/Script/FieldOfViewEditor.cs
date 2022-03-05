using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{
    private void OnSceneGUI()
    {
        FieldOfView fov=target as FieldOfView;
        Handles.color=Color.white;
        var pos = fov.transform.position;
        
        Handles.DrawWireArc(pos,Vector3.forward,Vector3.up, 360,fov.viewRadius);
        
        Vector3 vaiwAngleA = fov.DirFromAngle(fov.viewAngle / 2);
        Vector3 vaiwAngleB = fov.DirFromAngle(-fov.viewAngle / 2);
        Handles.DrawLine(pos,pos+fov.viewRadius*vaiwAngleA);
        Handles.DrawLine(pos,pos+fov.viewRadius*vaiwAngleB);

        Handles.color=Color.cyan;
        // foreach (var visiableTarget in fov.visiableTargets)
        // {
        //     Handles.DrawLine(pos,visiableTarget.position);
        // }
    }
}
