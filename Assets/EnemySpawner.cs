using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A debug class to spawn enemies in random locations, and move them in a random direction.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public class EnemyData
    {
        public GameObject m_gameObject = null;
        public float m_lifetime = 10f;
        public float m_moveSpeed = 1f;
    }

    [Header("Settings")]
    public int m_maxEnemies = 10;
    public float m_spawnRadius = 10f;
    public float m_spawnInterval = 5f;
    Vector2 m_moveSpeed = new Vector2(1f, 5f);
    public float m_maxLifetime = 20f;

    [Header("References")]
    public GameObject m_enemyPrefab;

    private List<EnemyData> m_enemies = new List<EnemyData>();

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_spawnRadius);
    }

    private void Start()
    {
        StartCoroutine(SpawnEnemies());
    }

    private void Update()
    {
        // prune enemies
        for (int i = m_enemies.Count - 1; i >= 0; i--)
        {
            // null
            if (m_enemies[i] == null)
            {
                m_enemies.RemoveAt(i);
            }
            // destroyed
            else if (m_enemies[i].m_gameObject == null)
            {
                m_enemies.RemoveAt(i);
            }
            // dead
            else if (m_enemies[i].m_gameObject.GetComponent<Health_Test>().IsDead)
            {
                Destroy(m_enemies[i].m_gameObject, 1f);
                m_enemies.RemoveAt(i);
            }
        }

        // move enemies forward
        foreach (EnemyData enemy in m_enemies)
        {
            enemy.m_gameObject.transform.position += enemy.m_gameObject.transform.forward * enemy.m_moveSpeed * Time.deltaTime;
        }
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            // spawn enemy if not at max
            if (m_enemies.Count < m_maxEnemies)
            {
                // randomize spawn position and rotation
                Vector3 spawnPos = transform.position + Random.insideUnitSphere * m_spawnRadius;
                spawnPos.y = 1f;
                Quaternion spawnRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                GameObject enemy = Instantiate(m_enemyPrefab, spawnPos, spawnRot, transform);

                // randomize move speed
                float moveSpeed = Random.Range(m_moveSpeed.x, m_moveSpeed.y);

                // add enemy to list
                m_enemies.Add(new EnemyData
                {
                    m_gameObject = enemy,
                    m_lifetime = m_maxLifetime,
                    m_moveSpeed = moveSpeed
                });

                // lifetime
                Destroy(enemy, m_maxLifetime);
            }

            yield return new WaitForSeconds(m_spawnInterval);
        }
    }
}
