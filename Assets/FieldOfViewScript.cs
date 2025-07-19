// File: FieldOfView.cs
using UnityEngine;
using System.Collections.Generic;

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
    [SerializeField]
    private Color meshColor = new Color(1f, 0f, 0f, 0.5f);

    [Header("Control")]
    [SerializeField]
    public bool isActive = true;

    private Mesh viewMesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Material fovMaterial;
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock propBlock;
    private static readonly RaycastHit[] raycastHits = new RaycastHit[1];

    void Awake()
    {
        // 1) Mesh setup
        viewMesh = new Mesh { name = "FOV Mesh" };
        viewMesh.MarkDynamic();                                   // hint for frequent updates 
        GetComponent<MeshFilter>().mesh = viewMesh;

        // 2) Pre‑allocate arrays once
        vertices = new Vector3[rayCount + 2];
        triangles = new int[rayCount * 3];

        // 3) Material and property block
        fovMaterial = FOVMaterialFactory.CreateTransparent();
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = fovMaterial;
        propBlock = new MaterialPropertyBlock();
    }

    void LateUpdate()
    {
        meshRenderer.enabled = isActive;
        if (isActive)
        {
            DrawFieldOfView();
            ScanForObjectsInFOV();
        }
    }

    private void DrawFieldOfView()
    {
        float angleStep = viewAngle / rayCount;
        int triIndex = 0;
        Vector3 origin = transform.position + Vector3.up * viewPointHeight;

        vertices[0] = Vector3.up * viewPointHeight;

        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = -viewAngle / 2f + angleStep * i;
            Vector3 worldDir = transform.TransformDirection(
                                      Quaternion.Euler(0, currentAngle, 0) * Vector3.forward
                                  );

            Vector3 worldPoint = origin + worldDir * viewRadius;
            // 4) Non‑allocating raycast 
            if (Physics.RaycastNonAlloc(origin, worldDir, raycastHits, viewRadius, obstructionMask) > 0)
                worldPoint = raycastHits[0].point;

            vertices[i + 1] = transform.InverseTransformPoint(worldPoint);

            if (i < rayCount)
            {
                triangles[triIndex++] = 0;
                triangles[triIndex++] = i + 1;
                triangles[triIndex++] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();

        // 5) Per‑instance color without material instancing 
        propBlock.SetColor("_BaseColor", meshColor);
        propBlock.SetColor("_Color", meshColor);
        meshRenderer.SetPropertyBlock(propBlock);
    }

    /// <summary>
    /// Checks if a given world position is inside the field of view
    /// </summary>
    /// <param name="targetPosition">The world position to check</param>
    /// <returns>True if the position is within the FOV cone and not obstructed</returns>
    public bool IsPositionInFieldOfView(Vector3 targetPosition)
    {
        if (!isActive) return false;

        Vector3 origin = transform.position + Vector3.up * viewPointHeight;
        Vector3 directionToTarget = (targetPosition - origin).normalized;
        float distanceToTarget = Vector3.Distance(origin, targetPosition);

        // Check if target is within view radius
        if (distanceToTarget > viewRadius) return false;

        // Calculate angle between forward direction and direction to target
        Vector3 forwardDirection = transform.forward;
        float angleToTarget = Vector3.Angle(forwardDirection, directionToTarget);

        // Check if target is within view angle
        if (angleToTarget > viewAngle / 2f) return false;

        // Check if there's an obstruction between origin and target
        if (Physics.RaycastNonAlloc(origin, directionToTarget, raycastHits, distanceToTarget, obstructionMask) > 0)
        {
            // If we hit something before reaching the target, it's obstructed
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if a transform is inside the field of view using its position
    /// </summary>
    /// <param name="target">The transform to check</param>
    /// <returns>True if the transform is within the FOV cone and not obstructed</returns>
    public bool IsTransformInFieldOfView(Transform target)
    {
        return IsPositionInFieldOfView(target.position);
    }

    /// <summary>
    /// Scans for InteractableItems and ControllableUnits within the field of view
    /// </summary>
    private void ScanForObjectsInFOV()
    {
        // Get all InteractableItems in the scene
        InteractableItem[] items = FindObjectsByType<InteractableItem>(FindObjectsSortMode.None);
        foreach (var item in items)
        {
            if (IsPositionInFieldOfView(item.transform.position))
            {
                // Get the ControllableUnit component of this FOV's owner
                ControllableUnit ownerUnit = GetComponent<ControllableUnit>();
                if (ownerUnit != null)
                {
                    ownerUnit.FoundInteractibleObject(item);
                }
            }
        }

        // Get all ControllableUnits in the scene
        ControllableUnit[] units = FindObjectsByType<ControllableUnit>(FindObjectsSortMode.None);
        foreach (var unit in units)
        {
            // Skip self
            if (unit.gameObject == gameObject) continue;

            if (IsPositionInFieldOfView(unit.transform.position))
            {
                // Find PlayerManagerScript and call unitSpotted
                PlayerManagerScript playerManager = FindFirstObjectByType<PlayerManagerScript>();
                if (playerManager != null)
                {
                    playerManager.unitSpotted(unit);
                }
            }
        }
    }
}
