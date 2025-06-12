using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace SFEditor.Utilities
{
    public static class SerializedPropertyExtensions
    {
        public static IEnumerable<SerializedProperty> GetChildrenProperties(this SerializedProperty property, int depth = 1)
        {

            var depthOfParent = property.depth;
            var enumerator = property.GetEnumerator();

            while(enumerator.MoveNext())
            {
                if(enumerator.Current is not SerializedProperty childProperty) continue;
                if(childProperty.depth > depthOfParent + depth) continue;

                yield return childProperty.Copy();
            }
        }

        public static IEnumerable<SerializedProperty> GetChildrenProperties(this SerializedObject obj, string startingProperty, int depth = 1)
        {
            SerializedProperty iterator = obj.GetIterator();
            return iterator.GetChildrenProperties();
        }

        public static IEnumerable<SerializedProperty> GetChildrenProperties(this SerializedObject obj, SerializedProperty iterator, int depth = 1)
        {
            return iterator.GetChildrenProperties();
        }

        public static void VisitAll(this SerializedObject obj, string firstProperty, bool visitChildren = false)
        {
            var prop = obj.FindProperty(firstProperty);
            do
            {
                Debug.Log(prop.name);
            }
            while(prop.Next(visitChildren));
        }
    }
}