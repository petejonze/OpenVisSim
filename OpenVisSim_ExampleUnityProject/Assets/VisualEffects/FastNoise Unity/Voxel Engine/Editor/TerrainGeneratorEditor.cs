using UnityEngine;
using UnityEditor;

namespace VoxelEngine
{
	[CustomEditor(typeof (TerrainGenerator), true)]
	public class TerrainGeneratorEditor : UnityEditor.Editor
	{
		private bool elementsFoldout = true;

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, new[] {"m_Script", "fastNoiseUnity"});

			TerrainGenerator terrainGenerator = ((TerrainGenerator) target);

			if (elementsFoldout = EditorGUILayout.Foldout(elementsFoldout, "Noise Array Elements"))
			{
				for (int i = 0; i < terrainGenerator.fastNoiseUnity.Length; i++)
				{
					if (!terrainGenerator.fastNoiseUnity[i])
					{
						var childNoise = terrainGenerator.gameObject.GetComponents<FastNoiseUnity>();

						if (childNoise.Length == 0)
							(terrainGenerator.fastNoiseUnity[i] = terrainGenerator.gameObject.AddComponent<FastNoiseUnity>()).noiseName =
								"Default Noise";
						else
							terrainGenerator.fastNoiseUnity[i] = childNoise[childNoise.Length - 1];
					}

					terrainGenerator.fastNoiseUnity[i] =
						(FastNoiseUnity)
							EditorGUILayout.ObjectField("#" + i + " - " + terrainGenerator.fastNoiseUnity[i].noiseName,
								terrainGenerator.fastNoiseUnity[i], typeof (FastNoiseUnity), true);
				}

				if (GUILayout.Button("Create new FastNoise Unity"))
				{
					terrainGenerator.gameObject.AddComponent<FastNoiseUnity>();
				}
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}