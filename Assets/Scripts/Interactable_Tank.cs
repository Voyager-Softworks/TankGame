using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Allows the player to enter and exit the tank.
/// @TODO might need re-name when more interactables on tank are added.
/// </summary>
public class Interactable_Tank : MonoBehaviour
{
    [System.Serializable]
    public class InteractPoint
    {
        [SerializeField, Utils.ReadOnly] private string m_name = "No Transform";
        public Transform m_transform;
        public float m_radius = 1f;

        public bool IsInRange(Collider _collider)
        {
            // null checks
            if (m_transform == null || _collider == null)
            {
                return false;
            }

            // check if part of collider within radius
            Vector3 closestPoint = _collider.ClosestPoint(m_transform.position);
            return Vector3.Distance(closestPoint, m_transform.position) <= m_radius;
        }

        /// <summary>
        /// Updates the name of the point to the name of the transform.
        /// </summary>
        /// <returns>True if the name changed.</returns>
        public bool UpdateName()
        {
            bool didChange = false;
            if (m_transform != null)
            {
                if (m_name != m_transform.name)
                {
                    m_name = m_transform.name;
                    didChange = true;
                }
            }
            else
            {
                if (m_name != "No Transform")
                {
                    m_name = "No Transform";
                    didChange = true;
                }
            }

            return didChange;
        }
    }

    [Header("References")]
    public List<InteractPoint> m_interactPoints;

    private void OnDrawGizmos() {
        foreach (InteractPoint point in m_interactPoints)
        {
            // null checks
            if (point.m_transform == null)
            {
                continue;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(point.m_transform.position, point.m_radius);
        }
    }

    private void Awake()
    {
        if (m_interactPoints == null || m_interactPoints.Count == 0)
        {
            Debug.LogError("Interactable_Tank.Awake | Interact points not assigned!");
        }
    }

    private void Update()
    {
        // null checks
        if (Tank.Instance == null || Player.Instance == null)
        {
            return;
        }

        // check if player is trying to enter
        if (!Tank.Instance.m_isPlayerInside && IsPlayerInRange())
        {
            if (InputManager.PlayerSpecial.Interact.triggered)
            {
                Tank.Instance.OnPlayerEnter();
                Player.Instance.DisablePlayer();
            }
        }
    }

    /// <summary>
    /// Checks if the player trans is in range of any of the interact points.
    /// </summary>
    /// <returns></returns>
    private bool IsPlayerInRange()
    {
        // null checks
        if (Player.Instance == null)
        {
            return false;
        }

        foreach (InteractPoint point in m_interactPoints)
        {
            if (point.IsInRange(Player.Instance.m_collider))
            {
                return true;
            }
        }

        return false;
    }

    // custom editor
    #if UNITY_EDITOR
    [CustomEditor(typeof(Interactable_Tank))]
    public class Interactable_Tank_Editor : Editor
    {
        private Interactable_Tank m_target;

        private void OnEnable()
        {
            m_target = (Interactable_Tank)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // update names of points
            bool needToSave = false;
            for (int i = 0; i < m_target.m_interactPoints.Count; i++)
            {
                if (m_target.m_interactPoints[i].UpdateName())
                {
                    needToSave = true;
                }
            }

            if (needToSave)
            {
                EditorUtility.SetDirty(m_target);
            }
        }
    }
    #endif
}