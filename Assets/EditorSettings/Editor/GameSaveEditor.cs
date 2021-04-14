using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameSave))]
public class GameSaveEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Override Current Saved Scene: [" + GameSave.SavedScene + "] to [" +
            ((GameSave)target).override_scene + "]"))
        {
            GameSave.SaveScene(((GameSave)target).override_scene);
            ((GameSave)target).override_scene = "";
        }
        EditorGUILayout.LabelField("game passed:");
        bool a = EditorGUILayout.Toggle(GameSave.PassedGame);
        if (GUI.changed && a != GameSave.PassedGame)
        {
            GameSave.SavePassedGame(a);
        }

        EditorGUILayout.LabelField("tutorial passed:");
        bool b = EditorGUILayout.Toggle(GameSave.PassedTutorial);
        if (GUI.changed)
        {
            GameSave.SaveTutorial(b);
        }
    }
}
