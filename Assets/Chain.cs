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

    public bool m_autoMass = true;
    public float m_mass = 0.1f;
    public float m_healthMassScale = 10.0f;

    public bool m_canBreak = true;
    public float m_breakForce = 20.0f;
    
    void Awake()
    {
        // listen for link deaths
        for (int i = 0; i < m_linkObjects.Count; i++)
        {
            Health health = m_linkObjects[i].GetComponent<Health>();
            if (health != null)
            {
                // avoid duplicate listeners
                health.OnDeath -= OnLinkDeath;
                health.OnDeath += OnLinkDeath;
            }
        }
    }

    /// <summary>
    /// Creates the chain by adding rigidbodies and hinge joints to the links.
    /// </summary>
    public void CreateChain()
    {
        // add rigidbodies and health to the links
        for (int i = 0; i < m_linkObjects.Count; i++)
        {
            GameObject link = m_linkObjects[i];
            Rigidbody rb = link.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = link.AddComponent<Rigidbody>();
            }
            // calculate mass from size
            if (m_autoMass)
            {
                Mesh mesh = link.GetComponent<MeshFilter>().sharedMesh;
                rb.mass = mesh.bounds.size.magnitude;
            }
            else
            {
                rb.mass = m_mass;
            }
            rb.drag = 1.0f;
            rb.angularDrag = 10.0f;

            // if the link is an anchor, make it kinematic
            bool isAnchor = m_anchors.Contains(link);
            if (isAnchor)
            {
                rb.isKinematic = true;
            }

            Health health = link.GetComponent<Health>();
            if (health == null)
            {
                health = link.AddComponent<Health_Prop>();
            }
            // set health to 10x mass
            health.m_maxHealth = rb.mass * m_healthMassScale;
            health.enabled = m_canBreak && !isAnchor;
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

            joint.breakForce = m_breakForce;
            joint.breakTorque = m_breakForce;
            joint.enablePreprocessing = false;
        }
    }

    /// <summary>
    /// Checks if the chain is broken by checking for destroyed links and missing connections.
    /// </summary>
    /// <returns></returns>
    public bool IsBroken()
    {
        // check for destroyed links
        for (int i = 0; i < m_linkObjects.Count; i++)
        {
            if (m_linkObjects[i] == null)
            {
                return true;
            }
        }

        // check for missing joints/connections
        for (int i = 0; i < m_linkObjects.Count; i++)
        {
            HingeJoint joint = m_linkObjects[i].GetComponent<HingeJoint>();
            if (joint == null || joint.connectedBody == null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Called when a link in the chain dies.
    /// </summary>
    private void OnLinkDeath()
    {
        // remove the hinge joint from dead links
        for (int i = 0; i < m_linkObjects.Count; i++)
        {
            Health health = m_linkObjects[i].GetComponent<Health>();
            if (health != null && health.IsDead)
            {
                HingeJoint joint = m_linkObjects[i].GetComponent<HingeJoint>();
                if (joint != null)
                {
                    Destroy(joint);
                }
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