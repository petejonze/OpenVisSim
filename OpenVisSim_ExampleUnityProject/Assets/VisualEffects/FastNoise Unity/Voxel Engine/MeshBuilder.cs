using System;
using UnityEngine;
using System.Collections.Generic;
using System.Net;

namespace VoxelEngine
{
	public class MeshBuilder
	{
		private const int ADJ_BIT_SIZE = Chunk.BIT_SIZE + 1;
		private const int ADJ_SIZE = 1 << ADJ_BIT_SIZE;
		private const int ADJ_VOXEL_STEP_X = ADJ_SIZE * ADJ_SIZE;
		private const int ADJ_VOXEL_STEP_Y = ADJ_SIZE;
		private const int ADJ_VOXEL_STEP_Z = 1;

		public static readonly Vector3[] directionNormals =
		{
			Vector3.left,
			Vector3.right,
			Vector3.down,
			Vector3.up,
			Vector3.back,
			Vector3.forward,
		};

		public enum Direction
		{
			Left,
			Right,
			Down,
			Up,
			Back,
			Forward,
		};

		public enum MeshType
		{
			Basic,
			AmbientOcclusion,
			Gradient,
		}

		private static Chunk chunk;
		private static List<Quad> quads = new List<Quad>();
		private static Dictionary<Vector3, int> lightLevels = new Dictionary<Vector3, int>();
		private static Dictionary<int, Vector3> gradientVerts = new Dictionary<int, Vector3>();

		public static void Clean()
		{
			chunk = null;
			quads.Clear();
			lightLevels.Clear();
			gradientVerts.Clear();
		}

		public static Mesh BuildMesh(Chunk chunk, MeshType meshType)
		{
			MeshBuilder.chunk = chunk;
			Mesh mesh = null;

			switch (meshType)
			{
				case MeshType.Basic:
					mesh = BasicMesh();
					break;
				case MeshType.AmbientOcclusion:
					mesh = AmbientOcclusionMesh();
					break;
				case MeshType.Gradient:
					mesh = GradientMesh();
					break;
				default:
					throw new ArgumentOutOfRangeException("meshType", meshType, null);
			}

			Clean();
			return mesh;
		}

		private static Mesh BasicMesh()
		{
			TerrainGeneratorBase terrainGenerator = chunk.voxelEngineManager.terrainGenerator;
			int index = -1;

			for (int x = 0; x < Chunk.SIZE; x++)
			{
				for (int y = 0; y < Chunk.SIZE; y++)
				{
					for (int z = 0; z < Chunk.SIZE; z++)
					{
						Voxel voxel = chunk.voxelData[++index];
						Voxel left = GetAdjVoxelLeft(index, x);
						Voxel down = GetAdjVoxelDown(index, y);
						Voxel back = GetAdjVoxelBack(index, z);

						if (voxel.IsSolid())
						{
							Color32 color = new Color32();
							bool colorInit = false;

							// Left
							if (!left.IsSolid())
							{
								colorInit = true;
								color = terrainGenerator.DensityColor(voxel);

								quads.Add(new Quad(
									new Vector3(x - 0.5f, y - 0.5f, z - 0.5f),
									new Vector3(x - 0.5f, y - 0.5f, z + 0.5f),
									new Vector3(x - 0.5f, y + 0.5f, z + 0.5f),
									new Vector3(x - 0.5f, y + 0.5f, z - 0.5f),
									color, Direction.Left));
							}

							// Down
							if (!down.IsSolid())
							{
								if (!colorInit)
								{
									colorInit = true;
									color = terrainGenerator.DensityColor(voxel);
								}

								quads.Add(new Quad(
									new Vector3(x - 0.5f, y - 0.5f, z - 0.5f),
									new Vector3(x + 0.5f, y - 0.5f, z - 0.5f),
									new Vector3(x + 0.5f, y - 0.5f, z + 0.5f),
									new Vector3(x - 0.5f, y - 0.5f, z + 0.5f),
									color, Direction.Down));
							}

							// Back
							if (!back.IsSolid())
							{
								if (!colorInit)
									color = terrainGenerator.DensityColor(voxel);

								quads.Add(new Quad(
									new Vector3(x - 0.5f, y - 0.5f, z - 0.5f),
									new Vector3(x - 0.5f, y + 0.5f, z - 0.5f),
									new Vector3(x + 0.5f, y + 0.5f, z - 0.5f),
									new Vector3(x + 0.5f, y - 0.5f, z - 0.5f),
									color, Direction.Back));
							}
						}
						else // Voxel not solid
						{
							// Left
							if (left.IsSolid())
							{
								quads.Add(new Quad(
									new Vector3(x - 0.5f, y + 0.5f, z - 0.5f),
									new Vector3(x - 0.5f, y + 0.5f, z + 0.5f),
									new Vector3(x - 0.5f, y - 0.5f, z + 0.5f),
									new Vector3(x - 0.5f, y - 0.5f, z - 0.5f),
									terrainGenerator.DensityColor(left), Direction.Right));
							}

							// Down
							if (down.IsSolid())
							{
								quads.Add(new Quad(
									new Vector3(x - 0.5f, y - 0.5f, z + 0.5f),
									new Vector3(x + 0.5f, y - 0.5f, z + 0.5f),
									new Vector3(x + 0.5f, y - 0.5f, z - 0.5f),
									new Vector3(x - 0.5f, y - 0.5f, z - 0.5f),
									terrainGenerator.DensityColor(down), Direction.Up));
							}

							// Back
							if (back.IsSolid())
							{
								quads.Add(new Quad(
									new Vector3(x + 0.5f, y - 0.5f, z - 0.5f),
									new Vector3(x + 0.5f, y + 0.5f, z - 0.5f),
									new Vector3(x - 0.5f, y + 0.5f, z - 0.5f),
									new Vector3(x - 0.5f, y - 0.5f, z - 0.5f),
									terrainGenerator.DensityColor(back), Direction.Forward));
							}
						}
					}
				}
			}

			if (quads.Count == 0)
				return null;

			Vector3[] verts = new Vector3[quads.Count * 4];
			Vector3[] normals = new Vector3[quads.Count * 4];
			//Vector2[] uvs = new Vector2[quads.Count * 4];
			Color32[] colors = new Color32[quads.Count * 4];
			int[] tris = new int[quads.Count * 6];

			int vertIndex = 0;
			int triIndex = 0;

			foreach (Quad quad in quads)
			{
				tris[triIndex++] = vertIndex;
				tris[triIndex++] = vertIndex + 1;
				tris[triIndex++] = vertIndex + 2;
				tris[triIndex++] = vertIndex;
				tris[triIndex++] = vertIndex + 2;
				tris[triIndex++] = vertIndex + 3;

				colors[vertIndex] = quad.color;
				normals[vertIndex] = directionNormals[(int)quad.direction];
				verts[vertIndex++] = quad.v0;
				colors[vertIndex] = quad.color;
				normals[vertIndex] = directionNormals[(int)quad.direction];
				verts[vertIndex++] = quad.v1;
				colors[vertIndex] = quad.color;
				normals[vertIndex] = directionNormals[(int)quad.direction];
				verts[vertIndex++] = quad.v2;
				colors[vertIndex] = quad.color;
				normals[vertIndex] = directionNormals[(int)quad.direction];
				verts[vertIndex++] = quad.v3;
			}

			Mesh mesh = new Mesh
			{
				vertices = verts,
				normals = normals,
				triangles = tris,
				colors32 = colors
			};

			return mesh;
		}

