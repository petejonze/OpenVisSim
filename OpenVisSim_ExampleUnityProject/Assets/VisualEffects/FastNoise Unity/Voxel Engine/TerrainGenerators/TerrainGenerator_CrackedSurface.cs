using UnityEngine;

namespace VoxelEngine
{
	public class TerrainGenerator_CrackedSurface : TerrainGenerator
	{
		public float terrainScale = 30f;
		public Color32 surfaceColor = new Color32(130, 130, 130, 255);
		public Color32 coreColor = new Color32(80, 0, 80, 255);
		public Color32 lavaColor = new Color32(243, 147, 0, 255);

		public override void Awake()
		{
			SetNoiseArraySize(1);
			SetInterpBitStep(2);

			minHeight = -terrainScale;
			maxHeight = terrainScale;
		}

		public override void GenerateChunk(Chunk chunk)
		{
			float[] interpLookup = new float[interpSize*interpSize*interpSize];
			Voxel[] voxelData = chunk.voxelData;

			int xOffset = chunk.chunkPos.x << Chunk.BIT_SIZE;
			int yOffset = chunk.chunkPos.y << Chunk.BIT_SIZE;
			int zOffset = chunk.chunkPos.z << Chunk.BIT_SIZE;
			int index = 0;

			for (int x = 0; x < interpSize; x++)
			{
				float xf = (x << interpBitStep) + xOffset;

				for (int y = 0; y < interpSize; y++)
				{
					float yf = (y << interpBitStep) + yOffset;

					for (int z = 0; z < interpSize; z++)
					{
						float zf = (z << interpBitStep) + zOffset;

						float voxel;

						if (yf <= terrainScale*-0.6f)
							voxel = 1000000f;
						else
						{
							if (yf < 0)
							{
								voxel = GetFastNoise(0).GetNoise(xf, 0, zf);
								voxel *= Mathf.Abs(voxel)*1.5f;
							}
							else
								voxel = GetFastNoise(0).GetNoise(xf, yf, zf);

							voxel *= terrainScale;
							voxel -= Mathf.Abs(yf);
						}

						interpLookup[index++] = voxel;
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
			if (voxel.density < 5f)
				return Color32.Lerp(surfaceColor, coreColor, voxel.density *0.2f);

			if (voxel.density >= 8f)
				return lavaColor;

			return coreColor;
		}
	}
}