using UnityEngine;

namespace VoxelEngine
{
	// Wrapper for Unity game object and components, allows it to be stored in an object pool
	public class ChunkGameObject
	{
		public GameObject gameObject;
		public MeshFilter meshFilter;
		public MeshRenderer meshRenderer;
		public MeshCollider meshCollider;

		public ChunkGameObject()
		{
			gameObject = new GameObject();
			gameObject.isStatic = true;
			meshFilter = gameObject.AddComponent<MeshFilter>();

#pragma warning disable 0162
			if (Chunk.GENERATE_COLLIDERS)
				meshCollider = gameObject.AddComponent<MeshCollider>();
#pragma warning restore 0162

			meshRenderer = gameObject.AddComponent<MeshRenderer>();
		}

		public void Setup(Vector3i chunkPos, Vector3 realPos, Transform parentTransform)
		{
			gameObject.transform.parent = parentTransform;
			gameObject.transform.position = realPos;
			gameObject.name = "Chunk (" + chunkPos.x + "," + chunkPos.y + "," + chunkPos.z + ")";
			gameObject.SetActive(true);
		}

		public void Clean()
		{
			gameObject.SetActive(false);
			gameObject.name = "Chunk (Pooled)";

			Object.Destroy(meshFilter.sharedMesh);
			meshFilter.sharedMesh = null;

#pragma warning disable 0162
			if (Chunk.GENERATE_COLLIDERS)
			{
				Object.Destroy(meshCollider.sharedMesh);
				meshCollider.sharedMesh = null;
			}
#pragma warning restore 0162
		}

		public void Destroy()
		{
			Object.Destroy(meshFilter.sharedMesh);
			meshFilter.sharedMesh = null;
			meshFilter = null;

#pragma warning disable 0162
			if (Chunk.GENERATE_COLLIDERS)
			{
				Object.Destroy(meshCollider.sharedMesh);
				meshCollider.sharedMesh = null;
				meshCollider = null;
			}
#pragma warning restore 0162

			Object.Destroy(gameObject);
			gameObject = null;
		}
	}
}