		private static Mesh AmbientOcclusionMesh()
		{
			TerrainGeneratorBase terrainGenerator = chunk.voxelEngineManager.terrainGenerator;
			int index = -Chunk.VOXEL_STEP_X - Chunk.VOXEL_STEP_Y - Chunk.VOXEL_STEP_Z - 1;

			for (int x = -1; x < Chunk.SIZE - 1; x++)
			{
				for (int y = -1; y < Chunk.SIZE - 1; y++)
				{
					for (int z = -1; z < Chunk.SIZE - 1; z++)
					{
						Voxel voxel;
						Voxel left;
						Voxel down;
						Voxel back;

						if (x == -1 || y == -1 || z == -1)
						{
							voxel = GetAdjVoxel(++index, x, y, z);
							left = GetAdjVoxel(index - Chunk.VOXEL_STEP_X, x - 1, y, z);
							down = GetAdjVoxel(index - Chunk.VOXEL_STEP_Y, x, y - 1, z);
							back = GetAdjVoxel(index - Chunk.VOXEL_STEP_Z, x, y, z - 1);
						}
						else
						{
							voxel = chunk.voxelData[++index];
							left = GetAdjVoxelLeft(index, x);
							down = GetAdjVoxelDown(index, y);
							back = GetAdjVoxelBack(index, z);
						}

						if (voxel.IsSolid())
						{
							Color32 color = new Color32();
							bool colorInit = false;

							// Left
							if (!left.IsSolid())
							{
								colorInit = true;
								color = terrainGenerator.DensityColor(voxel);

								Quad q = new Quad(
									new Vector3(x - 0.5f, y - 0.5f, z - 0.5f),
									new Vector3(x - 0.5f, y - 0.5f, z + 0.5f),
									new Vector3(x - 0.5f, y + 0.5f, z + 0.5f),
									new Vector3(x - 0.5f, y + 0.5f, z - 0.5f),
									color, Direction.Left);

								q.i0 = LightLevelX(q.v0, color, y, z, -0.25f);
								q.i1 = LightLevelX(q.v1, color, y, z, -0.25f);
								q.i2 = LightLevelX(q.v2, color, y, z, -0.25f);
								q.i3 = LightLevelX(q.v3, color, y, z, -0.25f);

								quads.Add(q);
							}

							// Down
							if (!down.IsSolid())
							{
								if (!colorInit)
								{
									colorInit = true;
									color = terrainGenerator.DensityColor(voxel);
								}

								Quad q = new Quad(
									new Vector3(x - 0.5f, y - 0.5f, z - 0.5f),
									new Vector3(x + 0.5f, y - 0.5f, z - 0.5f),
									new Vector3(x + 0.5f, y - 0.5f, z + 0.5f),
									new Vector3(x - 0.5f, y - 0.5f, z + 0.5f),
									color, Direction.Down);

								q.i0 = LightLevelY(q.v0, color, x, z, -0.25f);
								q.i1 = LightLevelY(q.v1, color, x, z, -0.25f);
								q.i2 = LightLevelY(q.v2, color, x, z, -0.25f);
								q.i3 = LightLevelY(q.v3, color, x, z, -0.25f);

								quads.Add(q);
							}

							// Back
							if (!back.IsSolid())
							{
								if (!colorInit)
									color = terrainGenerator.DensityColor(voxel);

								Quad q = new Quad(
									new Vector3(x - 0.5f, y - 0.5f, z - 0.5f),
									new Vector3(x - 0.5f, y + 0.5f, z - 0.5f),
									new Vector3(x + 0.5f, y + 0.5f, z - 0.5f),
									new Vector3(x + 0.5f, y - 0.5f, z - 0.5f),
									color, Direction.Back);

								q.i0 = LightLevelZ(q.v0, color, x, y, -0.25f);
								q.i1 = LightLevelZ(q.v1, color, x, y, -0.25f);
								q.i2 = LightLevelZ(q.v2, color, x, y, -0.25f);
								q.i3 = LightLevelZ(q.v3, color, x, y, -0.25f);

								quads.Add(q);
							}
						}
						else // Voxel not solid
						{
							// Left
							if (left.IsSolid())
							{
								Quad q = new Quad(
									new Vector3(x - 0.5f, y + 0.5f, z - 0.5f),
									new Vector3(x - 0.5f, y + 0.5f, z + 0.5f),
									new Vector3(x - 0.5f, y - 0.5f, z + 0.5f),
									new Vector3(x - 0.5f, y - 0.5f, z - 0.5f),
									terrainGenerator.DensityColor(left), Direction.Right);

								q.i0 = LightLevelX(q.v0, q.color, y, z, 0.25f);
								q.i1 = LightLevelX(q.v1, q.color, y, z, 0.25f);
								q.i2 = LightLevelX(q.v2, q.color, y, z, 0.25f);
								q.i3 = LightLevelX(q.v3, q.color, y, z, 0.25f);

								quads.Add(q);
							}

							// Down
							if (down.IsSolid())
							{
								Quad q = new Quad(
									new Vector3(x - 0.5f, y - 0.5f, z + 0.5f),
									new Vector3(x + 0.5f, y - 0.5f, z + 0.5f),
									new Vector3(x + 0.5f, y - 0.5f, z - 0.5f),
									new Vector3(x - 0.5f, y - 0.5f, z - 0.5f),
									terrainGenerator.DensityColor(down), Direction.Up);

								q.i0 = LightLevelY(q.v0, q.color, x, z, 0.25f);
								q.i1 = LightLevelY(q.v1, q.color, x, z, 0.25f);
								q.i2 = LightLevelY(q.v2, q.color, x, z, 0.25f);
								q.i3 = LightLevelY(q.v3, q.color, x, z, 0.25f);

								quads.Add(q);
							}

							// Back
							if (back.IsSolid())
							{
								Quad q = new Quad(
									new Vector3(x + 0.5f, y - 0.5f, z - 0.5f),
									new Vector3(x + 0.5f, y + 0.5f, z - 0.5f),
									new Vector3(x - 0.5f, y + 0.5f, z - 0.5f),
									new Vector3(x - 0.5f, y - 0.5f, z - 0.5f),
									terrainGenerator.DensityColor(back), Direction.Forward);

								q.i0 = LightLevelZ(q.v0, q.color, x, y, 0.25f);
								q.i1 = LightLevelZ(q.v1, q.color, x, y, 0.25f);
								q.i2 = LightLevelZ(q.v2, q.color, x, y, 0.25f);
								q.i3 = LightLevelZ(q.v3, q.color, x, y, 0.25f);

								quads.Add(q);
							}
						}
					}
				}
			}

			if (quads.Count == 0)
				return null;

			Vector3[] verts = new Vector3[quads.Count * 4];
			Vector3[] normals = new Vector3[quads.Count * 4];
			//Vector2[] uvs = new Vector2[quads.Count * 4];
			Color32[] colors = new Color32[quads.Count * 4];
			int[] tris = new int[quads.Count * 6];

			int vertIndex = 0;
			int triIndex = 0;

			foreach (Quad quad in quads)
			{
				if (quad.i0 + quad.i2 < quad.i1 + quad.i3)
				{
					tris[triIndex++] = vertIndex;
					tris[triIndex++] = vertIndex + 1;
					tris[triIndex++] = vertIndex + 2;
					tris[triIndex++] = vertIndex;
					tris[triIndex++] = vertIndex + 2;
					tris[triIndex++] = vertIndex + 3;
				}
				else
				{
					tris[triIndex++] = vertIndex + 1;
					tris[triIndex++] = vertIndex + 2;
					tris[triIndex++] = vertIndex + 3;
					tris[triIndex++] = vertIndex + 1;
					tris[triIndex++] = vertIndex + 3;
					tris[triIndex++] = vertIndex;
				}

				normals[vertIndex] = directionNormals[(int)quad.direction];
				colors[vertIndex] = LightColorAdjust(quad.color, quad.i0);
				verts[vertIndex++] = quad.v0;
				normals[vertIndex] = directionNormals[(int)quad.direction];
				colors[vertIndex] = LightColorAdjust(quad.color, quad.i1);
				verts[vertIndex++] = quad.v1;
				normals[vertIndex] = directionNormals[(int)quad.direction];
				colors[vertIndex] = LightColorAdjust(quad.color, quad.i2);
				verts[vertIndex++] = quad.v2;
				normals[vertIndex] = directionNormals[(int)quad.direction];
				colors[vertIndex] = LightColorAdjust(quad.color, quad.i3);
				verts[vertIndex++] = quad.v3;
			}

			Mesh mesh = new Mesh
			{
				vertices = verts,
				normals = normals,
				triangles = tris,
				colors32 = colors
			};

			return mesh;
		}

