using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

/// <summary>
/// 
/// </summary>
[CustomEditor(typeof(GenBehaviourScript))]
public class GenBehaviourScriptEditor : Editor
{

    [MenuItem("CONTEXT/GenBehaviourScript/RefGenerate")]
    private static void toRefGenerate()
    {
        List<Editor> editors;
        var ishave = getInspectorInstance(out editors);
        if (!ishave) return;


        for (int i = 0; i < editors.Count; i++)
        {
            var editor = editors[i];
            var self = editor.target as GenBehaviourScript;
            var serializedObject = editor.serializedObject;

            var tuples = GeneratedTest.GetComps(self);
            for (int j = 0; j < tuples.Count; j++)
            {
                var tuple = tuples[j];
                var prop = serializedObject.FindProperty(tuple.Item2);
                var temptr = self.transform;
                if (!string.IsNullOrEmpty(tuple.Item3)) temptr = self.transform.Find(tuple.Item3);

                var comp = temptr.GetComponent(tuple.Item1);
                prop.objectReferenceValue = temptr.GetComponent(tuple.Item1);
            }
            serializedObject.ApplyModifiedProperties();
        }



    }


    [MenuItem("CONTEXT/GenBehaviourScript/RefGenerate", true)]
    private static bool validateRefGenerate()
    {
        List<Editor> editors;
        bool ishave = getInspectorInstance(out editors);
        return ishave;
    }


    private static bool getInspectorInstance(out List<Editor> editorlist)
    {
        var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
        var inspectorwindow = EditorWindow.GetWindow(type);
        editorlist = new List<Editor>();
        var ishave = false;

        if (inspectorwindow == null) return ishave;

        var info = type.GetField("m_Tracker", BindingFlags.NonPublic | BindingFlags.Instance);
        var tracker = info.GetValue(inspectorwindow) as ActiveEditorTracker;
        var editors = tracker.activeEditors;
        var genbegaviour = typeof(GenBehaviourScript);

        for (int i = 0; i < editors.Length; i++)
        {
            var editor_item = editors[i];
            var type_item = editor_item.target.GetType();

            if (type_item.IsSubclassOf(genbegaviour))
            {
                editorlist.Add(editor_item);
                if (!ishave) ishave = true;
            }
        }
        return ishave;
    }



}


/// <summary>
/// 
/// </summary>
[CustomPropertyDrawer(typeof(CheckGeneratedVisiblyAttribute))]
public class CheckGeneratedVisiblyAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var constomattribute = (CheckGeneratedVisiblyAttribute)attribute;
        bool isShow = this.IsShow(constomattribute, property);
        if (isShow)
        {
            GUI.Box(position, "", EditorStyles.popup);
            //label.text = "[" + label.text + "]";
            EditorGUI.PropertyField(position, property, label);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var constomattribute = (CheckGeneratedVisiblyAttribute)attribute;
        bool isShow = this.IsShow(constomattribute, property);
        if (isShow) return base.GetPropertyHeight(property, label);
        else return EditorGUIUtility.standardVerticalSpacing * -1;
    }

    private bool IsShow(CheckGeneratedVisiblyAttribute attribute, SerializedProperty property)
    {
        SerializedProperty serializedProperty = property.serializedObject.FindProperty(attribute.boolname);
        return serializedProperty.boolValue;
    }


}
