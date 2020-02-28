using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FastNoiseSIMDUnity))]
public class FastNoiseSIMDUnityEditor : Editor
{
	public override void OnInspectorGUI()
	{
		FastNoiseSIMDUnity fastNoiseSIMDUnity = ((FastNoiseSIMDUnity)target);
		FastNoiseSIMD fastNoiseSIMD = fastNoiseSIMDUnity.fastNoiseSIMD;

		fastNoiseSIMDUnity.noiseName = EditorGUILayout.TextField("Name", fastNoiseSIMDUnity.noiseName);

		fastNoiseSIMDUnity.generalSettingsFold = EditorGUILayout.Foldout(fastNoiseSIMDUnity.generalSettingsFold,
			"General Settings");

		if (fastNoiseSIMDUnity.generalSettingsFold)
		{
			fastNoiseSIMD.SetNoiseType(
				fastNoiseSIMDUnity.noiseType =
					(FastNoiseSIMD.NoiseType)EditorGUILayout.EnumPopup("Noise Type", fastNoiseSIMDUnity.noiseType));
			fastNoiseSIMD.SetSeed(fastNoiseSIMDUnity.seed = EditorGUILayout.IntField("Seed", fastNoiseSIMDUnity.seed));
			fastNoiseSIMD.SetFrequency(
				fastNoiseSIMDUnity.frequency = EditorGUILayout.FloatField("Frequency", fastNoiseSIMDUnity.frequency));

			fastNoiseSIMDUnity.axisScales = EditorGUILayout.Vector3Field("Axis Scales", fastNoiseSIMDUnity.axisScales);
			fastNoiseSIMD.SetAxisScales(fastNoiseSIMDUnity.axisScales.x, fastNoiseSIMDUnity.axisScales.y,
				fastNoiseSIMDUnity.axisScales.z);
		}

		fastNoiseSIMDUnity.fractalSettingsFold = EditorGUILayout.Foldout(fastNoiseSIMDUnity.fractalSettingsFold,
			"Fractal Settings");

		if (fastNoiseSIMDUnity.fractalSettingsFold)
		{
			fastNoiseSIMD.SetFractalType(
				fastNoiseSIMDUnity.fractalType =
					(FastNoiseSIMD.FractalType)EditorGUILayout.EnumPopup("Fractal Type", fastNoiseSIMDUnity.fractalType));
			fastNoiseSIMD.SetFractalOctaves(
				fastNoiseSIMDUnity.octaves = EditorGUILayout.IntSlider("Octaves", fastNoiseSIMDUnity.octaves, 2, 9));
			fastNoiseSIMD.SetFractalLacunarity(
				fastNoiseSIMDUnity.lacunarity = EditorGUILayout.FloatField("Lacunarity", fastNoiseSIMDUnity.lacunarity));
			fastNoiseSIMD.SetFractalGain(fastNoiseSIMDUnity.gain = EditorGUILayout.FloatField("Gain", fastNoiseSIMDUnity.gain));
		}

		fastNoiseSIMDUnity.cellularSettingsFold = EditorGUILayout.Foldout(fastNoiseSIMDUnity.cellularSettingsFold,
			"Cellular Settings");

		if (fastNoiseSIMDUnity.cellularSettingsFold)
		{
			fastNoiseSIMD.SetCellularReturnType(
				fastNoiseSIMDUnity.cellularReturnType =
					(FastNoiseSIMD.CellularReturnType)EditorGUILayout.EnumPopup("Return Type", fastNoiseSIMDUnity.cellularReturnType));
			fastNoiseSIMD.SetCellularDistanceFunction(
				fastNoiseSIMDUnity.cellularDistanceFunction =
					(FastNoiseSIMD.CellularDistanceFunction)
						EditorGUILayout.EnumPopup("Distance Function", fastNoiseSIMDUnity.cellularDistanceFunction));
			fastNoiseSIMD.SetCellularNoiseLookupType(
				fastNoiseSIMDUnity.cellularNoiseLookupType =
					(FastNoiseSIMD.NoiseType)EditorGUILayout.EnumPopup("Noise Lookup Type", fastNoiseSIMDUnity.cellularNoiseLookupType));
			fastNoiseSIMD.SetCellularNoiseLookupFrequency(fastNoiseSIMDUnity.cellularNoiseLookupFrequency = EditorGUILayout.FloatField("Noise Lookup Frequency", fastNoiseSIMDUnity.cellularNoiseLookupFrequency));
		}

		fastNoiseSIMDUnity.perturbSettingsFold = EditorGUILayout.Foldout(fastNoiseSIMDUnity.perturbSettingsFold,
			"Perturb Settings");

		if (fastNoiseSIMDUnity.perturbSettingsFold)
		{
			fastNoiseSIMD.SetPerturbType(
				fastNoiseSIMDUnity.perturbType =
					(FastNoiseSIMD.PerturbType)EditorGUILayout.EnumPopup("Perturb Type", fastNoiseSIMDUnity.perturbType));
			fastNoiseSIMD.SetPerturbAmp(fastNoiseSIMDUnity.perturbAmp = EditorGUILayout.FloatField("Amp", fastNoiseSIMDUnity.perturbAmp));
			fastNoiseSIMD.SetPerturbFrequency(fastNoiseSIMDUnity.perturbFrequency = EditorGUILayout.FloatField("Frequency", fastNoiseSIMDUnity.perturbFrequency));

			fastNoiseSIMD.SetPerturbFractalOctaves(
				fastNoiseSIMDUnity.perturbOctaves = EditorGUILayout.IntSlider("Fractal Octaves", fastNoiseSIMDUnity.perturbOctaves, 2, 9));
			fastNoiseSIMD.SetPerturbFractalLacunarity(
				fastNoiseSIMDUnity.perturbLacunarity = EditorGUILayout.FloatField("Fractal Lacunarity", fastNoiseSIMDUnity.perturbLacunarity));
			fastNoiseSIMD.SetPerturbFractalGain(fastNoiseSIMDUnity.perturbGain = EditorGUILayout.FloatField("Fractal Gain", fastNoiseSIMDUnity.perturbGain));
		}

		if (GUILayout.Button("Reset"))
		{
			fastNoiseSIMD.SetSeed(fastNoiseSIMDUnity.seed = 1337);
			fastNoiseSIMD.SetFrequency(fastNoiseSIMDUnity.frequency = 0.01f);
			fastNoiseSIMD.SetNoiseType(fastNoiseSIMDUnity.noiseType = FastNoiseSIMD.NoiseType.Simplex);

			fastNoiseSIMD.SetFractalOctaves(fastNoiseSIMDUnity.octaves = 3);
			fastNoiseSIMD.SetFractalLacunarity(fastNoiseSIMDUnity.lacunarity = 2.0f);
			fastNoiseSIMD.SetFractalGain(fastNoiseSIMDUnity.gain = 0.5f);
			fastNoiseSIMD.SetFractalType(fastNoiseSIMDUnity.fractalType = FastNoiseSIMD.FractalType.FBM);

			fastNoiseSIMD.SetCellularDistanceFunction(
				fastNoiseSIMDUnity.cellularDistanceFunction = FastNoiseSIMD.CellularDistanceFunction.Euclidean);
			fastNoiseSIMD.SetCellularReturnType(
				fastNoiseSIMDUnity.cellularReturnType = FastNoiseSIMD.CellularReturnType.CellValue);

			fastNoiseSIMD.SetCellularNoiseLookupType(fastNoiseSIMDUnity.cellularNoiseLookupType = FastNoiseSIMD.NoiseType.Simplex);
			fastNoiseSIMD.SetCellularNoiseLookupFrequency(fastNoiseSIMDUnity.cellularNoiseLookupFrequency = 0.2f);

			fastNoiseSIMD.SetPerturbType(fastNoiseSIMDUnity.perturbType = FastNoiseSIMD.PerturbType.None);
			fastNoiseSIMD.SetPerturbAmp(fastNoiseSIMDUnity.perturbAmp = 1.0f);
			fastNoiseSIMD.SetPerturbFrequency(fastNoiseSIMDUnity.perturbFrequency = 0.5f);

			fastNoiseSIMD.SetPerturbFractalOctaves(fastNoiseSIMDUnity.perturbOctaves = 3);
			fastNoiseSIMD.SetPerturbFractalLacunarity(fastNoiseSIMDUnity.perturbLacunarity = 2.0f);
			fastNoiseSIMD.SetPerturbFractalGain(fastNoiseSIMDUnity.perturbGain = 0.5f);
		}
	}

