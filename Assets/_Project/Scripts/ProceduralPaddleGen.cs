using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class ProceduralPaddleGen : MonoBehaviour
{
    public PolygonCollider2D polygonCollider2D;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    
    public float radius = 1;
    public float paddleWidth = 0.5f;
    [OnValueChanged("GeneratePaddleMesh")]
    public float paddleArcLength;
    public float deltaAngle = 3f;

    private Vector2[] polygonVertices;
    private Vector3[] vertices;
    private int[] triangles;
    private Mesh mesh;
    
    void Start()
    {
        mesh = new Mesh();
        meshFilter.mesh = mesh;
        GeneratePaddleMesh();
    }

    public void GeneratePaddleMesh()
    {
        float totalAngle = (paddleArcLength / radius) * Mathf.Rad2Deg;
        int iterations = Mathf.CeilToInt(totalAngle / deltaAngle);     //multiplied by 2 for inner and outer vertices;
        vertices = new Vector3[iterations * 2];
        polygonVertices = new Vector2[iterations * 2];
        triangles = new int[(iterations - 1) * 6];

        float angle = 90 - totalAngle * 0.5f;
        Vector3 point = Vector3.zero;
        float innerWidth = radius - paddleWidth * 0.5f;
        float outerWidth = radius + paddleWidth * 0.5f;

        //Assign Vertices
        for (int i = 0; i < iterations; i++)
        {
            float angleInRadians = Mathf.Deg2Rad * angle;
            point.x = innerWidth * Mathf.Cos(angleInRadians);
            point.y = innerWidth * Mathf.Sin(angleInRadians);
            vertices[i] = point;
            
            point.x = outerWidth * Mathf.Cos(angleInRadians);
            point.y = outerWidth * Mathf.Sin(angleInRadians);
            vertices[i + iterations] = point;
            angle += deltaAngle;
        }

        //Assign Polygon Vertices
        for (int i = 0; i < iterations; i++)
        {
            polygonVertices[i] = vertices[i];
            polygonVertices[i + iterations] = vertices[iterations * 2 - i - 1];
        }

        //Assign Triangles
        int tris = 0;
        int vert = 0;
        for (int i = 0; i < iterations - 1; i++)
        {
            triangles[tris + 0] = vert;
            triangles[tris + 1] = vert + iterations + 1;
            triangles[tris + 2] = vert + iterations;
            triangles[tris + 3] = vert;
            triangles[tris + 4] = vert + 1;
            triangles[tris + 5] = vert + iterations + 1;
            tris += 6;
            vert++;
        }

        polygonCollider2D.points = polygonVertices;
        
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        Debug.Log("New Paddle Mesh Generated");
    }

    public void UpdatePaddleLength(float newLength)
    {
        paddleArcLength = newLength;
        GeneratePaddleMesh();
    }
}
