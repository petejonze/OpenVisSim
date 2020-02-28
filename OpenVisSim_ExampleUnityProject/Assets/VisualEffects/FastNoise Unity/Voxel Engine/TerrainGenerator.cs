namespace VoxelEngine
{
	public abstract class TerrainGenerator : TerrainGeneratorBase
	{
		public FastNoiseUnity[] fastNoiseUnity = new FastNoiseUnity[1];

		protected void SetNoiseArraySize(int size)
		{
			System.Array.Resize(ref fastNoiseUnity, size);
		}

		protected FastNoise GetFastNoise(int noiseArrayIndex)
		{
			return fastNoiseUnity[noiseArrayIndex].fastNoise;
		}
	}
}
