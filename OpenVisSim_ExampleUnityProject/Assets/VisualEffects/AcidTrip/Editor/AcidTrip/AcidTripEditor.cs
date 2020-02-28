/*
 * Author : Maxime JUMELLE
 * Namespace : AcidTrip
 * Project : AcidTrip
 * 
 * If you have any suggestion or comment, you can write me at webmaster[at]hardgames3d.com
 * 
 * File : AcidTripEditor.cs
 * 
 * */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace AcidTrip
{
	[CustomEditor(typeof(AcidTrip))]
	class AcidTripEditor : Editor {
		
		private SerializedObject s_target;
		private int theme;
		
		private string[] presets = new string[]
		{
			"Custom", "Low Level", "Medium Level", "High Level", "Colorful", "Lost"
		};
	
		public void OnEnable()
		{
			s_target = new SerializedObject (target);
		}
		
		public override void OnInspectorGUI()
		{
			s_target.Update ();
			
			EditorGUILayout.LabelField ("Reproduces view during an acid trip.", EditorStyles.miniLabel);

			theme = EditorGUILayout.Popup("Themes", theme, presets); 

			EditorGUILayout.PropertyField (s_target.FindProperty(string.Format("Wavelength")), new GUIContent("Waves Frequency"));
			EditorGUILayout.PropertyField (s_target.FindProperty(string.Format("DistortionStrength")), new GUIContent("Distortion Strength"));
			EditorGUILayout.PropertyField (s_target.FindProperty(string.Format("Sparkling")), new GUIContent("Sparkling"));

			EditorGUILayout.PropertyField (s_target.FindProperty(string.Format("SaturationBase")), new GUIContent("Saturation Base"));
			EditorGUILayout.PropertyField (s_target.FindProperty(string.Format("SaturationSpeed")), new GUIContent("Saturation Speed"));
			EditorGUILayout.PropertyField (s_target.FindProperty(string.Format("SaturationAmplitude")), new GUIContent("Saturation Amplitude"));

			switch (theme) {
			case 1:
				s_target.FindProperty(string.Format("Wavelength")).floatValue = 3.0f;
				s_target.FindProperty(string.Format("DistortionStrength")).floatValue = 0.1f;
				s_target.FindProperty(string.Format("Sparkling")).boolValue = false;
				s_target.FindProperty(string.Format("SaturationBase")).floatValue = 1.0f;
				s_target.FindProperty(string.Format("SaturationSpeed")).floatValue = 1.0f;
				s_target.FindProperty(string.Format("SaturationAmplitude")).floatValue = 0.1f;
				break;
			case 2:
				s_target.FindProperty(string.Format("Wavelength")).floatValue = 1.0f;
				s_target.FindProperty(string.Format("DistortionStrength")).floatValue = 0.25f;
				s_target.FindProperty(string.Format("Sparkling")).boolValue = false;
				s_target.FindProperty(string.Format("SaturationBase")).floatValue = 1.0f;
				s_target.FindProperty(string.Format("SaturationSpeed")).floatValue = 1.0f;
				s_target.FindProperty(string.Format("SaturationAmplitude")).floatValue = 0.3f;
				break;
			case 3:
				s_target.FindProperty(string.Format("Wavelength")).floatValue = 1.2f;
				s_target.FindProperty(string.Format("DistortionStrength")).floatValue = 0.35f;
				s_target.FindProperty(string.Format("Sparkling")).boolValue = true;
				s_target.FindProperty(string.Format("SaturationBase")).floatValue = 1.4f;
				s_target.FindProperty(string.Format("SaturationSpeed")).floatValue = 1.6f;
				s_target.FindProperty(string.Format("SaturationAmplitude")).floatValue = 0.5f;
				break;
			case 4:
				s_target.FindProperty(string.Format("Wavelength")).floatValue = 2.0f;
				s_target.FindProperty(string.Format("DistortionStrength")).floatValue = 0.3f;
				s_target.FindProperty(string.Format("Sparkling")).boolValue = false;
				s_target.FindProperty(string.Format("SaturationBase")).floatValue = 4.0f;
				s_target.FindProperty(string.Format("SaturationSpeed")).floatValue = 3.0f;
				s_target.FindProperty(string.Format("SaturationAmplitude")).floatValue = 1.0f;
				break;
			case 5:
				s_target.FindProperty(string.Format("Wavelength")).floatValue = 1.0f;
				s_target.FindProperty(string.Format("DistortionStrength")).floatValue = 0.6f;
				s_target.FindProperty(string.Format("Sparkling")).boolValue = false;
				s_target.FindProperty(string.Format("SaturationBase")).floatValue = 0.0f;
				s_target.FindProperty(string.Format("SaturationSpeed")).floatValue = 1.4f;
				s_target.FindProperty(string.Format("SaturationAmplitude")).floatValue = 3.0f;
				break;
			}
			
			s_target.ApplyModifiedProperties();
		}
	}
}