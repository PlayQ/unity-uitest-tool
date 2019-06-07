#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.Nodes;
using UnityEditor;
using UnityEngine;

public class AddBaseClassWindow : EditorWindow
{
    private List<string> classes;

    private void OnEnable()
    {
        titleContent.text = "Choose Base Class";
        maxSize= new Vector2(250, 100);
        minSize= new Vector2(250, 100);

        classes = new List<string>();

        //we can cache it and reset after compilation
        var assemblies = NodeFactory.GetAllSuitableAssemblies();
        foreach (var assembly in assemblies)
        {
            var typesInAssemblyList = assembly.GetTypes();
            foreach (var type in typesInAssemblyList)
            {
                classes.Add(type.FullName);
            }
        }

        classes.Sort();
    }

    void OnLostFocus()
    {
        Close();
    }

    private int selectedIndex;
    public Action<string> Result = s => { };
    private string filer = String.Empty;
    
    private void OnGUI()
    {
        Rect content = new Rect(10, 10, position.width - 20, EditorGUIUtility.singleLineHeight);
        
//        content.y += EditorGUIUtility.singleLineHeight;
        filer = EditorGUI.TextField(content, "Filter", filer);
        
        content.y += EditorGUIUtility.singleLineHeight*1.5f;
        selectedIndex = EditorGUI.Popup(content, selectedIndex, classes.Where(x => x.ToLower().Contains(filer.ToLower())).ToArray());

        content.y += EditorGUIUtility.singleLineHeight*1.5f;
        if (GUI.Button(content, "Accept"))
        {
            Result(classes[selectedIndex]);
            Close();
        }
        content.y += EditorGUIUtility.singleLineHeight*1.2f;
        if (GUI.Button(content, "Cancel"))
        {
            Close();
        }
    }
}

#endif