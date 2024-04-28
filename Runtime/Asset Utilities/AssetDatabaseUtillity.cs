using System.Collections.Generic;
using System.IO;

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


        public static List<T> FindAssetsOfType<T>(string searchGlobFilter = "") where T :  Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets($"{searchGlobFilter} t:{typeof(T).Name}");

            foreach ( string guid in guids ) 
            {
                _cachedPath = AssetDatabase.GUIDToAssetPath(guid);

                assets.Add(AssetDatabase.LoadAssetAtPath<T>(_cachedPath));
            }
            
            return assets;
        }
        /// <summary>
        /// Returns the first asset found using the search glob filter if any is passed in. If no filter string is passed in it will just find the first type without worrying about any filters being applied.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="searchGlobFilter"></param>
        /// <returns></returns>
        public static T FindFirstAssetOfType<T>(string searchGlobFilter = "") where T : Object
        {
            string[] guids = AssetDatabase.FindAssets($"{searchGlobFilter} t:{typeof(T).Name}");
            _cachedPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<T>(_cachedPath);
        }

        public static T GetOrCreateScriptableObject<T>(this string defaultPath) where T : ScriptableObject
        {
            if(!FileUtility.IsFolderPathValid(defaultPath))
                FileUtility.CreateFolderPath(defaultPath);

            T asset = ScriptableObject.CreateInstance<T>();
            Debug.Log(asset.GetType());
            AssetDatabase.CreateAsset(asset, defaultPath);
            AssetDatabase.SaveAssets();
            return asset;
        }

        #region Directory Utilities
        /// <summary>
        /// Gets the logical folder that an asset is in. The logical folder is the folder relative to the Unity Project's root not the folder path from the drive or storage device.
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static string GetLogicalFolderFromAsset(this ScriptableObject script)
        {
            // TODO: Replace the return Split with Unity's Path.Extensions GetDirectoryName

            // Gets the asset in the asset database.
            var asset = MonoScript.FromScriptableObject(script);

            _cachedPath = AssetDatabase.GetAssetPath(asset);
            Debug.Log(Path.GetDirectoryName(_cachedPath));

            return _cachedPath.Split(asset.name)[0];
        }
        /// <summary>
        /// Gets the logical folder that an asset is in. The logical folder is the folder relative to the Unity Project's root not the folder path from the drive or storage device.
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static string GetLogicalFolderFromAsset(this MonoScript asset)
        {
            // TODO: Replace the return Split with Unity's Path.Extensions GetDirectoryName
            _cachedPath = AssetDatabase.GetAssetPath(asset);
            return _cachedPath.Split(asset.name)[0];
        }
        #endregion

#endif
    }
}
