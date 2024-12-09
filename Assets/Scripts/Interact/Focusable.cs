using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Focusable : MonoBehaviour
{
    protected static List<Focusable> s_allFocusables = new List<Focusable>();
    public static List<Focusable> AllFocusables { get { return s_allFocusables; } }

    [Header("Settings")]
    [SerializeField] private bool m_isFocusable = true;
    public bool IsFocusable { get { return m_isFocusable; } set { m_isFocusable = value; } }

    protected Vector3[] m_localCorners = new Vector3[8];
    public Vector3[] WorldCorners
    {
        get
        {
            // convert to world space
            Vector3[] worldCorners = new Vector3[m_localCorners.Length];
            for (int i = 0; i < m_localCorners.Length; i++)
            {
                worldCorners[i] = transform.TransformPoint(m_localCorners[i]);
            }
            return worldCorners;
        }
    }
    protected Vector3 m_center = Vector3.zero;
    public Vector3 WorldCenter { get { return transform.TransformPoint(m_center); } }

    public Color m_gizmoColor = Color.green;

    private void OnDrawGizmos()
    {
        Gizmos.color = m_gizmoColor;
        Vector3[] corners = WorldCorners;
        for (int i = 0; i < corners.Length; i++)
        {
            Gizmos.DrawWireSphere(corners[i], 0.01f);
            Gizmos.DrawLine(corners[i], corners[(i + 1) % corners.Length]);
        }
    }

    protected virtual void Awake()
    {
        // add to list
        if (!s_allFocusables.Contains(this))
        {
            s_allFocusables.Add(this);
        }

        m_gizmoColor = Random.ColorHSV();

        UpdateCornerPositions();
    }

    virtual protected void Start()
    {
        // update corners
    }

    protected virtual void OnDestroy()
    {
        // remove from list
        if (s_allFocusables.Contains(this))
        {
            s_allFocusables.Remove(this);
        }
    }

    /// <summary> Re-calculates the corner positions of this object based on its mesh filters. </summary>
    public virtual void UpdateCornerPositions()
    {
        m_localCorners = Utils.Methods.GetCorners(gameObject, _local: true);

        // calculate center
        m_center = Vector3.zero;
        for (int i = 0; i < m_localCorners.Length; i++)
        {
            m_center += m_localCorners[i];
        }
        m_center /= m_localCorners.Length;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Focusable), true)]
    public class InteractableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Focusable interactable = (Focusable)target;

            if (GUILayout.Button("Update Corners"))
            {
                interactable.UpdateCornerPositions();
            }
        }
    }
#endif
}