using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Base class for all enemies.
/// </summary>
public abstract class Enemy : MonoBehaviour
{
    [SerializeField] private Health m_health;
    public Health Health { get { return m_health; } }
    [SerializeField] private Animator m_animator;
    public Animator Animator { get { return m_animator; } }

    protected virtual void Awake()
    {
        if (m_health == null)
        {
            m_health = GetComponent<Health>();
        }
    }

    // custom editor
#if UNITY_EDITOR
    [CustomEditor(typeof(Enemy), true)]
    public class EnemyEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Enemy enemy = (Enemy)target;
            bool didChange = false;

            List<string> missingComponents = new List<string>();

            // try auto-assign components
            if (enemy.Health == null)
            {
                Health health = enemy.GetComponent<Health>();
                if (health != null)
                {
                    enemy.m_health = health;
                    didChange = true;
                }
                else { missingComponents.Add(typeof(Health).Name); }
            }
            if (enemy.Animator == null)
            {
                Animator animator = enemy.GetComponent<Animator>();
                if (animator != null)
                {
                    enemy.m_animator = animator;
                    didChange = true;
                }
                else { missingComponents.Add(typeof(Animator).Name); }
            }

            // save if changed
            if (didChange)
            {
                EditorUtility.SetDirty(enemy);
            }

            // warnings
            if (missingComponents.Count > 0)
            {
                // (\n-) for newline and bullet point
                string missingList = string.Join("\n- ", missingComponents.ToArray());
                EditorGUILayout.HelpBox("Missing components:\n- " + missingList, MessageType.Warning);
            }
        }
    }
#endif
}