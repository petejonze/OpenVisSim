using System;
using UnityEngine;
using System.Collections.Generic;

namespace VoxelEngine
{
	public class Chunk
	{
		// Chunk size in bits (5 = 32) (4 = 16)
		public const int BIT_SIZE = 5;

		// Create mesh colliders on chunk game objects
		public const bool GENERATE_COLLIDERS = false;

		// Use gradient adjustment to smooth mesh (Not blocky)
		public const bool GRADIENT_MESH = false;

		// Calculate and bake ambient occlusion into voxel colours
		public const bool GENERATE_AMBIENT_OCCLUSION = true;

		// Intesity of ambient occlusion
		public const float AMBIENT_OCCLUSION_STRENGTH = 0.2f;

		// Don't touch these ----------------
		public const int SIZE = 1 << BIT_SIZE;
		public const int SIZE2 = SIZE >> 1;
		public const int BIT_MASK = SIZE - 1;

		internal const int VOXEL_STEP_X = SIZE * SIZE;
		internal const int VOXEL_STEP_Y = SIZE;
		internal const int VOXEL_STEP_Z = 1;
		internal const int VOXEL_STEP_CHUNK_X = VOXEL_STEP_X * SIZE;
		internal const int VOXEL_STEP_CHUNK_Y = VOXEL_STEP_Y * SIZE;
		internal const int VOXEL_STEP_CHUNK_Z = VOXEL_STEP_Z * SIZE;
#pragma warning disable 0429
		internal const int ADJ_CHUNK_SIZE = GENERATE_AMBIENT_OCCLUSION ? 7 : 3;
		internal const MeshBuilder.MeshType MESH_TYPE = GRADIENT_MESH ? MeshBuilder.MeshType.Gradient :
			GENERATE_AMBIENT_OCCLUSION ? MeshBuilder.MeshType.AmbientOcclusion : MeshBuilder.MeshType.Basic;
#pragma warning restore 0429

		private static readonly Vector3i[] adjChunkVectors =
		{
			new Vector3i(-1, 0, 0),
			new Vector3i(0, -1, 0),
			new Vector3i(0, 0, -1),
			new Vector3i(-1, -1, 0),
			new Vector3i(-1, 0, -1),
			new Vector3i(0, -1, -1),
			new Vector3i(-1, -1, -1),
		};

		public enum AdjDirection
		{
			Left,
			Down,
			Back,
			LeftDown,
			LeftBack,
			DownBack,
			LeftDownBack,
		};

		public enum FillType
		{
			Null,
			Mixed,
			Empty,
			Solid,
		};

		public Vector3i chunkPos;
		public Vector3 realPos;
		public ChunkGameObject chunkGameObject;
		public VoxelEngineManager voxelEngineManager;

		internal FillType fillType = FillType.Null;
		internal Voxel[] voxelData = new Voxel[SIZE * SIZE * SIZE];
		internal Chunk[] adjChunks = new Chunk[ADJ_CHUNK_SIZE];
		internal bool dirtyMesh = false;

		public static ObjectPool<ChunkGameObject> chunkGameObjectPool = new ObjectPool<ChunkGameObject>(64);

		// Used instead of a constructor because the object is retrevied from a pool
		public void Setup(Vector3i chunkPos, VoxelEngineManager voxelEngineManager)
		{
			this.chunkPos = chunkPos;
			this.voxelEngineManager = voxelEngineManager;
			realPos = new Vector3(chunkPos.x << BIT_SIZE, chunkPos.y << BIT_SIZE, chunkPos.z << BIT_SIZE);
			dirtyMesh = false;
		}

		// Called before putting a chunk back into the chunk pool
		public void Clean()
		{
			ReleaseChunkGameObject();
			ReleaseAdjChunks();
		}

		// Called if the chunk pool is full and object is destroyed
		public void Destroy()
		{
			ReleaseChunkGameObject();
			ReleaseAdjChunks();
		}

		// Notify adjacent chunks that chunk is unloading by nulling reference
		private void ReleaseAdjChunks()
		{
			for (int i = 0; i < ADJ_CHUNK_SIZE; i++)
			{
				adjChunks[i] = null;

				Chunk invAdjChunk = voxelEngineManager.GetChunk(chunkPos - adjChunkVectors[i]);

				if (invAdjChunk != null)
					invAdjChunk.UpdateAdjChunk(null, i);
			}
		}

		// Try release the Unity game object back into the game object pool
		private void ReleaseChunkGameObject()
		{
			if (chunkGameObject == null)
				return;

			if (chunkGameObjectPool.Add(chunkGameObject))
				chunkGameObject.Clean();
			else
				chunkGameObject.Destroy();

			chunkGameObject = null;
		}

		// Convert a local voxel position to its index in the voxelData
		public static int VoxelDataIndex(int localX, int localY, int localZ)
		{
			return localZ | (localY << BIT_SIZE) | (localX << (BIT_SIZE * 2));
		}

