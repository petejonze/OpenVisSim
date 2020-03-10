// file Touchable.cs
// Correctly backfills the missing Touchable concept in Unity.UI's OO chain.

using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(Touchable))]
public class Touchable_Editor : Editor
{ public override void OnInspectorGUI() { } }
#endif
public class Touchable : Text
{ protected override void Awake() { base.Awake(); } }