		private static Color32 LightColorAdjust(Color32 color, int lightLevel)
		{
			if (lightLevel != 0)
			{
				float lightModifier = 1.0f - lightLevel * Chunk.AMBIENT_OCCLUSION_STRENGTH;

				color.r = (byte)(color.r * lightModifier);
				color.g = (byte)(color.g * lightModifier);
				color.b = (byte)(color.b * lightModifier);
			}

			return color;
		}

		// Ambient light calulator for X normal faces
		private static int LightLevelX(Vector3 vert, Color32 color, int localY, int localZ, float xOffset)
		{
			int lightLevel = 0;
			vert.x += xOffset;

			if (!lightLevels.TryGetValue(vert, out lightLevel))
			{
				int ix = FastRound(vert.x);
				int iy = FastFloor(vert.y);
				int iz = FastFloor(vert.z);
				int sides = 0;
				int corner = 0;

				if (localY == iy)
				{
					if (localZ == iz)
					{
						if (GetAdjVoxel(ix, iy + 1, iz).IsSolid())
							sides++;
						if (GetAdjVoxel(ix, iy, iz + 1).IsSolid())
							sides++;
						if (GetAdjVoxel(ix, iy + 1, iz + 1).IsSolid())
							corner++;
					}
					else
					{
						if (GetAdjVoxel(ix, iy + 1, iz + 1).IsSolid())
							sides++;
						if (GetAdjVoxel(ix, iy, iz).IsSolid())
							sides++;
						if (GetAdjVoxel(ix, iy + 1, iz).IsSolid())
							corner++;
					}
				}
				else
				{
					if (localZ == iz)
					{
						if (GetAdjVoxel(ix, iy, iz).IsSolid())
							sides++;
						if (GetAdjVoxel(ix, iy + 1, iz + 1).IsSolid())
							sides++;
						if (GetAdjVoxel(ix, iy, iz + 1).IsSolid())
							corner++;
					}
					else
					{
						if (GetAdjVoxel(ix, iy, iz + 1).IsSolid())
							sides++;
						if (GetAdjVoxel(ix, iy + 1, iz).IsSolid())
							sides++;
						if (GetAdjVoxel(ix, iy, iz).IsSolid())
							corner++;
					}
				}

				if (sides == 2)
					lightLevel = 3;
				else
					lightLevel = sides + corner;

				lightLevels.Add(vert, lightLevel);
			}
			return lightLevel;
		}

