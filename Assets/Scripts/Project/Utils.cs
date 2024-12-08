using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utils
{
    public static class Methods
    {
        public static Bounds GetBounds(GameObject _obj)
        {
            Bounds bounds = new Bounds(_obj.transform.position, Vector3.zero);
            Renderer[] renderers = _obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            return bounds;
        }

        /// <summary>
        /// Custom method to get the locally oriented corners of an object using its mesh filters.
        /// </summary>
        /// <param name="_meshFilter"></param>
        /// <returns></returns>
        public static Vector3[] GetCorners(GameObject _obj, bool _local = false)
        {
            Vector3[] corners = new Vector3[8];

            Vector3 min = Vector3.zero;
            Vector3 max = Vector3.zero;
            MeshFilter[] meshFilters = _obj.GetComponentsInChildren<MeshFilter>();
            // get local corners
            foreach (MeshFilter meshFilter in meshFilters)
            {
                Mesh mesh = meshFilter.mesh;
                if (mesh == null)
                {
                    continue;
                }
                Vector3[] verts = mesh.vertices;
                for (int i = 0; i < verts.Length; i++)
                {
                    Vector3 vert = verts[i];
                    // convert to world space
                    vert = meshFilter.transform.TransformPoint(vert);
                    // relative to main object
                    vert = _obj.transform.InverseTransformPoint(vert);
                    if (i == 0)
                    {
                        min = vert;
                        max = vert;
                    }
                    else
                    {
                        min = Vector3.Min(min, vert);
                        max = Vector3.Max(max, vert);
                    }
                }
            }

            // get the corners
            corners[0] = new Vector3(min.x, min.y, min.z);
            corners[1] = new Vector3(min.x, min.y, max.z);
            corners[2] = new Vector3(max.x, min.y, max.z);
            corners[3] = new Vector3(max.x, min.y, min.z);
            corners[4] = new Vector3(min.x, max.y, min.z);
            corners[5] = new Vector3(min.x, max.y, max.z);
            corners[6] = new Vector3(max.x, max.y, max.z);
            corners[7] = new Vector3(max.x, max.y, min.z);

            if (_local)
            {
                return corners;
            }

            // convert to world space
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = _obj.transform.TransformPoint(corners[i]);
            }

            return corners;
        }
    }

    /// <summary>
    /// ReadOnly attribute for serialized fields.
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute
    {
    }
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
    #endif
}
