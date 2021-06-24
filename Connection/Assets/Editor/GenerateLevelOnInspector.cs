
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelData))]
public class GenerateLevelOnInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelData LevelData = (LevelData)target;

        if (GUILayout.Button("Generate Level"))
        {
            LevelData.GenerateLevel();
        }
    }
}

