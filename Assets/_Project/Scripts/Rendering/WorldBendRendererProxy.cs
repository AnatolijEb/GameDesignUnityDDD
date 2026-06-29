using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WorldBendRendererProxy : MonoBehaviour
{
    private MeshRenderer sourceRenderer;
    private GameObject visualProxy;
    private Transform visualProxyTransform;

    private void OnEnable()
    {
        Initialize();
    }

    private void LateUpdate()
    {
        if (!WorldBendController.IsActive)
        {
            return;
        }

        if (visualProxy == null)
        {
            Initialize();
        }

        ApplyBend();
    }

    private void OnDisable()
    {
        RestoreSourceRenderer();
    }

    private void OnDestroy()
    {
        RestoreSourceRenderer();

        if (visualProxy != null)
        {
            Destroy(visualProxy);
        }
    }

    private void Initialize()
    {
        MeshFilter sourceFilter = GetComponent<MeshFilter>();
        sourceRenderer = GetComponent<MeshRenderer>();

        if (sourceFilter.sharedMesh == null || sourceRenderer == null)
        {
            return;
        }

        if (visualProxy != null)
        {
            return;
        }

        visualProxy = new GameObject(name + " World Bend Visual");
        visualProxy.AddComponent<WorldBendProxyVisual>();
        visualProxyTransform = visualProxy.transform;
        visualProxyTransform.SetParent(null, false);

        MeshFilter proxyFilter = visualProxy.AddComponent<MeshFilter>();
        proxyFilter.sharedMesh = sourceFilter.sharedMesh;

        MeshRenderer proxyRenderer = visualProxy.AddComponent<MeshRenderer>();
        proxyRenderer.sharedMaterials = sourceRenderer.sharedMaterials;
        proxyRenderer.shadowCastingMode = sourceRenderer.shadowCastingMode;
        proxyRenderer.receiveShadows = sourceRenderer.receiveShadows;
        proxyRenderer.lightProbeUsage = sourceRenderer.lightProbeUsage;
        proxyRenderer.reflectionProbeUsage = sourceRenderer.reflectionProbeUsage;
        proxyRenderer.probeAnchor = sourceRenderer.probeAnchor;
        proxyRenderer.allowOcclusionWhenDynamic = sourceRenderer.allowOcclusionWhenDynamic;

        sourceRenderer.enabled = false;
        ApplyBend();
    }

    private void ApplyBend()
    {
        if (visualProxyTransform == null)
        {
            return;
        }

        Vector3 worldPosition = transform.position;
        Vector3 origin = WorldBendController.Origin;
        Vector3 offset = worldPosition - origin;
        float distance = offset.z - WorldBendController.StartDistance;

        if (distance > 0f)
        {
            float bend = distance * distance;
            worldPosition.y -= bend * WorldBendController.VerticalBend;
            worldPosition.x += offset.x * bend * WorldBendController.HorizontalBend;
        }

        visualProxyTransform.SetPositionAndRotation(worldPosition, transform.rotation);
        visualProxyTransform.localScale = transform.lossyScale;
    }

    private void RestoreSourceRenderer()
    {
        if (sourceRenderer != null)
        {
            sourceRenderer.enabled = true;
        }
    }
}
