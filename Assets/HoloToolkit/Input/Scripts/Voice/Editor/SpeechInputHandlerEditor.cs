// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HoloToolkit.Unity.InputModule
{
    [CustomEditor(typeof(SpeechInputHandler))]
    public class SpeechInputHandlerEditor : Editor
    {
        private SerializedProperty keywordsProperty;
        private string[] registeredKeywords;

        private void OnEnable()
        {
            keywordsProperty = serializedObject.FindProperty("keywords");
            registeredKeywords = RegisteredKeywords().Distinct().ToArray();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ShowList(keywordsProperty);
            serializedObject.ApplyModifiedProperties();

            // error and warning messages
            if (keywordsProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No keywords have been assigned!", MessageType.Warning);
            }
            else
            {
                SpeechInputHandler handler = target as SpeechInputHandler;
                string duplicateKeyword = handler.keywords.GroupBy(keyword => keyword.Keyword.ToLower()).Where(group => group.Count() > 1).Select(group => group.Key).FirstOrDefault();
                if (duplicateKeyword != null)
                {
                    EditorGUILayout.HelpBox("Keyword '" + duplicateKeyword + "' is assigned more than once!", MessageType.Warning);
                }
            }
        }

        private static GUIContent removeButtonContent = new GUIContent("-", "Remove keyword");
        private static GUIContent addButtonContent = new GUIContent("+", "Add keyword");
        private static GUILayoutOption miniButtonWidth = GUILayout.Width(20.0f);

        private void ShowList(SerializedProperty list)
        {
            EditorGUI.indentLevel++;

            // remove the keywords already assigned from the registered list
            SpeechInputHandler handler = target as SpeechInputHandler;
            string[] availableKeywords = registeredKeywords.Except(handler.keywords.Select(keywordAndResponse => keywordAndResponse.Keyword)).ToArray();

            // keyword rows
            for (int index = 0; index < list.arraySize; index++)
            {
                // the element
                SerializedProperty elementProperty = list.GetArrayElementAtIndex(index);
                EditorGUILayout.BeginHorizontal();
                bool elementExpanded = EditorGUILayout.PropertyField(elementProperty);
                GUILayout.FlexibleSpace();
                // the remove element button
                bool elementRemoved = GUILayout.Button(removeButtonContent, EditorStyles.miniButton, miniButtonWidth);
                if (elementRemoved)
                {
                    list.DeleteArrayElementAtIndex(index);
                }
                EditorGUILayout.EndHorizontal();

                if (!elementRemoved && elementExpanded)
                {
                    SerializedProperty keywordProperty = elementProperty.FindPropertyRelative("Keyword");
                    string[] keywords = availableKeywords.Concat(new[] { keywordProperty.stringValue }).OrderBy(keyword => keyword).ToArray();
                    int previousSelection = ArrayUtility.IndexOf(keywords, keywordProperty.stringValue);
                    int currentSelection = EditorGUILayout.Popup("Keyword", previousSelection, keywords);
                    if (currentSelection != previousSelection)
                    {
                        keywordProperty.stringValue = keywords[currentSelection];
                    }

                    SerializedProperty responseProperty = elementProperty.FindPropertyRelative("Response");
                    EditorGUILayout.PropertyField(responseProperty, true);
                }
            }

            // add button row
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            // the add element button
            if (GUILayout.Button(addButtonContent, EditorStyles.miniButton, miniButtonWidth))
            {
                list.InsertArrayElementAtIndex(list.arraySize);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }

        private IEnumerable<string> RegisteredKeywords()
        {
            foreach(SpeechInputSource source in Object.FindObjectsOfType<SpeechInputSource>())
            {
                foreach(SpeechInputSource.KeywordAndKeyCode keywordAndKeyCode in source.Keywords)
                {
                    yield return keywordAndKeyCode.Keyword;
                }
            }
        }
    }
}
