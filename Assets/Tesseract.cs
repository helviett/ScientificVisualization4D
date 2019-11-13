using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Tesseract : MonoBehaviour
{
	public Mesh vertexMesh;
	public Mesh edgeMesh;
	public Material material;
    public Material faceMaterial;
	private List<Vector4> original { get; set; }
	private List<Vector4> verticies { get; set; }
	private float x, y, z, w;
	public float lw = 2f;
	public float dx, dy, dz, dw;
    private List<Mesh> edges;
    private List<Mesh> faces;
    private int currentFaceIdx;

    private float scaleFactor = 0.1f;

    public SteamVR_Action_Boolean ButtonX;
    public SteamVR_Action_Boolean ButtonY;

    public SteamVR_Input_Sources handType;

    public void BtnXUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Debug.Log("Button X is up");
        dw = 0.0f;
    }
    public void BtnXDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Debug.Log("Button X is down");
        dw = -0.005f;
    }

    public void BtnYUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Debug.Log("Button Y is up");
        dw = 0.0f;
    }
    public void BtnYDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Debug.Log("Button Y is down");
        dw = 0.005f;
    }

    void Start()
    {
        ButtonX.AddOnStateDownListener(BtnXDown, handType);
        ButtonX.AddOnStateUpListener(BtnXUp, handType);
        ButtonY.AddOnStateDownListener(BtnYDown, handType);
        ButtonY.AddOnStateUpListener(BtnYUp, handType);

        edges = new List<Mesh>();
        faces = new List<Mesh>();
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
        }

        int edgeDrawIdx = -1;

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j <= 8; j+=8)
            {
                DrawEdge(pvs[i + j], pvs[(i + 1) % 4 + j], ++edgeDrawIdx);
                DrawEdge(pvs[i + 4 + j], pvs[(i + 1) % 4 + 4 + j], ++edgeDrawIdx);
                DrawEdge(pvs[i + j], pvs[i + 4 + j], ++edgeDrawIdx);
            }
        }

        for (int i = 0; i < 8; i++)
        {
            DrawEdge(pvs[i], pvs[i + 8], ++edgeDrawIdx);
        }

        currentFaceIdx = -1;
		for (int i = 0; i < 4; i++)
		{
			var j = i * 4;
			DrawFace(pvs[j], pvs[j + 1], pvs[j + 2], pvs[j + 3]);
		}
		for (int i = 0; i < 6; i++)
		{
			if (i == 2)
			{
				i += 2;
			}
			var j = i * 2;
			DrawFace(pvs[j], pvs[j + 1], pvs[j + 5], pvs[j + 4]);
		}

		DrawFace(pvs[1], pvs[2], pvs[6], pvs[5]);
		DrawFace(pvs[0], pvs[3], pvs[7], pvs[4]);
		DrawFace(pvs[8], pvs[11], pvs[15], pvs[12]);
		DrawFace(pvs[9], pvs[10], pvs[14], pvs[13]);

		// Inner

		DrawFace(pvs[0], pvs[8], pvs[9], pvs[1]);
		DrawFace(pvs[3], pvs[11], pvs[10], pvs[2]);
		DrawFace(pvs[4], pvs[12], pvs[13], pvs[5]);
		DrawFace(pvs[7], pvs[15], pvs[14], pvs[6]);

		DrawFace(pvs[0], pvs[4], pvs[12], pvs[8]);
		DrawFace(pvs[3], pvs[7], pvs[15], pvs[11]);
		DrawFace(pvs[2], pvs[6], pvs[14], pvs[10]);
		DrawFace(pvs[1], pvs[5], pvs[13], pvs[9]);

		DrawFace(pvs[0], pvs[3], pvs[11], pvs[8]);
		DrawFace(pvs[1], pvs[2], pvs[10], pvs[9]);
		DrawFace(pvs[5], pvs[6], pvs[14], pvs[13]);
		DrawFace(pvs[4], pvs[7], pvs[15], pvs[12]);
	}

    void DrawFace(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        if (++currentFaceIdx < faces.Count)
            faces[currentFaceIdx].Clear();
        else
            faces.Add(new Mesh());

        var face = faces[currentFaceIdx];
        var vertices = new Vector3[] { p0, p1, p2, p3 };
        var indecies = new int[] {
            0, 2, 1,
            0, 3, 2,
            0, 1, 2,
            0, 2, 3,
        };
        face.vertices = vertices;
        face.triangles = indecies;
        // face.RecalculateNormals();
        Graphics.DrawMesh(face, Matrix4x4.identity, faceMaterial, 31);
    }

    void DrawEdge(Vector3 start, Vector3 end, int edgeIdx, float lineWidth = 2000f)
     {
        if (edgeIdx < edges.Count)
            edges[edgeIdx].Clear();
        else
            edges.Add(new Mesh());

        var edge = edges[edgeIdx];

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
