using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tesseract : MonoBehaviour
{
	public Mesh vertexMesh;
	public Mesh edgeMesh;
	public Material material;
	private List<Vector4> original { get; set; }
	private List<Vector4> verticies { get; set; }
	private float x, y, z, w;
	public float lw = 2f;
	public float dx, dy, dz, dw;
    private float scaleFactor = 0.1f;
    void Start()
    {
		verticies = new List<Vector4> {
			new Vector4(-1, -1, -1, 1),
			new Vector4(1, -1, -1, 1),
			new Vector4(1, 1, -1, 1),
			new Vector4(-1, 1, -1, 1),
			new Vector4(-1, -1, 1, 1),
			new Vector4(1, -1, 1, 1),
			new Vector4(1, 1, 1, 1),
			new Vector4(-1, 1, 1, 1),
			new Vector4(-1, -1, -1, -1),
			new Vector4(1, -1, -1, -1),
			new Vector4(1, 1, -1, -1),
			new Vector4(-1, 1, -1, -1),
			new Vector4(-1, -1, 1, -1),
			new Vector4(1, -1, 1, -1),  
			new Vector4(1, 1, 1, -1),
			new Vector4(-1, 1, 1, -1),
		};
		original = new List<Vector4>(verticies);
     
		var scale = new Matrix4x4(
			new Vector4(scaleFactor, 0f, 0f, 0f), 
			new Vector4(0f, scaleFactor, 0f, 0f),
			new Vector4(0f, 0f, scaleFactor, 0f),
			new Vector4(0f, 0f, 0f, 1.0f)
		);
        edgeMesh = Instantiate(edgeMesh);
        vertexMesh = Instantiate(vertexMesh);
		var vertices = vertexMesh.vertices;
		var newVertices = new Vector3[vertexMesh.vertexCount];
		for (int i = 0; i < newVertices.Length; i++) {
			newVertices[i] = scale * vertices[i];
		}
        vertexMesh.vertices = newVertices;  
        vertices = edgeMesh.vertices;
        newVertices = new Vector3[edgeMesh.vertexCount];
        for (int i = 0; i < newVertices.Length; i++)
        {
            newVertices[i] = scale * vertices[i];
        }
        edgeMesh.vertices = newVertices;
    }

    // Update is called once per frame
    void Update()
    {
        if ((verticies?.Count ?? 0) == 0) {
            return;
        }
        x += dx;
        y += dy;
        z += dz;
        w += dw;
        var r = RotateX(x) * RotateY(y) * RotateZ(z) * RotateW(w);
        for (int i = 0; i < original.Count; i++)
        {
            verticies[i] = r * original[i];
        }
        var pvs = new List<Vector3>();
        var transform = gameObject.GetComponent<Transform>();
        foreach (var vertex in verticies) {
            var c = 1 / (lw - vertex.w);
            var projection = new float[][] {
                new []{c, 0, 0, 0},
                new []{0, c, 0, 0},
                new []{0, 0, c, 0},
            };
            var projected = transform.TransformVector(Muiltiply(projection, vertex));
            pvs.Add(projected);
            Graphics.DrawMesh(vertexMesh, projected, Quaternion.identity, material, 31);
            // pvs.Add(projected);
            // Gizmos.DrawSphere(projected, 0.1f);
        }
        var j = 0;
        for (int i = 0; i < 4; i++)
        {
            DrawEdge(pvs[i], pvs[(i + 1) % 4]);
            DrawEdge(pvs[i + 4], pvs[(i + 1) % 4 + 4]);
            DrawEdge(pvs[i], pvs[i + 4]);
        }

        for (int i = 0; i < 4; i++)
        {
            DrawEdge(pvs[i + 8], pvs[(i + 1) % 4 + 8]);
            DrawEdge(pvs[i + 4 + 8], pvs[(i + 1) % 4 + 4 + 8]);
            DrawEdge(pvs[i + 8], pvs[i + 8 + 4]);
        }

        for (int i = 0; i < 8; i++)
        {
            DrawEdge(pvs[i], pvs[i + 8]);
        }

    }

    void DrawEdge(Vector3 start, Vector3 end, float lineWidth = 2000f)
    {
        var edge = new Mesh();
        var vertices = new Vector3[edgeMesh.vertices.Length];
        var uv = new Vector2[edgeMesh.uv.Length];
        var triangles = new int[edgeMesh.triangles.Length];
        System.Array.Copy(edgeMesh.vertices, vertices, edgeMesh.vertices.Length);
        System.Array.Copy(edgeMesh.uv, uv, edgeMesh.uv.Length);
        System.Array.Copy(edgeMesh.triangles, triangles, edgeMesh.triangles.Length);
        edge.vertices = vertices;
        edge.uv = uv;
        edge.triangles = triangles;
        edge.normals = edgeMesh.normals;
        //var dir = end - start;
        //Vector3 normal = Vector3.Cross(end, start);
        //Vector3 side = Vector3.Cross(normal, end - start);
        var k = (end - start).magnitude;
        var size = edgeMesh.bounds.size.y;
        var angle = Quaternion.FromToRotation(Vector3.up, (end - start));
        Graphics.DrawMesh(edge, Matrix4x4.TRS((start + end) / 2f, angle, new Vector3(1, k / size / scaleFactor, 1)), material, 31); //(start + end) / 2f, angle, material, 31);
    }

    Matrix4x4 RotateZ(float phi) => new Matrix4x4 {
        m00 = Mathf.Cos(phi), m01 = -Mathf.Sin(phi), m02 = 0, m03 = 0,
        m10 = Mathf.Sin(phi), m11 = Mathf.Cos(phi), m12 = 0, m13 = 0,
        m20 = 0, m21 = 0, m22 = 1, m23 = 0,
        m30 = 0, m31 = 0, m32 = 0, m33 = 1,
    };

    Matrix4x4 RotateY(float phi) => new Matrix4x4 {
        m00 = Mathf.Cos(phi), m01 = 0, m02 = Mathf.Sin(phi), m03 = 0,
        m10 = 0, m11 = 1, m12 = 0, m13 = 0,
        m20 = -Mathf.Sin(phi), m21 = 0, m22 = Mathf.Cos(phi), m23 = 0,
        m30 = 0, m31 = 0, m32 = 0, m33 = 1,
    };

    Matrix4x4 RotateX(float phi) => new Matrix4x4 {
        m00 = 1, m01 = 0, m02 = 0, m03 = 0,
        m10 = 0, m11 = Mathf.Cos(phi), m12 = -Mathf.Sin(phi), m13 = 0,
        m20 = 0, m21 = Mathf.Sin(phi), m22 = Mathf.Cos(phi), m23 = 0,
        m30 = 0, m31 = 0, m32 = 0, m33 = 1,
    };

    Matrix4x4 RotateW(float phi) => new Matrix4x4 {
        m00 = 1, m01 = 0, m02 = 0, m03 = 0,
        m10 = 0, m11 = 1, m12 = 0, m13 = 0,
        m20 = 0, m21 = 0, m22 = Mathf.Cos(phi), m23 = -Mathf.Sin(phi),
        m30 = 0, m31 = 0, m32 = Mathf.Sin(phi), m33 = Mathf.Cos(phi),
    };

    // void OnDrawGizmos()
	// {
	// 	if ((verticies?.Count ?? 0) == 0) {
	// 		return;
	// 	}
	// 	Gizmos.color = Color.cyan;
	// 	var pvs = new List<Vector3>();
	// 	Transform transform = gameObject.GetComponent<Transform>();
	// 	Gizmos.matrix = transform.localToWorldMatrix;
	// 	if (verticies == null) {
	// 		return;
	// 	}
	// 	foreach (var vertex in verticies) {
	// 		var c = 1 / (lw - vertex.w);
	// 		var projection = new float[][] {
	// 			new []{c, 0, 0, 0},
	// 			new []{0, c, 0, 0},
	// 			new []{0, 0, c, 0},
	// 		};
	// 		var projected = Muiltiply(projection, vertex);
	// 		pvs.Add(projected);
	// 		Gizmos.DrawSphere(projected, 0.1f);
	// 	}
	// 	for (int i = 0; i < 4; i++) {
	// 		Gizmos.DrawLine(pvs[i], pvs[(i + 1) % 4]);
	// 		Gizmos.DrawLine(pvs[i + 4], pvs[(i + 1) % 4 + 4]);
	// 		Gizmos.DrawLine(pvs[i], pvs[i +  4]);
	// 	}
		
	// 	for (int i = 0; i < 4; i++) {
	// 		Gizmos.DrawLine(pvs[i + 8], pvs[(i + 1) % 4 + 8]);
	// 		Gizmos.DrawLine(pvs[i + 4 + 8], pvs[(i + 1) % 4 + 4 + 8]);
	// 		Gizmos.DrawLine(pvs[i + 8], pvs[i + 8 + 4]);
	// 	}

	// 	for (int i = 0; i < 8; i++) {
	// 		Gizmos.DrawLine(pvs[i], pvs[i + 8]);
	// 	}
	// }

	Vector3 Muiltiply(float[][] m, Vector4 vector) 
	{
		var result = new Vector3(0f, 0f, 0f);
		for (int i = 0; i < m.Length; i++) {
			for (int j = 0; j < 1; j++) {
				for (int k = 0; k < 4; k++) {
					result[i] += m[i][k] * vector[k];
				}
			}
		}
		return result;
	}
}
