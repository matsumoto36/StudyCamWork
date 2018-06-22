using UnityEngine;
using Assets;

public class CylinderBehaviour : MonoBehaviour
{
    Mesh mesh;
    int layer;
    Material material;

    const int countItems = 5;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        layer = gameObject.layer;
        material = GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        for (var i = 0; i < countItems; ++i)
        {
            var cloned = Clone(material);
            var color = cloned.color;
            color.a = 0.5f * ((countItems - (float)i) / (float)countItems);
            cloned.color = color;
            cloned.SetAsFade();

            Graphics.DrawMesh(mesh, Matrix4x4.Translate(new Vector3((i + 1.0f) * 1.5f, .0f, .0f)), cloned, layer);
        }
    }

    Material Clone(Material material)
    {
        var cloned = new Material(material.shader);
        cloned.color = material.color;
        return cloned;
    }
}