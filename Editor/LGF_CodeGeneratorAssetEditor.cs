using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using Object = UnityEngine.Object;

namespace Editor
{
    [CustomEditor(typeof(LGF_CodeGeneratorAsset))]
    public class LGF_CodeGeneratorAssetEditor : UnityEditor.Editor
    {
        SerializedProperty generatedFolderPath;
        SerializedProperty resourceConstantAssetsFolderName;
        SerializedProperty resourceConstantTypesForDefaultGeneration;

        void OnEnable()
        {
            generatedFolderPath = serializedObject.FindProperty("generatedFolderPath");
            resourceConstantAssetsFolderName = serializedObject.FindProperty("resourceConstantAssetsFolderName");
            resourceConstantTypesForDefaultGeneration = serializedObject.FindProperty("resourceConstantTypesForDefaultGeneration");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(generatedFolderPath);
            EditorGUILayout.PropertyField(resourceConstantAssetsFolderName);

            ShowTypes(resourceConstantTypesForDefaultGeneration);

            LGF_CodeGeneratorAsset script = (LGF_CodeGeneratorAsset)target;

            if (GUILayout.Button("Generate"))
            {
                script.GenerateAll();
            }

            serializedObject.ApplyModifiedProperties();
        }

        // Function to manage Types in List
        void ShowTypes(SerializedProperty list)
        {
            // Show size of List
            EditorGUILayout.PropertyField(list);

            if (list.isExpanded)
            {
                // Show individual entries
                for (int i = 0; i < list.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    SerializedProperty MyListRef = list.GetArrayElementAtIndex(i);
                    MyListRef.stringValue = EditorGUILayout.TextField("Type", MyListRef.stringValue);

                    // Display a button for browsing for Types within the Assembly
                    if (GUILayout.Button("Browse", GUILayout.MaxWidth(70)))
                    {
                        SelectTypeWindow.Open((type) => {
                            MyListRef.stringValue = type.AssemblyQualifiedName;
                            serializedObject.ApplyModifiedProperties();
                        });
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }

    public class SelectTypeWindow : EditorWindow
    {    
        private Action<Type> onSelect;
        private Vector2 scrollPosition;
        private string search = "";

        private Type[] _types;
        
        public static void Open(Action<Type> onSelect)
        {
            SelectTypeWindow window = GetWindow<SelectTypeWindow>();
            window.onSelect = onSelect;
        }
        
        private void OnEnable()
        {
            _types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.ContainsGenericParameters && !t.IsAbstract && typeof(Object).IsAssignableFrom(t))
                .OrderBy(t => t.FullName).ToArray();
        }
        
        void OnGUI()
        {
            EditorGUILayout.LabelField("Select a Type", EditorStyles.boldLabel);
            search = EditorGUILayout.TextField("Search:", search);

            // Filter types based on search input
            _types = _types.Where(t => t.FullName.ToLower().Contains(search.ToLower())).ToArray();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach(var type in _types)
            {
                if(GUILayout.Button(type.FullName))
                {
                    onSelect(type);
                    Close();
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
