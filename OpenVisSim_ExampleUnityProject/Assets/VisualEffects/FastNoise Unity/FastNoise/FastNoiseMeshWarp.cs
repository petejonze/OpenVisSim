using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("FastNoise/FastNoise Mesh Warp", 3)]
public class FastNoiseMeshWarp : MonoBehaviour
{
	public FastNoiseUnity fastNoiseUnity;
	public bool fractal;

	private Dictionary<GameObject, Mesh> originalMeshes = new Dictionary<GameObject, Mesh>();

	// Use this for initialization
	void Start ()
	{
		WarpAllMeshes();
	}

	public void WarpAllMeshes()
	{
		foreach (MeshFilter meshFilter in gameObject.GetComponentsInChildren<MeshFilter>())
		{
			WarpMesh(meshFilter);
		}
	}

	public void WarpMesh(MeshFilter meshFilter)
	{
		if (meshFilter.sharedMesh == null)
			return;

		Vector3 offset = meshFilter.gameObject.transform.position - gameObject.transform.position;
		Vector3[] verts;

		if (originalMeshes.ContainsKey(meshFilter.gameObject))
		{
			verts = originalMeshes[meshFilter.gameObject].vertices;
		}
		else
		{
			originalMeshes[meshFilter.gameObject] = meshFilter.sharedMesh;
			verts = meshFilter.sharedMesh.vertices;
		}

		for (int i = 0; i < verts.Length; i++)
		{
			verts[i] += offset;

			if (fractal)
				fastNoiseUnity.fastNoise.GradientPerturbFractal(ref verts[i].x, ref verts[i].y, ref verts[i].z);
			else
				fastNoiseUnity.fastNoise.GradientPerturb(ref verts[i].x, ref verts[i].y, ref verts[i].z);

			verts[i] -= offset;
		}

		meshFilter.mesh.vertices = verts;
		meshFilter.mesh.RecalculateNormals();
		meshFilter.mesh.RecalculateBounds();
	}
}
