using UnityEngine;

[AddComponentMenu("FastNoise/FastNoise Unity", 1)]

// FastNoise wrapper for Unity Editor
public class FastNoiseUnity : MonoBehaviour
{
	// Use this to access FastNoise functions
	public FastNoise fastNoise = new FastNoise();

	public string noiseName = "Default Noise";

	public int seed = 1337;
	public float frequency = 0.01f;
	public FastNoise.Interp interp = FastNoise.Interp.Quintic;
	public FastNoise.NoiseType noiseType = FastNoise.NoiseType.Simplex;
	
	public int octaves = 3;
	public float lacunarity = 2.0f;
	public float gain = 0.5f;
	public FastNoise.FractalType fractalType = FastNoise.FractalType.FBM;
	
	public FastNoise.CellularDistanceFunction cellularDistanceFunction = FastNoise.CellularDistanceFunction.Euclidean;
	public FastNoise.CellularReturnType cellularReturnType = FastNoise.CellularReturnType.CellValue;
	public FastNoiseUnity cellularNoiseLookup = null;
	
	public float positionWarpAmp = 1.0f;

#if UNITY_EDITOR
	public bool generalSettingsFold = true;
	public bool fractalSettingsFold = false;
	public bool cellularSettingsFold = false;
	public bool positionWarpSettingsFold = false;
#endif

	void Awake()
	{
		SaveSettings();
	}

	public void SaveSettings()
	{
		fastNoise.SetSeed(seed);
		fastNoise.SetFrequency(frequency);
		fastNoise.SetInterp(interp);
		fastNoise.SetNoiseType(noiseType);

		fastNoise.SetFractalOctaves(octaves);
		fastNoise.SetFractalLacunarity(lacunarity);
		fastNoise.SetFractalGain(gain);
		fastNoise.SetFractalType(fractalType);

		fastNoise.SetCellularDistanceFunction(cellularDistanceFunction);
		fastNoise.SetCellularReturnType(cellularReturnType);

		if (cellularNoiseLookup)
			fastNoise.SetCellularNoiseLookup(cellularNoiseLookup.fastNoise);

		fastNoise.SetGradientPerturbAmp(positionWarpAmp);
	}
}
