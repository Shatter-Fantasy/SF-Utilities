using System;
using System.IO;

using UnityEditor;

using UnityEngine;

namespace SF.UICreator
{
    public static class FileUtility
    {
        /// <summary>
        /// Creates a folder and any needed sub folders using the Assets directory as the root to place the folders in.
        /// </summary>
        /// <param name="folderPath"></param>
        public static void CreateFolderPath(string folderPath)
        {
            if(folderPath.StartsWith("/"))
                folderPath.Remove(0, 1);

            Directory.CreateDirectory(Application.dataPath+ "/" + folderPath);
            AssetDatabase.Refresh();
        }
    }
}
