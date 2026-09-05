using UnityEngine;


// Generuje płaski pierścień z promieniowym rozłożeniem UV pod teksturę-pasek.

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SaturnRing : MonoBehaviour
{
    [Tooltip("Promień wewnętrzny pierścienia.")]
    public float innerRadius = 1.2f;
    [Tooltip("Promień zewnętrzny pierścienia.")]
    public float outerRadius = 2.2f;
    [Tooltip("Liczba segmentów (gładkość okręgu).")]
    public int segments = 96;
    [Tooltip("Materiał pierścienia (przezroczysty, dwustronny).")]
    public Material ringMaterial;

    void Start()
    {
        GetComponent<MeshFilter>().mesh = GenerateRing(innerRadius, outerRadius, segments);
        if (ringMaterial != null)
            GetComponent<MeshRenderer>().material = ringMaterial;
    }

    private Mesh GenerateRing(float inner, float outer, int seg)
    {
        Mesh mesh = new Mesh { name = "SaturnRing" };

        Vector3[] vertices = new Vector3[(seg + 1) * 2];
        Vector2[] uv = new Vector2[(seg + 1) * 2];
        int[] triangles = new int[seg * 6];

        for (int i = 0; i <= seg; i++)
        {
            float angle = (float)i / seg * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            vertices[i * 2]     = new Vector3(cos * inner, 0f, sin * inner);  // krawędź wewnętrzna
            vertices[i * 2 + 1] = new Vector3(cos * outer, 0f, sin * outer);  // krawędź zewnętrzna

            uv[i * 2]     = new Vector2(0f, (float)i / seg);  // u = 0 wnętrze tekstury
            uv[i * 2 + 1] = new Vector2(1f, (float)i / seg);  // u = 1 zewnątrz tekstury
        }

        int t = 0;
        for (int i = 0; i < seg; i++)
        {
            int ia = i * 2, oa = i * 2 + 1, ib = (i + 1) * 2, ob = (i + 1) * 2 + 1;
            triangles[t++] = ia; triangles[t++] = ib; triangles[t++] = oa;
            triangles[t++] = oa; triangles[t++] = ib; triangles[t++] = ob;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}