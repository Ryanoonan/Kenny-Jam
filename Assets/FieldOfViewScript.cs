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
    [Tooltip("Assign a transparent material here in the Inspector. If left empty, a default transparent material will be created at runtime.")]
    [SerializeField]
    private Material fovMaterial;

    [Header("Control")]
    [SerializeField]
    public bool isActive = true;

    private Mesh viewMesh;
    private Vector3[] vertices;
    private int[] triangles;
    private MeshRenderer meshRenderer;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        // Assign or create transparent material
        if (fovMaterial == null)
        {
            fovMaterial = SetupTransparentMaterial();
        }
        meshRenderer.material = fovMaterial;

        // Create and configure mesh
        viewMesh = new Mesh { name = "FOV Mesh" };
        viewMesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = viewMesh;

        // Pre-allocate arrays
        vertices = new Vector3[rayCount + 2];
        triangles = new int[rayCount * 3];
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
        int triIndex = 0;

        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = -viewAngle / 2f + angleStep * i;
            Vector3 localDir = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward;
            Vector3 worldDir = transform.TransformDirection(localDir);
            Vector3 worldPoint = origin + worldDir * viewRadius;

            // Obstruction check
            if (Physics.Raycast(origin, worldDir, out RaycastHit hit, viewRadius, obstructionMask))
            {
                worldPoint = hit.point;
            }

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

        // Update material color if changed
        fovMaterial.color = fovMaterial.color;
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
