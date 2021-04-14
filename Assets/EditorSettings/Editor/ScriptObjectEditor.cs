using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(ScriptObjectScriptable))]
public class ScriptObjectEditor : Editor
{

    public void OnEnable()
    {
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        /*
        serializedObject.Update();

        ScriptObjectScriptable sos = (ScriptObjectScriptable)target;

        sos.name_ = EditorGUILayout.TextField("Script Name", sos.name_);
        EditorGUILayout.PropertyField(ST);
        sos.previous = null;
        EditorGUILayout.PropertyField(P);
        sos.source = (ScriptTextSource)ST.enumValueIndex;
        serializedObject.ApplyModifiedProperties();
        
        //text is from computer generated script
        if (sos.source == ScriptTextSource.SCRIPT)
        {
            sos.text_asset = null;
            EditorGUILayout.PropertyField(TW);
        }
        //text is from manually written script (text asset)
        else
        {
            sos.text_writer = null;
            EditorGUILayout.PropertyField(TA);
        }
        serializedObject.ApplyModifiedProperties();*/
        if (GUILayout.Button("Compile"))
        {
            ((ScriptObjectScriptable)target).CheckSyntax();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
