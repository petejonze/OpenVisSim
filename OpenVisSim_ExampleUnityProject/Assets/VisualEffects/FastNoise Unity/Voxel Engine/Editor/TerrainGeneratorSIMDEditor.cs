using UnityEngine;
using UnityEditor;

namespace VoxelEngine
{
	[CustomEditor(typeof(TerrainGeneratorSIMD), true)]
	public class TerrainGeneratorSIMDEditor : UnityEditor.Editor
	{
		private bool elementsFoldout = true;

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, new[] { "m_Script", "fastNoiseSIMDUnity" });

			TerrainGeneratorSIMD terrainGenerator = ((TerrainGeneratorSIMD)target);

			if (elementsFoldout = EditorGUILayout.Foldout(elementsFoldout, "Noise Array Elements"))
			{
				for (int i = 0; i < terrainGenerator.fastNoiseSIMDUnity.Length; i++)
				{
					if (!terrainGenerator.fastNoiseSIMDUnity[i])
					{
						var childNoise = terrainGenerator.gameObject.GetComponents<FastNoiseSIMDUnity>();

						if (childNoise.Length == 0)
							(terrainGenerator.fastNoiseSIMDUnity[i] = terrainGenerator.gameObject.AddComponent<FastNoiseSIMDUnity>()).noiseName = "Default Noise";
						else
							terrainGenerator.fastNoiseSIMDUnity[i] = childNoise[childNoise.Length - 1];
					}

					terrainGenerator.fastNoiseSIMDUnity[i] = (FastNoiseSIMDUnity)EditorGUILayout.ObjectField("#" + i + " - " + terrainGenerator.fastNoiseSIMDUnity[i].noiseName, terrainGenerator.fastNoiseSIMDUnity[i], typeof(FastNoiseSIMDUnity), true);
				}

				if (GUILayout.Button("Create new FastNoiseSIMD Unity"))
				{
					terrainGenerator.gameObject.AddComponent<FastNoiseSIMDUnity>();
				}
			}

			serializedObject.ApplyModifiedProperties();
		}
	}

}