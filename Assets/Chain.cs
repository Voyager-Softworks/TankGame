using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// A chain that has links, an anchor, and an end. This script takes in existing objects and creates a chain between them.
/// </summary>
public class Chain : MonoBehaviour
{
    /// <summary> List of objects to link together. Provided by the scene</summary>
    public List<GameObject> m_linkObjects = new List<GameObject>();
    /// <summary> List of objects in the chain that do not move. Provided by the scene</summary>
    public List<GameObject> m_anchors = new List<GameObject>();

    /// <summary>
    /// Adds 
    /// </summary>
    public void CreateChain()
    {
        // add hinge joints to the links
        for (int i = 0; i < m_linkObjects.Count; i++)
        {
            if (i == 0)
            {
                // add hinge joint to the first link
                HingeJoint hinge = m_linkObjects[i].AddComponent<HingeJoint>();
                hinge.connectedBody = m_anchors[0].GetComponent<Rigidbody>();
                hinge.anchor = Vector3.zero;
                hinge.axis = Vector3.forward;
            }
            else
            {
                // add hinge joint to the rest of the links
                HingeJoint hinge = m_linkObjects[i].AddComponent<HingeJoint>();
                hinge.connectedBody = m_linkObjects[i - 1].GetComponent<Rigidbody>();
                hinge.anchor = Vector3.zero;
                hinge.axis = Vector3.forward;
            }
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(Chain))]
    public class ChainEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Chain chain = (Chain)target;
            if (GUILayout.Button("Create Chain"))
            {
                chain.CreateChain();
            }
        }
    }
    #endif
}