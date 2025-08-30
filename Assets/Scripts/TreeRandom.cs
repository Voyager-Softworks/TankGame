using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TreeRandom : MonoBehaviour
{
    public List<GameObject> m_trees;

    public float m_randomScaleMin = 0.8f;
    public float m_randomScaleMax = 1.2f;

    public bool m_doRandomRotation = true;

    public void FetchTrees()
    {
        m_trees = new List<GameObject>();

        for (int i = 0; i < transform.childCount; i++)
        {
            m_trees.Add(transform.GetChild(i).gameObject);
        }
    }

    public void RandomizeTrees()
    {
        FetchTrees();

        foreach (GameObject tree in m_trees)
        {
            tree.transform.localScale = new Vector3(
                Random.Range(m_randomScaleMin, m_randomScaleMax),
                Random.Range(m_randomScaleMin, m_randomScaleMax),
                Random.Range(m_randomScaleMin, m_randomScaleMax)
            );

            if (m_doRandomRotation)
            {
                tree.transform.rotation = Quaternion.Euler(
                    0,
                    Random.Range(0, 360),
                    0
                );
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TreeRandom))]
    public class TreeRandomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            TreeRandom treeRandom = (TreeRandom)target;

            if (GUILayout.Button("Randomize Trees"))
            {
                treeRandom.RandomizeTrees();
            }

            if (GUILayout.Button("Fetch Trees"))
            {
                treeRandom.FetchTrees();
            }
        }
    }
#endif
}
