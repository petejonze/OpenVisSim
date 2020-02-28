using UnityEngine;

namespace VoxelEngine
{
	public class TerrainGeneratorSIMD_FloatingIslands : TerrainGeneratorSIMD
	{
		public float terrainScale = 20f;

		public Color32 grassColor = new Color32(112, 150, 48, 255);
		public Color32 dirtColor = new Color32(97, 75, 66, 255);
		public Color32 stoneColor = new Color32(150, 150, 150, 255);

		public override void Awake()
		{
			SetInterpBitStep(2);
			SetNoiseArraySize(2);
		}

		public override void GenerateChunk(Chunk chunk)
		{
			Voxel[] voxelData = chunk.voxelData;

			int index = 0;

			float[] islandNoise = GetInterpNoise(0, chunk.chunkPos);
			float[] terrainNoise = GetInterpNoise(1, chunk.chunkPos);

			for (int x = 0; x < interpSize; x++)
			{
				for (int y = 0; y < interpSize; y++)
				{
					for (int z = 0; z < interpSize; z++)
					{
						terrainNoise[index] = Mathf.Abs(terrainNoise[index] * terrainScale) + (Mathf.Abs(islandNoise[index]) * islandNoise[index] + 0.2f) * 20.0f;

						index++;
					}
				}
			}

			index = 0;

			for (int x = 0; x < Chunk.SIZE; x++)
			{
				for (int y = 0; y < Chunk.SIZE; y++)
				{
					for (int z = 0; z < Chunk.SIZE; z++)
					{
						ChunkFillUpdate(chunk, voxelData[index++] = new Voxel(VoxelInterpLookup(x, y, z, terrainNoise)));
					}
				}
			}
		}

		public override Color32 DensityColor(Voxel voxel)
		{
			if (voxel.density < 2f)
				return Color32.Lerp(grassColor, dirtColor, voxel.density * 0.5f);

			if (voxel.density < 6f)
			{
				float lerp = (voxel.density - 2f) * 0.25f;
				return Color32.Lerp(dirtColor, stoneColor, lerp * lerp);
			}

			return stoneColor;
		}
	}
}