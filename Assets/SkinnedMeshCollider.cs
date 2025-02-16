using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Updates the mesh collider to match the skinned mesh renderer.
/// </summary>
[RequireComponent(typeof(MeshCollider))]
public class SkinnedMeshCollider : MonoBehaviour
{
    [SerializeField, Utils.ReadOnly] private SkinnedMeshRenderer m_skinnedMeshRenderer;
    [SerializeField, Utils.ReadOnly] private MeshCollider m_meshCollider;

    private void Awake()
    {
        m_skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        m_meshCollider = GetComponent<MeshCollider>();
    }

    private void Update()
    {
        UpdateMeshCollider();
    }

    private void UpdateMeshCollider()
    {
        if (m_skinnedMeshRenderer == null || m_meshCollider == null)
        {
            return;
        }

        Mesh mesh = new Mesh();
        m_skinnedMeshRenderer.BakeMesh(mesh);

        m_meshCollider.sharedMesh = mesh;
    }
}