		// Ambient light calulator for Y normal faces
		private static int LightLevelY(Vector3 vert, Color32 color, int localX, int localZ, float yOffset)
		{
			int lightLevel = 0;
			vert.y += yOffset;

			if (!lightLevels.TryGetValue(vert, out lightLevel))
			{
				int ix = FastFloor(vert.x);
				int iy = FastRound(vert.y);
				int iz = FastFloor(vert.z);
				int sides = 0;
				int corner = 0;

				if (localX == ix)
				{
					if (localZ == iz)
					{
						if (GetAdjVoxel(ix + 1, iy, iz).IsSolid())
							sides++;
						if (GetAdjVoxel(ix, iy, iz + 1).IsSolid())
							sides++;
						if (GetAdjVoxel(ix + 1, iy, iz + 1).IsSolid())
							corner++;
					}
					else
					{
						if (GetAdjVoxel(ix + 1, iy, iz + 1).IsSolid())
							sides++;
						if (GetAdjVoxel(ix, iy, iz).IsSolid())
							sides++;
						if (GetAdjVoxel(ix + 1, iy, iz).IsSolid())
							corner++;
					}
				}
				else
				{
					if (localZ == iz)
					{
						if (GetAdjVoxel(ix, iy, iz).IsSolid())
							sides++;
						if (GetAdjVoxel(ix + 1, iy, iz + 1).IsSolid())
							sides++;
						if (GetAdjVoxel(ix, iy, iz + 1).IsSolid())
							corner++;
					}
					else
					{
						if (GetAdjVoxel(ix, iy, iz + 1).IsSolid())
							sides++;
						if (GetAdjVoxel(ix + 1, iy, iz).IsSolid())
							sides++;
						if (GetAdjVoxel(ix, iy, iz).IsSolid())
							corner++;
					}
				}

				if (sides == 2)
					lightLevel = 3;
				else
					lightLevel = sides + corner;

				lightLevels.Add(vert, lightLevel);
			}
			return lightLevel;
		}

