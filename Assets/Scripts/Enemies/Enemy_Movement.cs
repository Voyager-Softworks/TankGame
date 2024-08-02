using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_Movement : MonoBehaviour
{
    public NavMeshAgent m_navMeshAgent;
    [Utils.ReadOnly] public Transform m_target;

    private void Start()
    {
        m_navMeshAgent = GetComponent<NavMeshAgent>();
        m_target = Player.Instance.m_model.transform;
    }

    private void Update()
    {
        // if dead, disable navmesh agent
        if (GetComponent<Health>().IsDead)
        {
            m_navMeshAgent.enabled = false;
            return;
        }

        m_navMeshAgent.SetDestination(m_target.position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);

        if (m_navMeshAgent != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, m_navMeshAgent.destination);
        }

        if (m_target != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, m_target.position);
        }
    }
}
