// File: FieldOfView.cs
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfView : MonoBehaviour
{
    [Header("FOV Settings")]
    [SerializeField, Range(0f, 360f)]
    private float viewAngle = 90f;

    [SerializeField, Min(0f)]
    private float viewRadius = 5f;

    [SerializeField, Range(2, 100)]
    private int rayCount = 50;

    [Header("Origin Offset")]
    [SerializeField, Min(0f)]
    private float viewPointHeight = 1f;

    [Header("Obstructions")]
    [SerializeField]
    private LayerMask obstructionMask = ~0;

    [Header("Appearance")]
    [Tooltip("Assign a transparent material here in the Inspector.")]
    [SerializeField]
    private Material fovMaterial;

    [Header("Control")]
    [SerializeField]
    public bool isActive = true;

    private Mesh viewMesh;
    private Vector3[] vertices;
    private int[] triangles;
    private bool[] rayHits;
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock propBlock;
    private static readonly RaycastHit[] raycastHits = new RaycastHit[1];

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        // If no material assigned in Inspector, warn
        if (fovMaterial == null)
            Debug.LogWarning("FieldOfView: fovMaterial is not set. Assign one in the Inspector.");
        else
            meshRenderer.material = fovMaterial;

        // Create and configure mesh
        viewMesh = new Mesh { name = "FOV Mesh" };
        viewMesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = viewMesh;

        // Pre-allocate arrays
        vertices = new Vector3[rayCount + 2];
        triangles = new int[rayCount * 3];
        rayHits = new bool[rayCount + 1];
        propBlock = new MaterialPropertyBlock();
    }

    void LateUpdate()
    {
        meshRenderer.enabled = isActive;
        if (!isActive) return;
        DrawFieldOfView();
    }

    private void DrawFieldOfView()
    {
        float angleStep = viewAngle / rayCount;
        Vector3 origin = transform.position + Vector3.up * viewPointHeight;

        vertices[0] = Vector3.up * viewPointHeight;

        // Gather sample points
        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = -viewAngle / 2f + angleStep * i;
            Vector3 dir = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward;
            Vector3 worldPoint = origin + transform.TransformDirection(dir) * viewRadius;

            bool hit = Physics.RaycastNonAlloc(origin, transform.TransformDirection(dir), raycastHits, viewRadius, obstructionMask) > 0;
            if (hit) worldPoint = raycastHits[0].point;

            vertices[i + 1] = transform.InverseTransformPoint(worldPoint);
            rayHits[i] = hit;
        }

        // Build triangles, skipping across hit/no-hit transitions
        int ti = 0;
        for (int i = 0; i < rayCount; i++)
        {
            if (rayHits[i] == rayHits[i + 1])
            {
                triangles[ti++] = 0;
                triangles[ti++] = i + 1;
                triangles[ti++] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();

        // Update material color (if your shader uses _Color or _BaseColor)
        propBlock.SetColor("_Color", fovMaterial.color);
        meshRenderer.SetPropertyBlock(propBlock);
    }
}

/*
File: FOVMaterialFactory.cs
This file is no longer needed—delete it from your project.
*/