		// Ambient light calulator for Z normal faces
		private static int LightLevelZ(Vector3 vert, Color32 color, int localX, int localY, float zOffset)
		{
			int lightLevel = 0;
			vert.z += zOffset;

			if (!lightLevels.TryGetValue(vert, out lightLevel))
			{
				int ix = FastFloor(vert.x);
				int iy = FastFloor(vert.y);
				int iz = FastRound(vert.z);
				int sides = 0;
				int corner = 0;

				if (localX == ix)
				{
					if (localY == iy)
					{
						if (GetAdjVoxel(ix + 1, iy, iz).IsSolid())
							sides++;
						if (GetAdjVoxel(ix, iy + 1, iz).IsSolid())
							sides++;
						if (GetAdjVoxel(ix + 1, iy + 1, iz).IsSolid())
							corner++;
					}
					else
					{
						if (GetAdjVoxel(ix + 1, iy + 1, iz).IsSolid())
							sides++;
						if (GetAdjVoxel(ix, iy, iz).IsSolid())
							sides++;
						if (GetAdjVoxel(ix + 1, iy, iz).IsSolid())
							corner++;
					}
				}
				else
				{
					if (localY == iy)
					{
						if (GetAdjVoxel(ix, iy, iz).IsSolid())
							sides++;
						if (GetAdjVoxel(ix + 1, iy + 1, iz).IsSolid())
							sides++;
						if (GetAdjVoxel(ix, iy + 1, iz).IsSolid())
							corner++;
					}
					else
					{
						if (GetAdjVoxel(ix, iy + 1, iz).IsSolid())
							sides++;
						if (GetAdjVoxel(ix + 1, iy, iz).IsSolid())
							sides++;
						if (GetAdjVoxel(ix, iy, iz).IsSolid())
							corner++;
					}
				}

				if (sides == 2)
					lightLevel = 3;
				else
					lightLevel = sides + corner;

				lightLevels.Add(vert, lightLevel);
			}
			return lightLevel;
		}

		private static int FastFloor(float f) { return f >= 0.0f ? (int)f : (int)f - 1; }
		private static int FastRound(float f) { return (f >= 0.0f) ? (int)(f + 0.5f) : (int)(f - 0.5f); }