		// Return the voxel at the given local position, no bounds checking
		public Voxel GetVoxelUnsafe(int localX, int localY, int localZ)
		{
			return voxelData[VoxelDataIndex(localX, localY, localZ)];
		}

		// Return the voxel at the given local position if within chunk bounds
		public Voxel GetVoxel(int localX, int localY, int localZ)
		{
			if ((localX & BIT_MASK) != localX ||
				(localY & BIT_MASK) != localY ||
				(localZ & BIT_MASK) != localZ)
				return Voxel.Empty;

			return voxelData[VoxelDataIndex(localX, localY, localZ)];
		}

		// Try to find adjacent chunks to inform them of this chunk and fill the adjChunk lookup
		public void FillAdjChunks()
		{
			bool adjReady = true;
			FillType adjType = fillType;

			// Adjacent chunks are needed to mesh the chunk
			for (int i = 0; i < ADJ_CHUNK_SIZE; i++)
			{
				adjChunks[i] = voxelEngineManager.GetChunk(chunkPos + adjChunkVectors[i]);

				if (adjChunks[i] != null)
				{
					adjType = adjChunks[i].fillType == adjType ? adjType : FillType.Mixed;
				}
				else
					adjReady = false;

				Chunk invAdjChunk = voxelEngineManager.GetChunk(chunkPos - adjChunkVectors[i]);

				if (invAdjChunk != null)
					invAdjChunk.UpdateAdjChunk(this, i);
			}

			// Cancel meshing if not all adjacent chunks are loaded
			if (!adjReady)
				return;

			// If adjacent chunks are all full or all empty no meshing is needed
			if (adjType != FillType.Mixed)
			{
				dirtyMesh = false;
				return;
			}

			if (dirtyMesh)
				voxelEngineManager.QueueChunkMeshing(chunkPos);
		}

		// Called by other chunks to inform they are loaded and ready to be used for meshing
		// or removed if other chunk is unloading
		public void UpdateAdjChunk(Chunk chunk, int side)
		{
			adjChunks[side] = chunk;
			
			if (!dirtyMesh || (chunk == null))
				return;

			// Check if all adjacent chunks are ready to queue for meshing
			if (CanBuildMesh())
				voxelEngineManager.QueueChunkMeshing(chunkPos);
		}

		// Checks if all adjacent chunks are loaded and not all full or all empty
		public bool CanBuildMesh()
		{
			FillType adjType = fillType;

			for (int i = 0; i < ADJ_CHUNK_SIZE; i++)
			{
				if (adjChunks[i] == null)
					return false;

				adjType = adjChunks[i].fillType == adjType ? adjType : FillType.Mixed;
			}

			if (adjType != FillType.Mixed)
			{
				dirtyMesh = false;
				return false;
			}
			return true;
		}

		// Get the min and max height from the terrain generator to check if this chunk is in it's bounds
		// Fill the chunk with the terrain generators min/max voxels if not
		public bool CheckTerrainBounds()
		{
			dirtyMesh = true;

			if (voxelEngineManager.terrainGenerator.MinHeight() > realPos.y + SIZE)
			{
				FloodVoxelData(voxelEngineManager.terrainGenerator.MinVoxel());
				return false;
			}

			if (voxelEngineManager.terrainGenerator.MaxHeight() < realPos.y)
			{
				FloodVoxelData(voxelEngineManager.terrainGenerator.MaxVoxel());
				return false;
			}
			return true;
		}

		// Flood the voxel data with the given voxel and update chunk fill type accordingly
		private void FloodVoxelData(Voxel floodVoxel)
		{
			fillType = FillType.Null;
			TerrainGeneratorBase.ChunkFillUpdate(this, floodVoxel);

			int index = 0;

			for (int i = 0; i < SIZE * SIZE * SIZE; i++)
			{
				voxelData[index++] = floodVoxel;
			}
		}

		// Send the chunk to the terrain generator to fill the voxel data
		public void GenerateVoxelData()
		{
			dirtyMesh = true;
			fillType = FillType.Null;
			voxelEngineManager.terrainGenerator.GenerateChunk(this);
		}

		// Builds the mesh for the chunk
		public void BuildMesh()
		{
			if (!dirtyMesh)
				return;

			dirtyMesh = false;

			Mesh mesh = MeshBuilder.BuildMesh(this, MESH_TYPE);

			// Mesh can be null if it contained no verticies
			if (mesh == null)
			{
				ReleaseChunkGameObject();
				return;
			}

			// Get a new game object from the pool if nessecary
			if (chunkGameObject == null)
			{
				chunkGameObject = chunkGameObjectPool.Get();
				chunkGameObject.Setup(chunkPos, realPos, voxelEngineManager.gameObject.transform);
				chunkGameObject.meshRenderer.material = voxelEngineManager.meshMaterial;
			}

			chunkGameObject.meshFilter.sharedMesh = mesh;

#pragma warning disable 0162
			if (GENERATE_COLLIDERS)
				chunkGameObject.meshCollider.sharedMesh = mesh;
#pragma warning restore 0162
		}
	}
}