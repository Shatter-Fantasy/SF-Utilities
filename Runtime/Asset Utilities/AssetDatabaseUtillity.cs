using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SF.UICreator
{
    public static class AssetDatabaseUtillity
    {
        private static string _cachedPath;
#if UNITY_EDITOR
        public static List<T> FindAssetsOfType<T>() where T :  Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            
            foreach ( string guid in guids ) 
            {
                _cachedPath = AssetDatabase.GUIDToAssetPath(guid);
                assets.Add(AssetDatabase.LoadAssetAtPath<T>(_cachedPath));
            }

            return assets;
        }

        public static T FindFirstAssetOfType<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            _cachedPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<T>(_cachedPath);
        }
#endif
    }
}
