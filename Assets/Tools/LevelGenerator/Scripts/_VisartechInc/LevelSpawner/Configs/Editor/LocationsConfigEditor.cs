using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(LocationsConfig))]
public class LocationsConfigEditor : Editor
{
    ReorderableList _locationsList;

    private int _focused = 0;
    
    private void OnEnable()
    {
        InitLocationsList();
    }


    void InitLocationsList()
    {
        _locationsList = new ReorderableList(serializedObject, 
            serializedObject.FindProperty("LocationConfigs"), 
            true, true, true, true);

        _locationsList.drawElementCallback = 
            (rect, index, isActive, isFocused) => {
                var element = _locationsList.serializedProperty.GetArrayElementAtIndex(index);  
                rect.y += 2;
                
                EditorGUI.LabelField(
                    new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight),
                    "Location");
                
                EditorGUI.PropertyField(
                    new Rect(125, rect.y, 100, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("LocationType"), GUIContent.none);              

                var line = 0f;
                
                line++;
                if (_focused == index)
                {
                    line++;

                    EditorGUI.LabelField(
                        new Rect(rect.x, rect.y + (line * EditorGUIUtility.singleLineHeight), 100,
                            EditorGUIUtility.singleLineHeight), "Location settings");

                    line += 1.3f;

                    EditorGUI.LabelField(
                        new Rect(rect.x, rect.y + (line * EditorGUIUtility.singleLineHeight), 100,
                            EditorGUIUtility.singleLineHeight),
                        "Length");

                    EditorGUI.PropertyField(
                        new Rect(125, rect.y + (line * EditorGUIUtility.singleLineHeight), 100,
                            EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("LocationLength"), GUIContent.none);

                    line += 1.3f;
                    

                    EditorGUI.LabelField(
                        new Rect(rect.x, rect.y + (line * EditorGUIUtility.singleLineHeight), 120,
                            EditorGUIUtility.singleLineHeight),
                        "Patterns");

                    EditorGUI.PropertyField(
                        new Rect(125, rect.y + (line * EditorGUIUtility.singleLineHeight), 118,
                            EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("PatternsInfo"), GUIContent.none);

                    line += 1.3f;
                }

            };

        _locationsList.elementHeightCallback = (int index) =>
            index == _focused ? (EditorGUIUtility.singleLineHeight * 7) : EditorGUIUtility.singleLineHeight * 1.5f;
     
        _locationsList.onAddCallback = list => {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            var difficultyTypes = (LocationType[])Enum.GetValues(typeof(LocationType));
            Dictionary<LocationType, bool> usedTypes = new Dictionary<LocationType, bool>();
            
            foreach (var type in difficultyTypes) {
                usedTypes.Add(type, false);
                for (int i = 0; i < index; i++) {
                    var el = list.serializedProperty.GetArrayElementAtIndex(i);
                    if (el.FindPropertyRelative("LocationType").enumValueIndex == (int)type) {
                        usedTypes[type] = true;
                        break;
                    }
                }
            }
            if(usedTypes.Values.All(val => val)) {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                return;
            }

            var nextEnumIndex = (int)difficultyTypes.First(type => !usedTypes[type]);
            element.FindPropertyRelative("LocationType").enumValueIndex = nextEnumIndex; 
        };
        
        _locationsList.onCanRemoveCallback = list => list.count > 1;
        
        _locationsList.onRemoveCallback = list => {
            if (EditorUtility.DisplayDialog("Warning!", 
                "Are you sure you want to delete this difficulty entry?", "Yes", "No")) {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
            }
        };
        
        _locationsList.drawHeaderCallback = rect => {
            EditorGUI.LabelField(
                new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight),
                 new GUIContent("Locations info"));
        };
        _locationsList.onSelectCallback = list =>
        {
            _focused = list.index;
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        _locationsList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}
