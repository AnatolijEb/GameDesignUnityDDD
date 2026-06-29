using UnityEngine;

[DisallowMultipleComponent]
public class WorldBendController : MonoBehaviour
{
    public static bool IsActive { get; private set; }
    public static Vector3 Origin { get; private set; }
    public static float StartDistance { get; private set; }
    public static float VerticalBend { get; private set; }
    public static float HorizontalBend { get; private set; }
    public static bool RecalculateNormals { get; private set; }

    [Header("Origin")]
    [SerializeField] private Transform origin;

    [Header("Targets")]
    [Tooltip("Usually ActiveChunksParent. If empty, this GameObject's children are used.")]
    [SerializeField] private Transform[] targetRoots;
    [SerializeField, Min(0.1f)] private float refreshInterval = 0.5f;
    [SerializeField] private bool includeInactive = true;

    [Header("Bend")]
    [SerializeField, Min(0f)] private float startDistance = 25f;
    [SerializeField] private float verticalBend = 0.0018f;
    [SerializeField] private float horizontalBend;
    [SerializeField] private bool recalculateNormals;

    private float refreshTimer;

    private void OnEnable()
    {
        IsActive = true;
        ApplySettings();
        RegisterMeshes();
    }

    private void OnDisable()
    {
        IsActive = false;
    }

    private void Update()
    {
        ApplySettings();

        refreshTimer -= Time.deltaTime;
        if (refreshTimer <= 0f)
        {
            RegisterMeshes();
            refreshTimer = refreshInterval;
        }
    }

    private void ApplySettings()
    {
        Origin = origin != null ? origin.position : transform.position;
        StartDistance = startDistance;
        VerticalBend = verticalBend;
        HorizontalBend = horizontalBend;
        RecalculateNormals = recalculateNormals;
    }

    private void RegisterMeshes()
    {
        if (targetRoots != null && targetRoots.Length > 0)
        {
            foreach (Transform targetRoot in targetRoots)
            {
                RegisterMeshesInRoot(targetRoot);
            }

            return;
        }

        RegisterMeshesInRoot(transform);
    }

    private void RegisterMeshesInRoot(Transform root)
    {
        if (root == null)
        {
            return;
        }

        MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(includeInactive);
        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.TryGetComponent(out WorldBendProxyVisual _))
            {
                continue;
            }

            if (meshFilter.sharedMesh == null)
            {
                continue;
            }

            if (meshFilter.TryGetComponent(out WorldBendMesh _) || meshFilter.TryGetComponent(out WorldBendRendererProxy _))
            {
                continue;
            }

            if (meshFilter.sharedMesh.isReadable)
            {
                meshFilter.gameObject.AddComponent<WorldBendMesh>();
            }
            else if (meshFilter.TryGetComponent(out MeshRenderer _))
            {
                meshFilter.gameObject.AddComponent<WorldBendRendererProxy>();
            }
        }
    }
}
