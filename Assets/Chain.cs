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
    /// <summary> The end of the chain. Provided by the scene</summary>
    public GameObject m_end;

    /// <summary>
    /// Adds 
    /// </summary>
    public void CreateChain()
    {
        // add rigidbodies to the links
        for (int i = 0; i < m_linkObjects.Count; i++)
        {
            GameObject link = m_linkObjects[i];
            Rigidbody rb = link.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = link.AddComponent<Rigidbody>();
            }
            // calculate mass from size
            Mesh mesh = link.GetComponent<MeshFilter>().sharedMesh;
            rb.mass = mesh.bounds.size.magnitude;
            rb.drag = 0.05f;
            rb.angularDrag = 0.05f;

            // if the link is an anchor, make it kinematic
            if (m_anchors.Contains(link))
            {
                rb.isKinematic = true;
            }
        }

        // ensure the end has a rigidbody
        Rigidbody endRb = m_end.GetComponent<Rigidbody>();
        if (endRb == null)
        {
            endRb = m_end.AddComponent<Rigidbody>();
        }

        // add hinge joints to the links
        for (int i = 0; i < m_linkObjects.Count; i++)
        {
            Rigidbody link = m_linkObjects[i].GetComponent<Rigidbody>();
            Rigidbody nextLink = i + 1 < m_linkObjects.Count ? m_linkObjects[i + 1].GetComponent<Rigidbody>() : endRb;

            HingeJoint joint = link.GetComponent<HingeJoint>();
            if (joint == null)
            {
                joint = link.gameObject.AddComponent<HingeJoint>();
            }

            joint.autoConfigureConnectedAnchor = false;
            joint.connectedBody = nextLink;
            joint.axis = Vector3.right;

            // get mid point between the two links
            Vector3 midPoint = (link.transform.position + nextLink.transform.position) / 2;

            // set anchor to mid point
            joint.anchor = link.transform.InverseTransformPoint(midPoint);
            joint.connectedAnchor = nextLink.transform.InverseTransformPoint(midPoint);
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