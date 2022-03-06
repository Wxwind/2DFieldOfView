using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class QFovTarget : MonoBehaviour
{
    public bool onVisible;
    public void View(bool visible)
    {
       onVisible=visible;
    }
    public void Start()
    {
        View(false);
    }
}
