
using UnityEditor;
using UnityEngine;

public class SpawnContactButtonMenuItems
{
    
    [MenuItem("GameObject/Contact Buttons/Contact Button", false, 10)]
    private static void SpawnContactButton(MenuCommand menuCommand)
    {
        CoreSpawner("Packages/com.vincil.vusharp.contact-button/Runtime/Prefabs/ContactButton.prefab", menuCommand);
    }

    [MenuItem("GameObject/Contact Buttons/Contact Button (With Audio)", false, 10)]
    private static void SpawnContactButtonWithAudio(MenuCommand menuCommand)
    {
        CoreSpawner("Packages/com.vincil.vusharp.contact-button/Runtime/Prefabs/ContactButton (With Audio).prefab", menuCommand);
    }

    private static void CoreSpawner(string assetPath, MenuCommand menuCommand)
    {
        GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

        if (prefabAsset == null)
        {
            Debug.LogError("Prefab not found at path: " + assetPath);
            return;
        }

        GameObject parent = menuCommand.context as GameObject;
        Transform parentTransform = parent != null ? parent.transform : null;

        string uniqueName = GameObjectUtility.GetUniqueNameForSibling(parentTransform, prefabAsset.name);


        GameObject spawnedObject = Object.Instantiate(prefabAsset);
        spawnedObject.name = uniqueName;


        if (parent != null)
        {
            GameObjectUtility.SetParentAndAlign(spawnedObject, parent);
        }

        // Register the action with Undo system for Ctrl+Z functionality
        Undo.RegisterCreatedObjectUndo(spawnedObject, "Spawn " + spawnedObject.name);

        // Optional: Select the newly spawned object in the Hierarchy
        Selection.activeGameObject = spawnedObject;
    }
}
