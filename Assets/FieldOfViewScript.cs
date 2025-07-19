// File: FieldOfView.cs
using System.Collections.Generic;
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
    [SerializeField]
    private Material fovMaterial;

    [Header("Control")]
    [SerializeField]
    public bool isActive = true;

    private PlayerManagerScript playerManager;
    private Mesh viewMesh;
    private Vector3[] vertices;
    private int[] triangles;
    private MeshRenderer meshRenderer;

    void Awake()
    {
        playerManager = GameObject.Find("PlayerManager")
                              .GetComponent<PlayerManagerScript>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (fovMaterial == null)
            fovMaterial = SetupTransparentMaterial();
        meshRenderer.material = fovMaterial;

        viewMesh = new Mesh { name = "FOV Mesh" };
        viewMesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = viewMesh;

        vertices = new Vector3[rayCount + 2];
        triangles = new int[rayCount * 3];
    }

    void LateUpdate()
    {
        meshRenderer.enabled = isActive;
        if (!isActive) return;

        DrawFieldOfView();

        Vector3 origin = transform.position + Vector3.up * viewPointHeight;
        DetectInteractibles(origin);
        DetectUnits(origin);
    }

    private void DrawFieldOfView()
    {
        float angleStep = viewAngle / rayCount;
        Vector3 origin = transform.position + Vector3.up * viewPointHeight;

        vertices[0] = Vector3.up * viewPointHeight;
        int triIndex = 0;

        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = -viewAngle / 2f + angleStep * i;
            Vector3 dir = transform.TransformDirection(
                Quaternion.Euler(0, currentAngle, 0) * Vector3.forward
            );
            Vector3 end = origin + dir * viewRadius;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, viewRadius, obstructionMask))
                end = hit.point;

            vertices[i + 1] = transform.InverseTransformPoint(end);

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

        // keep inspector happy
        fovMaterial.color = fovMaterial.color;
    }

    private void DetectInteractibles(Vector3 origin)
    {
        Collider[] hits = Physics.OverlapSphere(origin, viewRadius);
        foreach (var col in hits)
        {
            // only objects with an InteractibleItem component
            if (col.TryGetComponent<InteractableItem>(out var interact))
            {
                Vector3 dir = (col.transform.position - origin).normalized;
                if (Vector3.Angle(transform.forward, dir) <= viewAngle / 2f)
                {
                    float dist = Vector3.Distance(origin, col.transform.position);
                    if (!Physics.Raycast(origin, dir, dist, obstructionMask))
                        GetComponent<ControllableUnit>().FoundInteractibleObject(col.gameObject.GetComponent<InteractableItem>());
                }
            }
        }
    }

    private void DetectUnits(Vector3 origin)
    {
        Collider[] hits = Physics.OverlapSphere(origin, viewRadius);
        foreach (var col in hits)
        {
            // only objects with a ControllableUnit component
            if (col.TryGetComponent<ControllableUnit>(out var unit))
            {
                Vector3 dir = (col.transform.position - origin).normalized;
                if (Vector3.Angle(transform.forward, dir) <= viewAngle / 2f)
                {
                    float dist = Vector3.Distance(origin, col.transform.position);
                    if (!Physics.Raycast(origin, dir, dist, obstructionMask))
                        playerManager.unitSpotted(col.gameObject.GetComponent<ControllableUnit>());
                }
            }
        }
    }

    private Material SetupTransparentMaterial()
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        Material mat;
        if (urpLit != null)
        {
            mat = new Material(urpLit)
            {
                renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent
            };
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        }
        else
        {
            Shader std = Shader.Find("Standard");
            mat = new Material(std)
            {
                renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent
            };
            mat.SetFloat("_Mode", 3f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_ALPHABLEND_ON");
        }
        mat.SetOverrideTag("RenderType", "Transparent");
        return mat;
    }
}
