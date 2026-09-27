using UnityEngine;
using UnityEngine.Rendering;

namespace DirectionalPowerShafts;

internal static class ShaftArrow
{
    private static Mesh _mesh;
    private static Material _material;

    public static GameObject Create(Transform parent)
    {
        var arrow = new GameObject("Construction direction arrow");
        arrow.layer = parent.gameObject.layer;
        arrow.transform.SetParent(parent, worldPositionStays: false);
        arrow.transform.localPosition = new Vector3(0.5f, 1.025f, 0.5f);
        arrow.transform.localRotation = Quaternion.identity;
        arrow.transform.localScale = Vector3.one;
        arrow.AddComponent<MeshFilter>().sharedMesh = Mesh();
        var renderer = arrow.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = Material(parent);
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return arrow;
    }

    private static Mesh Mesh()
    {
        if (_mesh != null)
        {
            return _mesh;
        }

        // In unrotated grid space the arrow points toward -Y (world -Z),
        // matching the terrain block's directional blocker.
        _mesh = new Mesh { name = "Directional power shaft arrow" };
        _mesh.vertices = new[]
        {
            new Vector3(-0.075f, 0f, 0.30f),
            new Vector3( 0.075f, 0f, 0.30f),
            new Vector3(-0.075f, 0f,-0.045f),
            new Vector3( 0.075f, 0f,-0.045f),
            new Vector3(-0.27f, 0f,-0.045f),
            new Vector3( 0.27f, 0f,-0.045f),
            new Vector3( 0f,    0f,-0.35f),
        };
        // Both windings keep the arrow visible from elevated camera angles.
        _mesh.triangles = new[]
        {
            0, 1, 2, 1, 3, 2,
            4, 5, 6,
            2, 1, 0, 2, 3, 1,
            6, 5, 4,
        };
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        return _mesh;
    }

    private static Material Material(Transform parent)
    {
        if (_material != null)
        {
            return _material;
        }
        var shader = Shader.Find("Universal Render Pipeline/Unlit")
            ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
        if (shader == null)
        {
            foreach (var existingRenderer in parent.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (existingRenderer.sharedMaterial != null)
                {
                    shader = existingRenderer.sharedMaterial.shader;
                    break;
                }
            }
        }
        if (shader == null)
        {
            Debug.LogError("Directional Power Shafts: no shader found for construction arrows.");
            return null;
        }
        _material = new Material(shader) { name = "Directional power shaft arrow" };
        var color = new Color(1f, 0.75f, 0.1f, 1f);
        if (_material.HasProperty("_BaseColor"))
        {
            _material.SetColor("_BaseColor", color);
        }
        if (_material.HasProperty("_Color"))
        {
            _material.SetColor("_Color", color);
        }
        return _material;
    }
}
