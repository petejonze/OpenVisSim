using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace VoxelEngine
{
	// This is the main class that manages all the chunks that create the voxel terrain
	// It is resonsible for loading and unloading chunks as the target transform moves around

	public class VoxelEngineManager : MonoBehaviour
	{
		public TerrainGeneratorBase terrainGenerator;
		public Transform targetTransform;
		public float loadDistance = 400f;
		public float unloadDistanceModifier = 1.2f;
		public float yDistanceModifier = 1.5f;
		public int maxThreads = 8;
		public float targetFPS = 100f;
		public Material meshMaterial;
		public bool showDebugInfo = true;

		// More low level voxel engine settings can be found in Chunk.cs

		private static ObjectPool<Chunk> chunkPool = new ObjectPool<Chunk>(128);
		private Dictionary<Vector3i, Chunk> chunkMap = new Dictionary<Vector3i, Chunk>();
		private ChunkQueue chunkQueue = new ChunkQueue();
		private Queue<Vector3i> chunkMeshQueue = new Queue<Vector3i>();
		private Stack<Chunk> chunkUnloadStack = new Stack<Chunk>();

		private int yLoadTick = -1;
		private int unloadTick = 0;
		private int threadCount = 0;
		private int meshesLastFrame = 0;
		private int updateTimerLastFrame = 0;
		private float averageFPS = 0.0f;
		private float deltaTimeFPS = 0.0f;

		void Start()
		{
			averageFPS = targetFPS;

			if (showDebugInfo)
				UnityEngine.Debug.Log("FastNoiseSIMD level: " + FastNoiseSIMD.GetSIMDLevel());
		}

		// Draw debug info and terrain generator buttons
		void OnGUI()
		{
			int labelSpacing = 18;
			Rect rect = new Rect(4, 0, 300, 20);

			if (showDebugInfo)
			{
				GUI.Label(rect, "Pooled Chunks: " + chunkPool.Count);
				rect.y += labelSpacing;
				GUI.Label(rect, "Pooled Chunk GameObjects: " + Chunk.chunkGameObjectPool.Count);
				rect.y += labelSpacing;
				GUI.Label(rect, "Chunks Loaded: " + chunkMap.Count);
				rect.y += labelSpacing;
				GUI.Label(rect, "Chunk Queue: " + chunkQueue.Count);
				rect.y += labelSpacing;
				GUI.Label(rect, "Chunk Mesh Queue: " + chunkMeshQueue.Count);
				rect.y += labelSpacing;
				GUI.Label(rect, "Meshes Last Frame: " + meshesLastFrame);
				rect.y += labelSpacing;
				GUI.Label(rect, "Update Time Last Frame: " + updateTimerLastFrame + "ms");
				rect.y += labelSpacing;
				GUI.Label(rect, "Thread Count: " + threadCount);
				rect.y += labelSpacing;
				GUI.Label(rect, "FPS: " + string.Format("{0:0.0}", averageFPS));
			}

			rect = new Rect(Screen.width - 172, 2, 170, 20);
			labelSpacing = 22;

			if (GUI.Button(rect, "Grass Hills"))
			{
				terrainGenerator = FindObjectOfType<TerrainGenerator_GrassLand>();
				ResetAll();
			}
			rect.y += labelSpacing;

			if (GUI.Button(rect, "Alien Planet"))
			{
				terrainGenerator = FindObjectOfType<TerrainGenerator_AlienPlanet>();
				ResetAll();
			}
			rect.y += labelSpacing;

			if (GUI.Button(rect, "Cracked Surface"))
			{
				terrainGenerator = FindObjectOfType<TerrainGenerator_CrackedSurface>();
				ResetAll();
			}
			rect.y += labelSpacing;

			if (GUI.Button(rect, "Desert (SIMD)"))
			{
				terrainGenerator = FindObjectOfType<TerrainGeneratorSIMD_Desert>();
				ResetAll();
			}
			rect.y += labelSpacing;

			if (GUI.Button(rect, "Floating Islands (SIMD)"))
			{
				terrainGenerator = FindObjectOfType<TerrainGeneratorSIMD_FloatingIslands>();
				ResetAll();
			}
		}

		void ResetAll()
		{
			UnloadAllChunks();
			targetTransform.position = new Vector3(0,50,0);
		}

		void Update()
		{
			deltaTimeFPS += (Time.deltaTime - deltaTimeFPS) * 0.1f;

			averageFPS = Mathf.Lerp(averageFPS, 1f/deltaTimeFPS, 0.05f);
			
			if (Input.GetKeyDown(KeyCode.Escape))
				Application.Quit();
		}

		// Uses called in late update since it is called after corountine updates allowing it to start new threads if they have just finished
		// Not using fixed update so that the updates will speed up/slow down based on PC performance
		void LateUpdate()
		{
			Stopwatch updateTimer = new Stopwatch();
			updateTimer.Start();
			
			UpdateLoadingQueue();

			CheckUnloadChunks();
		
			LoadChunksFromQueue();

			MeshChunksFromQueue(updateTimer);

			// For debug info
			updateTimerLastFrame = (int)updateTimer.ElapsedMilliseconds;
		}

		private void UpdateLoadingQueue()
		{
			// All distance checks use the distance squared since it saves calulcating a square root for every distance
			float loadDistanceSq = loadDistance * loadDistance;
			// Load distances in chunks to know how far to extend the for loop from the player's chunk
			int loadDistanceChunk = ((Mathf.CeilToInt(loadDistance) - Chunk.SIZE2) >> Chunk.BIT_SIZE) + 1;
			int loadDistanceChunkY = Mathf.CeilToInt(loadDistanceChunk * yDistanceModifier);

			// How much to sections to stagger the chunk location checking
			const int yCheckDelay = 8;

			Vector3i chunkPos = new Vector3i();
			Vector3 chunkRealPos = new Vector3();
			Vector3i targetChunk = new Vector3i(
				Mathf.RoundToInt(targetTransform.position.x) >> Chunk.BIT_SIZE,
				Mathf.RoundToInt(targetTransform.position.y) >> Chunk.BIT_SIZE,
				Mathf.RoundToInt(targetTransform.position.z) >> Chunk.BIT_SIZE);

			// yLoadTick staggers chunk location checking to reduce time spent each frame
			for (int y = yLoadTick - loadDistanceChunkY; y < loadDistanceChunkY; y += yCheckDelay)
			{
				chunkPos.y = targetChunk.y + y;
				chunkRealPos.y = ((y + targetChunk.y) << Chunk.BIT_SIZE) + Chunk.SIZE2;

				for (int x = -loadDistanceChunk; x < loadDistanceChunk; x++)
				{
					chunkPos.x = targetChunk.x + x;
					chunkRealPos.x = ((x + targetChunk.x) << Chunk.BIT_SIZE) + Chunk.SIZE2;

					for (int z = -loadDistanceChunk; z < loadDistanceChunk; z++)
					{
						chunkPos.z = targetChunk.z + z;

						// Don't try to queue the chunk location if is already loaded or already in queue
						if (chunkMap.ContainsKey(chunkPos) || chunkQueue.Contains(chunkPos))
							continue;

						chunkRealPos.z = ((z + targetChunk.z) << Chunk.BIT_SIZE) + Chunk.SIZE2;

						float distanceSq = ScaledTargetDistanceSq(chunkRealPos);

						if (distanceSq < loadDistanceSq)
							chunkQueue.Enqueue(distanceSq, chunkPos);
					}
				}
			}

			// Increment the yLoadTick so that different locations will be checked next frame
			if (++yLoadTick >= yCheckDelay)
				yLoadTick = 0;
		}

		private void CheckUnloadChunks()
		{
			float unloadDistanceSq = loadDistance * loadDistance * unloadDistanceModifier * unloadDistanceModifier;

			// Unloading sections stagger must be (2^n)-1
			const int unloadTickMax = 31;
			
			// Check if chunk is in stagger section then if it is outside the unload distance
			foreach (Chunk chunk in chunkMap.Values.Where(chunk => 
				(chunk.chunkPos.y & unloadTickMax) != unloadTick && 
				ScaledTargetDistanceSq(chunk.realPos) > unloadDistanceSq))
			{
				chunkUnloadStack.Push(chunk);
			}

			if (++unloadTick > unloadTickMax)
				unloadTick = 0;

			// Unload chunks outside the foreach to avoid removing elements causing errors
			while (chunkUnloadStack.Count != 0)
			{
				UnloadChunk(chunkUnloadStack.Pop());
			}
		}

		private void LoadChunksFromQueue()
		{
			Vector3i chunkPos = new Vector3i();

			int adjustedMaxThreads = Mathf.RoundToInt(maxThreads - chunkMeshQueue.Count*0.2f);

			while (threadCount < adjustedMaxThreads)
			{
				// Get the closest chunk location from the queue if one exists
				if (!chunkQueue.Dequeue(out chunkPos))
					break;
				
				// Threaded
				StartCoroutine(LoadChunkThreaded(chunkPos));

				// Not threaded
				//LoadChunkThreaded(chunkPos);
			}
		}

		private void MeshChunksFromQueue(Stopwatch updateTimer)
		{
			// For debug info
			meshesLastFrame = 0;

			// Allow more time meshing if above target FPS
			int milliMax = Mathf.RoundToInt(averageFPS - targetFPS);

			while (chunkMeshQueue.Count > 0)
			{
				Chunk chunk;

				// Try and get the chunk from it's postion (it may have been unloaded since it was added to queue)
				if (!chunkMap.TryGetValue(chunkMeshQueue.Dequeue(), out chunk))
					continue;

				// This should always be true, but adjacent chunks may have unloaded since being added to queue
				if (chunk.CanBuildMesh())
				{
					chunk.BuildMesh();
					meshesLastFrame++;
				}
				else
					continue;
				
				// Stop meshing if too long has been spent updating this frame
				// This is at the end of the loop to ensure at least 1 mesh will generate per frame
				if (updateTimer.ElapsedMilliseconds >= milliMax)
					break;
			}
			
		}

		// Get distance squared to target using the yDistanceModifier
		public float ScaledTargetDistanceSq(Vector3 realPos)
		{
			return new Vector3(
				targetTransform.position.x - realPos.x,
				(targetTransform.position.y - realPos.y) * yDistanceModifier,
				targetTransform.position.z - realPos.z).sqrMagnitude;
		}

		public void LoadChunk(Vector3i chunkPos)
		{
			Chunk chunk = chunkPool.Get();
			chunk.Setup(chunkPos, this);

			// Skip generating if outside terrain generator bounds
			if (chunk.CheckTerrainBounds())
				chunk.GenerateVoxelData();

			// Notify adjacent chunks this chunk can be used for meshing
			chunk.FillAdjChunks();
			// Added the chunk to the dictionary
			chunkMap.Add(chunkPos, chunk);
			// Mark the chunk position as complete and remove from the queue
			chunkQueue.Remove(chunkPos);
		}

		public IEnumerator LoadChunkThreaded(Vector3i chunkPos)
		{
			Chunk chunk = chunkPool.Get();
			chunk.Setup(chunkPos, this);

			// Skip generating if outside terrain generator bounds
			if (chunk.CheckTerrainBounds())
			{
				// Start a new thread to generate the voxel data
				threadCount++;
				bool done = false;
				Thread thread = new Thread(() =>
				{
					chunk.GenerateVoxelData();
					done = true;
				})
				{
					Priority = System.Threading.ThreadPriority.BelowNormal
				};

				thread.Start();

				// Corountine waits for the thread to finish before continuing on the main thread
				while (!done)
					yield return null;

				threadCount--;
			}

			// Notify adjacent chunks this chunk can be used for meshing
			chunk.FillAdjChunks();

			// Added the chunk to the dictionary
			chunkMap.Add(chunkPos, chunk);

			// Mark the chunk position as complete and remove from the queue
			// This is needed for threaded loading as there is a delay between dequeuing and it being added to the chunkMap
			chunkQueue.Remove(chunkPos);
		}

		// Clear the chunkMap and all queues
		// Use this if changing terrain/meshing to load with updated values
		public void UnloadAllChunks()
		{
			// Stop all threaded chunk loading and reset thread counter
			StopAllCoroutines();
			threadCount = 0;

			foreach (Chunk chunk in chunkMap.Values)
			{
				chunkUnloadStack.Push(chunk);
			}

			while (chunkUnloadStack.Count != 0)
			{
				UnloadChunk(chunkUnloadStack.Pop());
			}

			chunkQueue.Clear();
			chunkMeshQueue.Clear();
		}

		public void UnloadChunk(Chunk chunk)
		{
			chunkMap.Remove(chunk.chunkPos);

			// Try to add the chunk object to the pool, if not destroy it
			if (chunkPool.Add(chunk))
				chunk.Clean();
			else
				chunk.Destroy();
		}

		// Try and get a chunk, returns null if chunk is not loaded
		public Chunk GetChunk(Vector3i chunkPos)
		{
			Chunk chunk;
			chunkMap.TryGetValue(chunkPos, out chunk);
			return chunk;
		}

		// Returns a chunk, if the chunk is not loaded this will throw an exeption
		public Chunk GetChunkUnsafe(Vector3i chunkPos)
		{
			return chunkMap[chunkPos];
		}

		// Used by chunks to queue themselves for meshing
		public void QueueChunkMeshing(Vector3i chunkPos)
		{
			chunkMeshQueue.Enqueue(chunkPos);
		}
	}
}