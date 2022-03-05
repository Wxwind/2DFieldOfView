using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMeshSortingLayer : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    [SerializeField] private string sortingLayerName;
    [SerializeField] private int sortingOrder;

    public void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sortingLayerName = sortingLayerName;
        meshRenderer.sortingOrder = sortingOrder;
    }
}