	public override bool HasPreviewGUI()
	{
		return true;
	}

	public override GUIContent GetPreviewTitle()
	{
		return new GUIContent("FastNoiseSIMD Unity - " + ((FastNoiseSIMDUnity)target).noiseName);
	}

	public override void DrawPreview(Rect previewArea)
	{
		if (previewArea.height < 9)
			return;

		FastNoiseSIMDUnity fastNoiseSIMDUnity = ((FastNoiseSIMDUnity)target);
		FastNoiseSIMD fastNoiseSIMD = fastNoiseSIMDUnity.fastNoiseSIMD;

		Texture2D tex = new Texture2D((int)previewArea.width, (int)previewArea.height);
		Color32[] pixels = new Color32[tex.width * tex.height];

		Vector3[] vectors = new Vector3[tex.width * tex.height];

		int index = 0;
		for (int y = 0; y < tex.height; y++)
		{
			for (int x = 0; x < tex.width; x++)
			{
				vectors[index++] = new Vector3(x, y);
			}
		}

		float[] noiseSet = new float[vectors.Length];
		fastNoiseSIMD.FillNoiseSetVector(noiseSet, new FastNoiseSIMD.VectorSet(vectors));

		index = 0;
		for (int y = 0; y < tex.height; y++)
		{
			for (int x = 0; x < tex.width; x++)
			{
				byte noise = (byte)Mathf.Clamp(noiseSet[index] * 127.5f + 127.5f, 0f, 255f);
				pixels[index++] = new Color32(noise, noise, noise, 255);
			}
		}

		tex.SetPixels32(pixels);
		tex.Apply();
		GUI.DrawTexture(previewArea, tex, ScaleMode.StretchToFill, false);
	}
}