		private static Mesh GradientMesh()
		{
			TerrainGeneratorBase terrainGenerator = chunk.voxelEngineManager.terrainGenerator;
			int index = -Chunk.VOXEL_STEP_X - Chunk.VOXEL_STEP_Y - Chunk.VOXEL_STEP_Z - 1;

			for (int x = -1; x < Chunk.SIZE - 1; x++)
			{
				for (int y = -1; y < Chunk.SIZE - 1; y++)
				{
					for (int z = -1; z < Chunk.SIZE - 1; z++)
					{
						Voxel voxel, left, down, back;
						int adjIndex = -1;

						if (x == -1 || y == -1 || z == -1)
						{
							voxel = GetAdjVoxel(++index, x, y, z);
							left = GetAdjVoxel(index - Chunk.VOXEL_STEP_X, x - 1, y, z);
							down = GetAdjVoxel(index - Chunk.VOXEL_STEP_Y, x, y - 1, z);
							back = GetAdjVoxel(index - Chunk.VOXEL_STEP_Z, x, y, z - 1);
						}
						else
						{
							voxel = chunk.voxelData[++index];
							left = GetAdjVoxelLeft(index, x);
							down = GetAdjVoxelDown(index, y);
							back = GetAdjVoxelBack(index, z);
						}

						if (voxel.IsSolid())
						{
							Color32 color = new Color32();
							bool colorInit = false;

							// Left
							if (!left.IsSolid())
							{
								colorInit = true;
								color = terrainGenerator.DensityColor(voxel);

								if (adjIndex == -1)
									adjIndex = AdjIndex(x, y, z);

								Quad q = new Quad(
									adjIndex,
									adjIndex + ADJ_VOXEL_STEP_Z,
									adjIndex + ADJ_VOXEL_STEP_Y + ADJ_VOXEL_STEP_Z,
									adjIndex + ADJ_VOXEL_STEP_Y,
									color);

								if (!gradientVerts.ContainsKey(q.i0))
									gradientVerts[q.i0] = new Vector3(x - 0.5f, y - 0.5f, z - 0.5f) + VoxelGradient(x - 1, y - 1, z - 1);
								if (!gradientVerts.ContainsKey(q.i1))
									gradientVerts[q.i1] = new Vector3(x - 0.5f, y - 0.5f, z + 0.5f) + VoxelGradient(x - 1, y - 1, z);
								if (!gradientVerts.ContainsKey(q.i2))
									gradientVerts[q.i2] = new Vector3(x - 0.5f, y + 0.5f, z + 0.5f) + VoxelGradient(x - 1, y, z);
								if (!gradientVerts.ContainsKey(q.i3))
									gradientVerts[q.i3] = new Vector3(x - 0.5f, y + 0.5f, z - 0.5f) + VoxelGradient(x - 1, y, z - 1);

								quads.Add(q);
							}

							// Down
							if (!down.IsSolid())
							{
								if (!colorInit)
								{
									colorInit = true;
									color = terrainGenerator.DensityColor(voxel);
								}

								if (adjIndex == -1)
									adjIndex = AdjIndex(x, y, z);

								Quad q = new Quad(
									adjIndex,
									adjIndex + ADJ_VOXEL_STEP_X,
									adjIndex + ADJ_VOXEL_STEP_X + ADJ_VOXEL_STEP_Z,
									adjIndex + ADJ_VOXEL_STEP_Z,
									color);

								if (!gradientVerts.ContainsKey(q.i0))
									gradientVerts[q.i0] = new Vector3(x - 0.5f, y - 0.5f, z - 0.5f) + VoxelGradient(x - 1, y - 1, z - 1);
								if (!gradientVerts.ContainsKey(q.i1))
									gradientVerts[q.i1] = new Vector3(x + 0.5f, y - 0.5f, z - 0.5f) + VoxelGradient(x, y - 1, z - 1);
								if (!gradientVerts.ContainsKey(q.i2))
									gradientVerts[q.i2] = new Vector3(x + 0.5f, y - 0.5f, z + 0.5f) + VoxelGradient(x, y - 1, z);
								if (!gradientVerts.ContainsKey(q.i3))
									gradientVerts[q.i3] = new Vector3(x - 0.5f, y - 0.5f, z + 0.5f) + VoxelGradient(x - 1, y - 1, z);

								quads.Add(q);
							}

							// Back
							if (!back.IsSolid())
							{
								if (!colorInit)
									color = terrainGenerator.DensityColor(voxel);

								if (adjIndex == -1)
									adjIndex = AdjIndex(x, y, z);

								Quad q = new Quad(
									adjIndex,
									adjIndex + ADJ_VOXEL_STEP_Y,
									adjIndex + ADJ_VOXEL_STEP_X + ADJ_VOXEL_STEP_Y,
									adjIndex + ADJ_VOXEL_STEP_X,
									color);

								if (!gradientVerts.ContainsKey(q.i0))
									gradientVerts[q.i0] = new Vector3(x - 0.5f, y - 0.5f, z - 0.5f) + VoxelGradient(x - 1, y - 1, z - 1);
								if (!gradientVerts.ContainsKey(q.i1))
									gradientVerts[q.i1] = new Vector3(x - 0.5f, y + 0.5f, z - 0.5f) + VoxelGradient(x - 1, y, z - 1);
								if (!gradientVerts.ContainsKey(q.i2))
									gradientVerts[q.i2] = new Vector3(x + 0.5f, y + 0.5f, z - 0.5f) + VoxelGradient(x, y, z - 1);
								if (!gradientVerts.ContainsKey(q.i3))
									gradientVerts[q.i3] = new Vector3(x + 0.5f, y - 0.5f, z - 0.5f) + VoxelGradient(x, y - 1, z - 1);

								quads.Add(q);
							}
						}
						else // Voxel not solid
						{
							// Left
							if (left.IsSolid())
							{
								if (adjIndex == -1)
									adjIndex = AdjIndex(x, y, z);

								Quad q = new Quad(
									adjIndex + ADJ_VOXEL_STEP_Y,
									adjIndex + ADJ_VOXEL_STEP_Y + ADJ_VOXEL_STEP_Z,
									adjIndex + ADJ_VOXEL_STEP_Z,
									adjIndex,
									terrainGenerator.DensityColor(left));

								if (!gradientVerts.ContainsKey(q.i0))
									gradientVerts[q.i0] = new Vector3(x - 0.5f, y + 0.5f, z - 0.5f) + VoxelGradient(x - 1, y, z - 1);
								if (!gradientVerts.ContainsKey(q.i1))
									gradientVerts[q.i1] = new Vector3(x - 0.5f, y + 0.5f, z + 0.5f) + VoxelGradient(x - 1, y, z);
								if (!gradientVerts.ContainsKey(q.i2))
									gradientVerts[q.i2] = new Vector3(x - 0.5f, y - 0.5f, z + 0.5f) + VoxelGradient(x - 1, y - 1, z);
								if (!gradientVerts.ContainsKey(q.i3))
									gradientVerts[q.i3] = new Vector3(x - 0.5f, y - 0.5f, z - 0.5f) + VoxelGradient(x - 1, y - 1, z - 1);

								quads.Add(q);
							}

							// Down
							if (down.IsSolid())
							{
								if (adjIndex == -1)
									adjIndex = AdjIndex(x, y, z);

								Quad q = new Quad(
									adjIndex + ADJ_VOXEL_STEP_Z,
									adjIndex + ADJ_VOXEL_STEP_X + ADJ_VOXEL_STEP_Z,
									adjIndex + ADJ_VOXEL_STEP_X,
									adjIndex,
									terrainGenerator.DensityColor(down));

								if (!gradientVerts.ContainsKey(q.i0))
									gradientVerts[q.i0] = new Vector3(x - 0.5f, y - 0.5f, z + 0.5f) + VoxelGradient(x - 1, y - 1, z);
								if (!gradientVerts.ContainsKey(q.i1))
									gradientVerts[q.i1] = new Vector3(x + 0.5f, y - 0.5f, z + 0.5f) + VoxelGradient(x, y - 1, z);
								if (!gradientVerts.ContainsKey(q.i2))
									gradientVerts[q.i2] = new Vector3(x + 0.5f, y - 0.5f, z - 0.5f) + VoxelGradient(x, y - 1, z - 1);
								if (!gradientVerts.ContainsKey(q.i3))
									gradientVerts[q.i3] = new Vector3(x - 0.5f, y - 0.5f, z - 0.5f) + VoxelGradient(x - 1, y - 1, z - 1);

								quads.Add(q);
							}

							// Back
							if (back.IsSolid())
							{
								if (adjIndex == -1)
									adjIndex = AdjIndex(x, y, z);

								Quad q = new Quad(
									adjIndex + ADJ_VOXEL_STEP_X,
									adjIndex + ADJ_VOXEL_STEP_X + ADJ_VOXEL_STEP_Y,
									adjIndex + ADJ_VOXEL_STEP_Y,
									adjIndex,
									terrainGenerator.DensityColor(back));

								if (!gradientVerts.ContainsKey(q.i0))
									gradientVerts[q.i0] = new Vector3(x + 0.5f, y - 0.5f, z - 0.5f) + VoxelGradient(x, y - 1, z - 1);
								if (!gradientVerts.ContainsKey(q.i1))
									gradientVerts[q.i1] = new Vector3(x + 0.5f, y + 0.5f, z - 0.5f) + VoxelGradient(x, y, z - 1);
								if (!gradientVerts.ContainsKey(q.i2))
									gradientVerts[q.i2] = new Vector3(x - 0.5f, y + 0.5f, z - 0.5f) + VoxelGradient(x - 1, y, z - 1);
								if (!gradientVerts.ContainsKey(q.i3))
									gradientVerts[q.i3] = new Vector3(x - 0.5f, y - 0.5f, z - 0.5f) + VoxelGradient(x - 1, y - 1, z - 1);

								quads.Add(q);
							}
						}
					}
				}
			}

			if (quads.Count == 0)
				return null;

			Vector3[] verts = new Vector3[quads.Count * 4];
			//Vector2[] uvs = new Vector2[quads.Count * 4];
			Color32[] colors = new Color32[quads.Count * 4];
			int[] tris = new int[quads.Count * 6];

			int vertIndex = 0;
			int triIndex = 0;

			foreach (Quad quad in quads)
			{
				colors[vertIndex] = quad.color;
				verts[vertIndex++] = gradientVerts[quad.i0];
				colors[vertIndex] = quad.color;
				verts[vertIndex++] = gradientVerts[quad.i1];
				colors[vertIndex] = quad.color;
				verts[vertIndex++] = gradientVerts[quad.i2];
				colors[vertIndex] = quad.color;
				verts[vertIndex++] = gradientVerts[quad.i3];

				if ((verts[vertIndex - 4] - verts[vertIndex - 2]).sqrMagnitude <
				    (verts[vertIndex - 3] - verts[vertIndex - 1]).sqrMagnitude)
				{
					tris[triIndex++] = vertIndex - 4;
					tris[triIndex++] = vertIndex - 3;
					tris[triIndex++] = vertIndex - 2;
					tris[triIndex++] = vertIndex - 4;
					tris[triIndex++] = vertIndex - 2;
					tris[triIndex++] = vertIndex - 1;
				}
				else
				{
					tris[triIndex++] = vertIndex - 3;
					tris[triIndex++] = vertIndex - 2;
					tris[triIndex++] = vertIndex - 1;
					tris[triIndex++] = vertIndex - 3;
					tris[triIndex++] = vertIndex - 1;
					tris[triIndex++] = vertIndex - 4;
				}
			}

			Mesh mesh = new Mesh
			{
				vertices = verts,
				triangles = tris,
				colors32 = colors
			};

			mesh.RecalculateNormals();

			return mesh;
		}

