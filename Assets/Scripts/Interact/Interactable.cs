using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Interactable is a base class for all interactable objects in the game. <br/>
/// Usually to be interacted by <see cref="Interacter"/>.
/// </summary>
public class Interactable : MonoBehaviour
{
    protected static List<Interactable> s_allInteractables = new List<Interactable>();
    public static List<Interactable> AllInteractables { get { return s_allInteractables; } }

    [Header("Settings")]
    [SerializeField] private bool m_isInteractable = true;
    public bool IsInteractable { get { return m_isInteractable; } set { m_isInteractable = value; } }
    [SerializeField] protected float m_interactRange = 5f; // takes the biggest, this or interacter's range
    public float InteractRange { get { return m_interactRange; } }
    [SerializeField] protected bool m_destroyOnInteract = false;

    private Vector3[] m_localCorners = new Vector3[8];
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
        if (!s_allInteractables.Contains(this))
        {
            s_allInteractables.Add(this);
        }

        m_gizmoColor = Random.ColorHSV();

        UpdateCornerPositions();
    }

    protected virtual void OnDestroy()
    {
        // remove from list
        if (s_allInteractables.Contains(this))
        {
            s_allInteractables.Remove(this);
        }
    }

    /// <summary>
    /// Called when this object is interacted with.
    /// </summary>
    public virtual void OnInteract(Interacter _interacter)
    {
        Debug.Log($"Interactable.OnInteract | {gameObject.name} was interacted with by {_interacter.name}.");

        // destroy if needed
        if (m_destroyOnInteract)
        {
            Destroy(gameObject);
        }
    }

    /// <summary> Re-calculates the corner positions of this object based on its mesh filters. </summary>
    public virtual void UpdateCornerPositions()
    {
        m_localCorners = Utils.Methods.GetCorners(gameObject, _local: true);
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(Interactable), true)]
    public class InteractableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Interactable interactable = (Interactable)target;

            if (GUILayout.Button("Update Corners"))
            {
                interactable.UpdateCornerPositions();
            }
        }
    }
    #endif
}
