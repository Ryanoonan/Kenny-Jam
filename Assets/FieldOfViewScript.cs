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
    private Color meshColor = new Color(1f, 0f, 0f, 0.5f); // Editable RGBA

    [Header("Control")]
    [SerializeField]
    public bool isActive = true;

    private Mesh viewMesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Material fovMaterial;
    private MeshRenderer meshRenderer;

    void Awake()
    {
        // Create mesh
        viewMesh = new Mesh { name = "FOV Mesh" };
        GetComponent<MeshFilter>().mesh = viewMesh;

        // Create or assign transparent material
        fovMaterial = SetupTransparentMaterial();
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = fovMaterial;
    }

    void LateUpdate()
    {
        if (isActive)
        {
            meshRenderer.enabled = true;
            DrawFieldOfView();
        }
        else
        {
            meshRenderer.enabled = false;
        }
    }

    private void DrawFieldOfView()
    {
        float angleStep = viewAngle / rayCount;
        vertices = new Vector3[rayCount + 2];
        triangles = new int[rayCount * 3];
        vertices[0] = Vector3.up * viewPointHeight; // Set origin to correct height in local space

        int triIndex = 0;
        Vector3 origin = transform.position + Vector3.up * viewPointHeight;

        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = -viewAngle / 2f + angleStep * i;
            Vector3 localDir = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward;
            Vector3 worldDir = transform.TransformDirection(localDir);

            Vector3 worldPoint = origin + worldDir * viewRadius;
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

        // Update color in case it changed
        fovMaterial.color = meshColor;
    }

    private Material SetupTransparentMaterial()
    {
        // Detect URP by checking for Universal Render Pipeline's Lit shader
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        Material mat;

        if (urpLit != null)
        {
            // URP Lit Transparent setup
            mat = new Material(urpLit);
            mat.SetFloat("_Surface", 1f); // Transparent
            mat.SetFloat("_Blend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        else
        {
            // Built-in Standard shader Transparent setup
            Shader std = Shader.Find("Standard");
            mat = new Material(std);
            mat.SetFloat("_Mode", 3f); // Transparent mode 
            mat.SetOverrideTag("RenderType", "Transparent");          // :contentReference[oaicite:7]{index=7}
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_ALPHABLEND_ON");                      // 
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent; // :contentReference[oaicite:9]{index=9}
        }

        return mat;
    }
}

