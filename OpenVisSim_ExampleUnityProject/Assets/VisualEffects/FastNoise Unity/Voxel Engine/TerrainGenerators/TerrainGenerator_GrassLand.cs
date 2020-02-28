using UnityEngine;

namespace VoxelEngine
{
	public class TerrainGenerator_GrassLand : TerrainGenerator
	{
		public float terrainScale = 20f;
		public Color32 grassColor = new Color32(112, 150, 48, 255);
		public Color32 dirtColor = new Color32(97, 75, 66, 255);
		public Color32 stoneColor = new Color32(150, 150, 150, 255);

		public override void Awake()
		{
			SetNoiseArraySize(2);
			SetInterpBitStep(2);

			minHeight = -terrainScale - fastNoiseUnity[1].positionWarpAmp;
			maxHeight = terrainScale + fastNoiseUnity[1].positionWarpAmp;
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
				for (int y = 0; y < interpSize; y++)
				{
					for (int z = 0; z < interpSize; z++)
					{
						float xf = (x << interpBitStep) + xOffset;
						float yf = (y << interpBitStep) + yOffset;
						float zf = (z << interpBitStep) + zOffset;

						GetFastNoise(1).GradientPerturb(ref xf, ref yf, ref zf);

						float voxel = GetFastNoise(0).GetNoise(xf, yf, zf);
						voxel *= terrainScale;
						voxel -= yf;

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
				return Color32.Lerp(grassColor, dirtColor, voxel.density *0.2f);

			if (voxel.density < 15f)
			{
				float lerp = (voxel.density - 5f)*0.1f;
				return Color32.Lerp(dirtColor, stoneColor, lerp*lerp);
			}

			return stoneColor;
		}
	}
}