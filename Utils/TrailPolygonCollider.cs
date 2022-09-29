using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CardsPlusPlugin.Utils
{
    [RequireComponent(typeof(TrailRenderer))]
    [RequireComponent(typeof(PolygonCollider2D))]
    public class TrailPolygonCollider : MonoBehaviour
    {
        public bool debug = true;

        private TrailRenderer trail;
        private PolygonCollider2D polygonCollider;

        private GameObject visuals;

        private void Awake()
        {
            trail = GetComponent<TrailRenderer>();
            polygonCollider = GetComponent<PolygonCollider2D>();

            if (debug) visuals = new GameObject("Trail Debug", typeof(MeshFilter), typeof(MeshRenderer));
        }

        private void Update()
        {
            Mesh mesh = new Mesh();
            trail.BakeMesh(mesh, Camera.main, true);
            ApplyMeshToCollider(mesh, polygonCollider);

            if (visuals) visuals.GetComponent<MeshFilter>().mesh = mesh;
        }

        private void ApplyMeshToCollider(Mesh mesh, PolygonCollider2D collider)
        {
            // Get triangles and vertices from mesh
            int[] triangles = mesh.triangles;
            Vector3[] vertices = mesh.vertices;

            if (vertices.Length == 0) return;

            // Get just the outer edges from the mesh's triangles (ignore or remove any shared edges)
            Dictionary<string, KeyValuePair<int, int>> edges = new Dictionary<string, KeyValuePair<int, int>>();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                for (int e = 0; e < 3; e++)
                {
                    int vert1 = triangles[i + e];
                    int vert2 = triangles[i + e + 1 > i + 2 ? i : i + e + 1];
                    string edge = Mathf.Min(vert1, vert2) + ":" + Mathf.Max(vert1, vert2);
                    if (edges.ContainsKey(edge))
                    {
                        edges.Remove(edge);
                    }
                    else
                    {
                        edges.Add(edge, new KeyValuePair<int, int>(vert1, vert2));
                    }
                }
            }

            // Create edge lookup (Key is first vertex, Value is second vertex, of each edge)
            Dictionary<int, int> lookup = new Dictionary<int, int>();
            foreach (KeyValuePair<int, int> edge in edges.Values)
            {
                if (lookup.ContainsKey(edge.Key) == false)
                {
                    lookup.Add(edge.Key, edge.Value);
                }
            }

            // Create empty polygon collider
            collider.pathCount = 0;

            // Loop through edge vertices in order
            int startVert = 0;
            int nextVert = startVert;
            int highestVert = startVert;
            List<Vector2> colliderPath = new List<Vector2>();
            while (true)
            {

                // Add vertex to collider path
                colliderPath.Add(trail.transform.InverseTransformPoint(vertices[nextVert]));

                // Get next vertex
                nextVert = lookup[nextVert];

                // Store highest vertex (to know what shape to move to next)
                if (nextVert > highestVert)
                {
                    highestVert = nextVert;
                }

                // Shape complete
                if (nextVert == startVert)
                {

                    // Add path to polygon collider
                    collider.pathCount++;
                    collider.SetPath(collider.pathCount - 1, colliderPath.ToArray());
                    colliderPath.Clear();

                    // Go to next shape if one exists
                    if (lookup.ContainsKey(highestVert + 1))
                    {

                        // Set starting and next vertices
                        startVert = highestVert + 1;
                        nextVert = startVert;

                        // Continue to next loop
                        continue;
                    }

                    // No more verts
                    break;
                }
            }
        }

        private void OnDestroy()
        {
            if (visuals) Destroy(visuals);
        }
    }
}
