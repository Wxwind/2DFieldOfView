using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class InputController : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    public Camera mainCam;
    public int speed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        rb.velocity = speed * new Vector2(x, Input.GetAxisRaw("Vertical"));
        if (x > 0)
        {
            sr.flipX = false;
        }
        else if (x < 0)
        {
            sr.flipX = true;
        }
    }
}