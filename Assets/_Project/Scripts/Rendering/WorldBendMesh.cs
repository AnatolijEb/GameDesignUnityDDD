using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WorldBendMesh : MonoBehaviour
{
    private MeshFilter meshFilter;
    private Mesh sourceMesh;
    private Mesh bentMesh;
    private Vector3[] sourceVertices;
    private Vector3[] bentVertices;

    private void OnEnable()
    {
        Initialize();

        if (WorldBendController.IsActive)
        {
            ApplyBend();
        }
    }

    private void LateUpdate()
    {
        if (!WorldBendController.IsActive)
        {
            return;
        }

        if (bentMesh == null)
        {
            Initialize();
        }

        ApplyBend();
    }

    private void OnDisable()
    {
        RestoreSourceMesh();
    }

    private void OnDestroy()
    {
        RestoreSourceMesh();

        if (bentMesh != null)
        {
            Destroy(bentMesh);
        }
    }

    private void Initialize()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter.sharedMesh == null)
        {
            return;
        }

        if (meshFilter.sharedMesh == bentMesh && bentMesh != null)
        {
            return;
        }

        if (meshFilter.sharedMesh == sourceMesh && bentMesh != null)
        {
            meshFilter.sharedMesh = bentMesh;
            return;
        }

        sourceMesh = meshFilter.sharedMesh;
        sourceVertices = sourceMesh.vertices;
        bentVertices = new Vector3[sourceVertices.Length];

        bentMesh = Instantiate(sourceMesh);
        bentMesh.name = sourceMesh.name + " (World Bend Runtime)";
        bentMesh.MarkDynamic();
        meshFilter.sharedMesh = bentMesh;
    }

    private void ApplyBend()
    {
        Vector3 origin = WorldBendController.Origin;
        float startDistance = WorldBendController.StartDistance;
        float verticalBend = WorldBendController.VerticalBend;
        float horizontalBend = WorldBendController.HorizontalBend;

        for (int i = 0; i < sourceVertices.Length; i++)
        {
            Vector3 worldPosition = transform.TransformPoint(sourceVertices[i]);
            Vector3 offset = worldPosition - origin;
            float distance = offset.z - startDistance;

            if (distance > 0f)
            {
                float bend = distance * distance;
                worldPosition.y -= bend * verticalBend;
                worldPosition.x += offset.x * bend * horizontalBend;
            }

            bentVertices[i] = transform.InverseTransformPoint(worldPosition);
        }

        bentMesh.vertices = bentVertices;

        if (WorldBendController.RecalculateNormals)
        {
            bentMesh.RecalculateNormals();
        }

        bentMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
    }

    private void RestoreSourceMesh()
    {
        if (meshFilter != null && sourceMesh != null)
        {
            meshFilter.sharedMesh = sourceMesh;
        }
    }
}