		// Calculate difference vector from the isosurface
		private static Vector3 VoxelGradient(int localX, int localY, int localZ)
		{
			int index = localX * Chunk.VOXEL_STEP_X + localY * Chunk.VOXEL_STEP_Y + localZ * Chunk.VOXEL_STEP_Z;

			return Gradient(
				GetAdjVoxel(index, localX, localY, localZ).density,
				GetAdjVoxel(index + Chunk.VOXEL_STEP_X, localX + 1, localY, localZ).density,
				GetAdjVoxel(index + Chunk.VOXEL_STEP_Y, localX, localY + 1, localZ).density,
				GetAdjVoxel(index + Chunk.VOXEL_STEP_X + Chunk.VOXEL_STEP_Y, localX + 1, localY + 1, localZ).density,
				GetAdjVoxel(index += Chunk.VOXEL_STEP_Z,localX, localY, localZ + 1).density,
				GetAdjVoxel(index + Chunk.VOXEL_STEP_X, localX + 1, localY, localZ + 1).density,
				GetAdjVoxel(index + Chunk.VOXEL_STEP_Y, localX, localY + 1, localZ + 1).density,
				GetAdjVoxel(index + Chunk.VOXEL_STEP_X + Chunk.VOXEL_STEP_Y, localX + 1, localY + 1, localZ + 1).density);
		}

