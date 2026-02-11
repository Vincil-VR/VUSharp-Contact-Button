using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using VRC.Udon;

namespace Vincil.VUSharp.UI.ContactButton
{
    [CustomEditor(typeof(ContactButtonImplementation))]
    public class ContactButtonEditor : Editor
    {
        private SerializedProperty onClickEventReceiversProperty;
        private SerializedProperty onClickEventReceiverMethodNamesProperty;
        int onClickNewSize = 0;

        private SerializedProperty onUnclickEventReceiversProperty;
        private SerializedProperty onUnclickEventReceiverMethodNamesProperty;
        int onUnclickNewSize = 0;

        private void OnEnable()
        {
            // Get the serialized properties from the target script
            onClickEventReceiversProperty = serializedObject.FindProperty("onClickEventReceiversArray");
            onClickEventReceiverMethodNamesProperty = serializedObject.FindProperty("onClickEventReceiverMethodNamesArray");
            onClickNewSize = onClickEventReceiversProperty.arraySize; // Initialize newSize with the current array size

            onUnclickEventReceiversProperty = serializedObject.FindProperty("onUnclickEventReceiversArray");
            onUnclickEventReceiverMethodNamesProperty = serializedObject.FindProperty("onUnclickEventReceiverMethodNamesArray");
            onUnclickNewSize = onUnclickEventReceiversProperty.arraySize; // Initialize newSize with the current array size
        }

        public override void OnInspectorGUI()
        {
            //if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            DrawDefaultInspector();

            DrawInteractSettings();

            // Update the serialized object to get the latest values
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("OnClick Listeners (UdonBehaviour & Method Names)", EditorStyles.boldLabel);
            HandleParallelArray(onClickEventReceiversProperty, onClickEventReceiverMethodNamesProperty, ref onClickNewSize);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("OnUnclick Listeners (UdonBehaviour & Method Names)", EditorStyles.boldLabel);
            HandleParallelArray(onUnclickEventReceiversProperty, onUnclickEventReceiverMethodNamesProperty, ref onUnclickNewSize);

            // Apply all modifications and support Undo/Redo
            serializedObject.ApplyModifiedProperties();
        }

        private void HandleParallelArray(SerializedProperty objectReferenceArrayProperty, SerializedProperty stringArrayProperty, ref int arrayNewSizeProperty)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add", GUILayout.Width(100)))
            {
                arrayNewSizeProperty++;
            }
            if (GUILayout.Button("Remove", GUILayout.Width(100)))
            {
                if (arrayNewSizeProperty > 0) arrayNewSizeProperty--;
            }
            EditorGUILayout.EndHorizontal();


            if (arrayNewSizeProperty != objectReferenceArrayProperty.arraySize)
            {
                int arraySizeDifference = arrayNewSizeProperty - objectReferenceArrayProperty.arraySize;
                objectReferenceArrayProperty.arraySize = arrayNewSizeProperty;
                stringArrayProperty.arraySize = arrayNewSizeProperty;
                if(arraySizeDifference > 0)
                {
                    // If we added new elements, initialize them to default values
                    for (int i = objectReferenceArrayProperty.arraySize - arraySizeDifference; i < objectReferenceArrayProperty.arraySize; i++)
                    {
                        objectReferenceArrayProperty.GetArrayElementAtIndex(i).objectReferenceValue = null; // Default for UdonBehaviour references
                        stringArrayProperty.GetArrayElementAtIndex(i).stringValue = ""; // Default for method name strings
                    }
                }
            }

            // Draw the elements in a synchronized manner
            for (int i = 0; i < objectReferenceArrayProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();

                // Get the properties for the current index in each array
                SerializedProperty objectReferenceElement = objectReferenceArrayProperty.GetArrayElementAtIndex(i);
                SerializedProperty stringElement = stringArrayProperty.GetArrayElementAtIndex(i);

                // Draw the fields on the same horizontal line
                EditorGUILayout.LabelField("Element " + i, GUILayout.Width(80));
                EditorGUILayout.PropertyField(objectReferenceElement, GUIContent.none, GUILayout.MinWidth(50));
                EditorGUILayout.PropertyField(stringElement, GUIContent.none, GUILayout.MinWidth(30));

                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawInteractSettings()
        {
            UdonBehaviour backingBehaviour = UdonSharpEditorUtility.GetBackingUdonBehaviour((UdonSharpBehaviour)target);

            string currentText = backingBehaviour.interactText;

            EditorGUI.BeginChangeCheck();

            string newInteractText = EditorGUILayout.TextField("Interaction Text", currentText);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(backingBehaviour, "Change interact text");
                backingBehaviour.interactText = newInteractText;

                if (PrefabUtility.IsPartOfPrefabInstance(backingBehaviour))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(backingBehaviour);
            }

            float currentProximity = backingBehaviour.proximity;

            EditorGUI.BeginChangeCheck();
            float newProximity = EditorGUILayout.Slider("Proximity", currentProximity, 0f, 100f);

            if (EditorGUI.EndChangeCheck())
            {
                if (newProximity >= 0f)
                {
                    Undo.RecordObject(backingBehaviour, "Change interact distance");
                    backingBehaviour.proximity = newProximity;

                    if (PrefabUtility.IsPartOfPrefabInstance(backingBehaviour))
                        PrefabUtility.RecordPrefabInstancePropertyModifications(backingBehaviour);
                }
            }
        }
    }
}
