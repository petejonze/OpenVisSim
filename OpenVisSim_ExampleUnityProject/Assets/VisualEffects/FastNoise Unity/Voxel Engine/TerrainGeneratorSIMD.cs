namespace VoxelEngine
{
	public abstract class TerrainGeneratorSIMD : TerrainGeneratorBase
	{
		public FastNoiseSIMDUnity[] fastNoiseSIMDUnity = new FastNoiseSIMDUnity[1];

		protected void SetNoiseArraySize(int size)
		{
			System.Array.Resize(ref fastNoiseSIMDUnity, size);
		}

		protected float[] GetInterpNoise(int noiseArrayIndex, Vector3i chunkPos)
		{
			int offsetShift = Chunk.BIT_SIZE - interpBitStep;

			return fastNoiseSIMDUnity[noiseArrayIndex].fastNoiseSIMD.GetNoiseSet(chunkPos.x << offsetShift,
				chunkPos.y << offsetShift, chunkPos.z << offsetShift, interpSize, interpSize, interpSize, 1 << interpBitStep);
		}
	}
}