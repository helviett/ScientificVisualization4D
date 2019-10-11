using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tesseract : MonoBehaviour
{
	private List<Vector4> original { get; set; }
	private List<Vector4> verticies { get; set; } 
	private float x, y, z, w;
	public float lw = 2f;
	public float dx, dy, dz, dw;

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
        for (int i = 0; i < original.Count; i++) {
			verticies[i] = r * original[i];
		}
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

    void OnDrawGizmos()
	{
		if ((verticies?.Count ?? 0) == 0) {
			return;
		}
		Gizmos.color = Color.cyan;
		var pvs = new List<Vector3>();
		Transform transform = gameObject.GetComponent<Transform>();
		Gizmos.matrix = transform.localToWorldMatrix;
		if (verticies == null) {
			return;
		}
		foreach (var vertex in verticies) {
			var c = 1 / (lw - vertex.w);
			var projection = new float[][] {
				new []{c, 0, 0, 0},
				new []{0, c, 0, 0},
				new []{0, 0, c, 0},
			};
			var projected = Muiltiply(projection, vertex);
			pvs.Add(projected);
			Gizmos.DrawSphere(projected, 0.1f);
		}
		for (int i = 0; i < 4; i++) {
			Gizmos.DrawLine(pvs[i], pvs[(i + 1) % 4]);
			Gizmos.DrawLine(pvs[i + 4], pvs[(i + 1) % 4 + 4]);
			Gizmos.DrawLine(pvs[i], pvs[i +  4]);
		}
		
		for (int i = 0; i < 4; i++) {
			Gizmos.DrawLine(pvs[i + 8], pvs[(i + 1) % 4 + 8]);
			Gizmos.DrawLine(pvs[i + 4 + 8], pvs[(i + 1) % 4 + 4 + 8]);
			Gizmos.DrawLine(pvs[i + 8], pvs[i + 8 + 4]);
		}

		for (int i = 0; i < 8; i++) {
			Gizmos.DrawLine(pvs[i], pvs[i + 8]);
		}
	}

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
