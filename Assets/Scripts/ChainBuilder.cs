using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Chain))]
public class ChainBuilder : MonoBehaviour
{
    public Chain m_chain;
    public GameObject m_linkPrefab;
    public Vector3 m_prefabRotation = new Vector3(0, 0, 0);

    public Transform m_start;
    public Transform m_end;

    public int m_linkCount = 10;
    public float m_linkDistance = 1.0f;
    public float m_rotationOffset = 90f;
    public bool m_alternateRotation = true;


    public List<GameObject> m_createdLinks = new List<GameObject>();

    private void OnDrawGizmos()
    {
        if (m_start != null && m_end != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(m_start.position, m_end.position);
        }
    }

    /// <summary>
    /// Builds the chain using the provided link prefab.
    /// </summary>
    public void BuildChain()
    {
        // null checks
        if (m_chain == null)
        {
            m_chain = GetComponent<Chain>();
        }
        if (m_chain == null)
        {
            Debug.LogError("No Chain component found.");
            return;
        }
        if (m_linkPrefab == null)
        {
            Debug.LogError("No link prefab found.");
            return;
        }

        ClearChain();

        Vector3 direction = m_end.position - m_start.position;

        // create new links like above, but with a direction
        float currentAngle = 0;
        bool didAlternate = false;
        for (int i = 0; i < m_linkCount; i++)
        {
            GameObject link = Instantiate(m_linkPrefab, transform);
            link.transform.position = m_start.position + direction.normalized * i * m_linkDistance;
            // rotate link to face direction
            link.transform.rotation = Quaternion.LookRotation(direction);
            // rotate using prefab rotation
            link.transform.Rotate(m_prefabRotation);
            // add offset
            link.transform.Rotate(direction, currentAngle);
            m_createdLinks.Add(link);

            // alternate rotation
            if (m_alternateRotation)
            {
                // add or subtract rotation offset
                currentAngle += didAlternate ? m_rotationOffset : -m_rotationOffset;
                didAlternate = !didAlternate;
            }
            else
            {
                currentAngle += m_rotationOffset;
            }
        }

        // update chain
        m_chain.m_linkObjects = m_createdLinks;
        m_chain.m_anchors = new List<GameObject>() { m_createdLinks[0] };
        m_chain.CreateChain();
    }

    /// <summary>
    /// Clears the existing chain.
    /// </summary>
    private void ClearChain()
    {
        // clear existing links
        foreach (GameObject link in m_createdLinks)
        {
            DestroyImmediate(link);
        }
        m_createdLinks.Clear();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ChainBuilder))]
    public class ChainBuilderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ChainBuilder builder = (ChainBuilder)target;
            if (GUILayout.Button("Build Chain"))
            {
                builder.BuildChain();
            }
            if (GUILayout.Button("Clear Chain"))
            {
                builder.ClearChain();
            }
        }
    }
#endif
}