using UnityEngine;

[AddComponentMenu("FastNoise/FastNoise SIMD Unity", 2)]

// FastNoise SIMD wrapper for Unity Editor
public class FastNoiseSIMDUnity : MonoBehaviour
{
	// Use this to access FastNoise SIMD functions
	public FastNoiseSIMD fastNoiseSIMD = new FastNoiseSIMD();

	public string noiseName = "Default Noise";

	public int seed = 1337;
	public float frequency = 0.01f;
	public FastNoiseSIMD.NoiseType noiseType = FastNoiseSIMD.NoiseType.Simplex;
	public Vector3 axisScales = Vector3.one;

	public int octaves = 3;
	public float lacunarity = 2.0f;
	public float gain = 0.5f;
	public FastNoiseSIMD.FractalType fractalType = FastNoiseSIMD.FractalType.FBM;

	public FastNoiseSIMD.CellularDistanceFunction cellularDistanceFunction = FastNoiseSIMD.CellularDistanceFunction.Euclidean;
	public FastNoiseSIMD.CellularReturnType cellularReturnType = FastNoiseSIMD.CellularReturnType.Distance;
	public FastNoiseSIMD.NoiseType cellularNoiseLookupType = FastNoiseSIMD.NoiseType.Simplex;
	public float cellularNoiseLookupFrequency = 0.2f;

	public FastNoiseSIMD.PerturbType perturbType = FastNoiseSIMD.PerturbType.None;
	public float perturbAmp = 1.0f;
	public float perturbFrequency = 0.5f;
	 
	public int perturbOctaves = 3;
	public float perturbLacunarity = 2.0f;
	public float perturbGain = 0.5f;

#if UNITY_EDITOR
	public bool generalSettingsFold = true;
	public bool fractalSettingsFold = false;
	public bool cellularSettingsFold = false;
	public bool perturbSettingsFold = false;
#endif

	void Awake()
	{
		SaveSettings();
	}

	public void SaveSettings()
	{
		fastNoiseSIMD.SetSeed(seed);
		fastNoiseSIMD.SetFrequency(frequency);
		fastNoiseSIMD.SetNoiseType(noiseType);
		fastNoiseSIMD.SetAxisScales(axisScales.x, axisScales.y, axisScales.z);
		
		fastNoiseSIMD.SetFractalOctaves(octaves);
		fastNoiseSIMD.SetFractalLacunarity(lacunarity);
		fastNoiseSIMD.SetFractalGain(gain);
		fastNoiseSIMD.SetFractalType(fractalType);
		
		fastNoiseSIMD.SetCellularDistanceFunction(cellularDistanceFunction);
		fastNoiseSIMD.SetCellularReturnType(cellularReturnType);
		fastNoiseSIMD.SetCellularNoiseLookupType(cellularNoiseLookupType);
		fastNoiseSIMD.SetCellularNoiseLookupFrequency(cellularNoiseLookupFrequency);

		fastNoiseSIMD.SetPerturbType(perturbType);
		fastNoiseSIMD.SetPerturbFrequency(perturbFrequency);
		fastNoiseSIMD.SetPerturbAmp(perturbAmp);

		fastNoiseSIMD.SetPerturbFractalOctaves(perturbOctaves);
		fastNoiseSIMD.SetPerturbFractalLacunarity(perturbLacunarity);
		fastNoiseSIMD.SetPerturbFractalGain(perturbGain);

	}
}