		// Get gradient of 2x2x2 set of voxels
		private static Vector3 Gradient(float a, float b, float c, float d, float e, float f, float g, float h)
		{
			float v = (a + b + c + d + e + f + g + h) * -0.125f;

			Vector3 v3 = new Vector3(
				(-a + b - c + d - e + f - g + h),
				(-a - b + c + d - e - f + g + h),
				(-a - b - c - d + e + f + g + h));

			v3 *= 0.25f;
			v /= v3.sqrMagnitude;
			v3 *= v;

			return v3;
		}

		private static int AdjIndex(int localX, int localY, int localZ)
		{
			return (localZ + 1) | ((localY + 1) << ADJ_BIT_SIZE) | ((localX + 1) << (ADJ_BIT_SIZE * 2));
		}

		private static Voxel GetAdjVoxel(int voxelIndex, int localX, int localY, int localZ)
		{
			int adjIndex = -2;

			if (localX < 0)
			{
				adjIndex += 2;
				voxelIndex += Chunk.VOXEL_STEP_CHUNK_X;
			}
			if (localY < 0)
			{
				adjIndex += 3;
				voxelIndex += Chunk.VOXEL_STEP_CHUNK_Y;
			}
			if (localZ < 0)
			{
				adjIndex += 4;
				voxelIndex += Chunk.VOXEL_STEP_CHUNK_Z;
			}

			if (adjIndex == -2)
				return chunk.voxelData[voxelIndex];

			return chunk.adjChunks[Math.Min(adjIndex, 6)].voxelData[voxelIndex];
		}

		private static Voxel GetAdjVoxel(int localX, int localY, int localZ)
		{
			int adjIndex = -2;

			if (localX < 0)
			{
				adjIndex += 2;
				localX += Chunk.SIZE;
			}
			if (localY < 0)
			{
				adjIndex += 3;
				localY += Chunk.SIZE;
			}
			if (localZ < 0)
			{
				adjIndex += 4;
				localZ += Chunk.SIZE;
			}

			if (adjIndex == -2)
				return chunk.GetVoxelUnsafe(localX, localY, localZ);

			return chunk.adjChunks[Math.Min(adjIndex, 6)].GetVoxelUnsafe(localX, localY, localZ);
		}

		private static Voxel GetAdjVoxelLeft(int voxelIndex, int localX)
		{
			voxelIndex -= Chunk.VOXEL_STEP_X;

			if (localX > 0)
				return chunk.voxelData[voxelIndex];

			voxelIndex += Chunk.VOXEL_STEP_CHUNK_X;
			return chunk.adjChunks[(int)Chunk.AdjDirection.Left].voxelData[voxelIndex];
		}

		private static Voxel GetAdjVoxelDown(int voxelIndex, int localY)
		{
			voxelIndex -= Chunk.VOXEL_STEP_Y;

			if (localY > 0)
				return chunk.voxelData[voxelIndex];

			voxelIndex += Chunk.VOXEL_STEP_CHUNK_Y;
			return chunk.adjChunks[(int)Chunk.AdjDirection.Down].voxelData[voxelIndex];
		}

		private static Voxel GetAdjVoxelBack(int voxelIndex, int localZ)
		{
			voxelIndex -= Chunk.VOXEL_STEP_Z;

			if (localZ > 0)
				return chunk.voxelData[voxelIndex];

			voxelIndex += Chunk.VOXEL_STEP_CHUNK_Z;
			return chunk.adjChunks[(int)Chunk.AdjDirection.Back].voxelData[voxelIndex];
		}

		public struct Quad
		{
			public Vector3 v0, v1, v2, v3;
			public Color32 color;
			public Direction direction;
			public int i0, i1, i2, i3;

			public Quad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Color32 color, Direction direction)
			{
				this.v0 = v0;
				this.v1 = v1;
				this.v2 = v2;
				this.v3 = v3;
				this.color = color;
				this.direction = direction;
				i0 = i1 = i2 = i3 = 0;
			}

			public Quad(int i0, int i1, int i2, int i3, Color32 color)
			{
				this.i0 = i0;
				this.i1 = i1;
				this.i2 = i2;
				this.i3 = i3;
				this.color = color;
				direction = Direction.Left;
				v0 = v1 = v2 = v3 = Vector3.zero;
			}
		}
	}
}