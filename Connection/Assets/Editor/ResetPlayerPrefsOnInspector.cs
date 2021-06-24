using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class ResetPlayerPrefsOnInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameManager gameManager = (GameManager)target;

        if (GUILayout.Button("Reset PlayerPrefs"))
        {
            gameManager.ResetCompletedLevels();
        }
    }
}
