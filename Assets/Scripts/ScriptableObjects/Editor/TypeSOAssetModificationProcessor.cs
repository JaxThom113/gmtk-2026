using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TypeSOAssetModificationProcessor : AssetModificationProcessor
{
    // This static method is called by Unity when an asset is about to be deleted.
    static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
    {
        // 1. Get the object type from the asset path
        Object asset = AssetDatabase.LoadMainAssetAtPath(path);

        if (asset != null)
        {
            // 2. Check if the asset is the specific type you are looking for
            if (asset is ITypeCanReset) // Replace ScriptableObject with your specific type
            {
                (asset as BaseTypeSO).OnFileDelete();
            }
            // Add more specific type checks as needed
        }

        // 3. Return DidNotDelete to allow Unity to proceed with the deletion
        // (or DidDelete if your custom logic handles the actual file removal)
        return AssetDeleteResult.DidNotDelete;
    }
}
