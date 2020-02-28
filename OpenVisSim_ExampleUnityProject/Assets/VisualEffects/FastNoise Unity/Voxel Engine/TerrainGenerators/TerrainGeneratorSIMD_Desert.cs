using UnityEngine;

namespace VoxelEngine
{
	public class TerrainGeneratorSIMD_Desert : TerrainGeneratorSIMD
	{
		public float terrainScale = 20f;
		public float canyonMaxHeight = 2f;
		public float canyonGradient = 3f;

		public Color32 sandColor = new Color32(240, 190, 2, 255);
		public Color32 stoneColor = new Color32(120, 120, 80, 255);

		public override void Awake()
		{
			SetInterpBitStep(1);
			SetNoiseArraySize(2);

			minHeight = -terrainScale;
			maxHeight = terrainScale*(canyonMaxHeight + 1f);
		}

		public override void GenerateChunk(Chunk chunk)
		{
			Voxel[] voxelData = chunk.voxelData;

			int yOffset = chunk.chunkPos.y << Chunk.BIT_SIZE;
			int index = 0;

			float[] interpLookup = GetInterpNoise(0, chunk.chunkPos);
			float[] canyonNoise = GetInterpNoise(1, chunk.chunkPos);

			for (int x = 0; x < interpSize; x++)
			{
				for (int y = 0; y < interpSize; y++)
				{
					float yf = (y << interpBitStep) + yOffset;

					for (int z = 0; z < interpSize; z++)
					{
						canyonNoise[index] += 0.4f;

						interpLookup[index] = Mathf.Min(interpLookup[index] + canyonMaxHeight,
							Mathf.Max(interpLookup[index], canyonGradient*canyonNoise[index]*Mathf.Abs(canyonNoise[index])));
						interpLookup[index] *= terrainScale;
						interpLookup[index] -= yf;

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
						ChunkFillUpdate(chunk, voxelData[index++] = new Voxel(VoxelInterpLookup(x, y, z, interpLookup)));
					}
				}
			}
		}

		public override Color32 DensityColor(Voxel voxel)
		{
			if (voxel.density < 3.33f)
				return Color32.Lerp(sandColor, stoneColor, voxel.density *0.3f);

			return stoneColor;
		}
	}
}