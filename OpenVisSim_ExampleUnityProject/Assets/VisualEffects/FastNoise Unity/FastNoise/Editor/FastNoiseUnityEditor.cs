using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FastNoiseUnity))]
public class FastNoiseUnityEditor : Editor
{
	public override void OnInspectorGUI()
	{
		FastNoiseUnity fastNoiseUnity = ((FastNoiseUnity)target);
		FastNoise fastNoise = fastNoiseUnity.fastNoise;

		fastNoiseUnity.noiseName = EditorGUILayout.TextField("Name", fastNoiseUnity.noiseName);

		fastNoiseUnity.generalSettingsFold = EditorGUILayout.Foldout(fastNoiseUnity.generalSettingsFold, "General Settings");

		if (fastNoiseUnity.generalSettingsFold)
		{
			fastNoise.SetNoiseType(
				fastNoiseUnity.noiseType = (FastNoise.NoiseType)EditorGUILayout.EnumPopup("Noise Type", fastNoiseUnity.noiseType));
			fastNoise.SetSeed(fastNoiseUnity.seed = EditorGUILayout.IntField("Seed", fastNoiseUnity.seed));
			fastNoise.SetFrequency(fastNoiseUnity.frequency = EditorGUILayout.FloatField("Frequency", fastNoiseUnity.frequency));
			fastNoise.SetInterp(
				fastNoiseUnity.interp = (FastNoise.Interp)EditorGUILayout.EnumPopup("Interpolation", fastNoiseUnity.interp));
		}

		fastNoiseUnity.fractalSettingsFold = EditorGUILayout.Foldout(fastNoiseUnity.fractalSettingsFold, "Fractal Settings");

		if (fastNoiseUnity.fractalSettingsFold)
		{
			fastNoise.SetFractalType(
				fastNoiseUnity.fractalType =
					(FastNoise.FractalType)EditorGUILayout.EnumPopup("Fractal Type", fastNoiseUnity.fractalType));
			fastNoise.SetFractalOctaves(
				fastNoiseUnity.octaves = EditorGUILayout.IntSlider("Octaves", fastNoiseUnity.octaves, 2, 9));
			fastNoise.SetFractalLacunarity(
				fastNoiseUnity.lacunarity = EditorGUILayout.FloatField("Lacunarity", fastNoiseUnity.lacunarity));
			fastNoise.SetFractalGain(fastNoiseUnity.gain = EditorGUILayout.FloatField("Gain", fastNoiseUnity.gain));
		}

		fastNoiseUnity.cellularSettingsFold = EditorGUILayout.Foldout(fastNoiseUnity.cellularSettingsFold,
			"Cellular Settings");

		if (fastNoiseUnity.cellularSettingsFold)
		{
			fastNoise.SetCellularReturnType(
				fastNoiseUnity.cellularReturnType =
					(FastNoise.CellularReturnType)EditorGUILayout.EnumPopup("Return Type", fastNoiseUnity.cellularReturnType));
			fastNoise.SetCellularDistanceFunction(
				fastNoiseUnity.cellularDistanceFunction =
					(FastNoise.CellularDistanceFunction)
						EditorGUILayout.EnumPopup("Distance Function", fastNoiseUnity.cellularDistanceFunction));

			if (fastNoiseUnity.cellularReturnType == FastNoise.CellularReturnType.NoiseLookup)
			{
				fastNoiseUnity.cellularNoiseLookup =
					(FastNoiseUnity)
						EditorGUILayout.ObjectField("Noise Lookup", fastNoiseUnity.cellularNoiseLookup, typeof(FastNoiseUnity), true);

				if (fastNoiseUnity.cellularNoiseLookup)
					fastNoise.SetCellularNoiseLookup(fastNoiseUnity.cellularNoiseLookup.fastNoise);
			}
		}

		fastNoiseUnity.positionWarpSettingsFold = EditorGUILayout.Foldout(fastNoiseUnity.positionWarpSettingsFold,
			"Position Warp Settings");

		if (fastNoiseUnity.positionWarpSettingsFold)
			fastNoise.SetGradientPerturbAmp(
				fastNoiseUnity.positionWarpAmp = EditorGUILayout.FloatField("Amplitude", fastNoiseUnity.positionWarpAmp));

		if (GUILayout.Button("Reset"))
		{
			fastNoise.SetSeed(fastNoiseUnity.seed = 1337);
			fastNoise.SetFrequency(fastNoiseUnity.frequency = 0.01f);
			fastNoise.SetInterp(fastNoiseUnity.interp = FastNoise.Interp.Quintic);
			fastNoise.SetNoiseType(fastNoiseUnity.noiseType = FastNoise.NoiseType.Simplex);

			fastNoise.SetFractalOctaves(fastNoiseUnity.octaves = 3);
			fastNoise.SetFractalLacunarity(fastNoiseUnity.lacunarity = 2.0f);
			fastNoise.SetFractalGain(fastNoiseUnity.gain = 0.5f);
			fastNoise.SetFractalType(fastNoiseUnity.fractalType = FastNoise.FractalType.FBM);

			fastNoise.SetCellularDistanceFunction(
				fastNoiseUnity.cellularDistanceFunction = FastNoise.CellularDistanceFunction.Euclidean);
			fastNoise.SetCellularReturnType(fastNoiseUnity.cellularReturnType = FastNoise.CellularReturnType.CellValue);

			fastNoise.SetGradientPerturbAmp(fastNoiseUnity.positionWarpAmp = 1.0f);
		}
	}

	public override bool HasPreviewGUI()
	{
		return true;
	}

	public override GUIContent GetPreviewTitle()
	{
		return new GUIContent("FastNoise Unity - " + ((FastNoiseUnity)target).noiseName);
	}

	public override void DrawPreview(Rect previewArea)
	{
		FastNoiseUnity fastNoiseUnity = ((FastNoiseUnity)target);
		FastNoise fastNoise = fastNoiseUnity.fastNoise;

		if (fastNoiseUnity.noiseType == FastNoise.NoiseType.Cellular &&
			fastNoiseUnity.cellularReturnType == FastNoise.CellularReturnType.NoiseLookup &&
			fastNoiseUnity.cellularNoiseLookup == null)
		{
			GUI.Label(previewArea, "Set cellular noise lookup");
			return;
		}

		Texture2D tex = new Texture2D((int)previewArea.width, (int)previewArea.height);
		Color32[] pixels = new Color32[tex.width * tex.height];
		int index = 0;

		for (int y = 0; y < tex.height; y++)
		{
			for (int x = 0; x < tex.width; x++)
			{
				byte noise = (byte)Mathf.Clamp(fastNoise.GetNoise(x * 2f, y * 2f) * 127.5f + 127.5f, 0f, 255f);
				pixels[index++] = new Color32(noise, noise, noise, 255);
			}
		}

		tex.SetPixels32(pixels);
		tex.Apply();
		GUI.DrawTexture(previewArea, tex, ScaleMode.StretchToFill, false);
	}
}