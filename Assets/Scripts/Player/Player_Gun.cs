using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the gun of the player.
/// </summary>
public class Player_Gun : MonoBehaviour
{
    [Header("References")]
    public Transform m_shellPoint;
    public Transform m_firePoint;

    [Header("Stats")]
    public float m_damage = 100.0f;
    public float m_range = 100.0f;

    [Header("Timers")]
    public float m_shootTime = 0.5f;
    public float m_boltTime = 2.5f;
    public float m_shellEjectDelay = 1.0f;

    [Header("State")]
    [SerializeField, Utils.ReadOnly] private bool m_canShoot = true;
    [SerializeField, Utils.ReadOnly] private bool m_readyToEject = false;

    [Header("Prefabs")]
    public GameObject m_shellPrefab;
    public GameObject m_fxGunShot;

    [Header("Follow Cam")]
    public bool m_followCam = false;
    private Vector3 m_originalCamPos;
    private Vector3 m_originalPlayerPos;

    // Start is called before the first frame update
    void Start()
    {
        // null checks
        if (Player.Instance == null)
        {
            Debug.LogError("Player_Gun.Awake | Player instance not found!");
            return;
        }
        Player_Movement movement = Player.Instance.m_movement;

        // set original positions
        m_originalCamPos = transform.position - movement.m_cam.transform.position;
        m_originalPlayerPos = transform.position - Player.Instance.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // null checks
        if (Player.Instance == null)
        {
            return;
        }

        FollowCam();

        // shoot
        if (InputManager.PlayerGun.Shoot.triggered)
        {
            StartCoroutine(Shoot());
        }
        // when player releases the shoot button, eject shell
        if (m_readyToEject && InputManager.PlayerGun.Shoot.IsPressed() == false)
        {
            StartCoroutine(BoltCoroutine());
        }
    }

    private IEnumerator Shoot()
    {
        if (!m_canShoot)
        {
            yield break;
        }

        m_canShoot = false;

        // FX
        Instantiate(m_fxGunShot, m_firePoint.position, m_firePoint.rotation);

        // // raycast against every objects with Health:
        // // get all healths
        // List<Health> healths = new List<Health>(Health.GetAllHealth());
        // // sort by distance
        // healths.Sort((a, b) => Vector3.Distance(a.transform.position, m_firePoint.position).CompareTo(Vector3.Distance(b.transform.position, m_firePoint.position)));

        // raycastall to check for hits
        LayerMask ignoreMask = LayerMask.GetMask("Player");
        RaycastHit[] hits = Physics.RaycastAll(Player.Instance.m_movement.m_cam.transform.position, Player.Instance.m_movement.m_cam.transform.forward, m_range, ~ignoreMask);
        // sort hits by distance
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // loop through hits
        foreach (RaycastHit hit in hits)
        {
            // get health component in child&/parent
            Health health = hit.collider.GetComponentInParent<Health>() ?? hit.collider.GetComponentInChildren<Health>();
            if (health != null)
            {
                // generate damage info
                Health.DamageInfo damageInfo = new Health.DamageInfo(m_damage, gameObject, m_firePoint.position, hit.point, hit.normal);

                // damage health
                health.Damage(damageInfo);
            }

            // break after first hit (for now)
            //@TODO: implement piercing? e.g. StoppingPower stat
            break;
        }

        // wait for shoot time
        yield return new WaitForSeconds(m_shootTime);
        m_readyToEject = true;
    }

    private IEnumerator BoltCoroutine()
    {
        m_readyToEject = false;

        // audio
        AudioManager.SpawnSound<AutoSound_GunReload>(m_shellPoint.position);
        // spawn shell after a delay
        StartCoroutine(EjectShellCoroutine(m_shellEjectDelay));

        // wait for bolt time
        yield return new WaitForSeconds(m_boltTime);

        m_canShoot = true;
    }

    private IEnumerator EjectShellCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        GameObject shell = Instantiate(m_shellPrefab, m_shellPoint.position, m_shellPoint.rotation);
        Rigidbody shellRb = shell.GetComponent<Rigidbody>();
        if (shellRb != null)
        {
            // right and up
            shellRb.velocity = m_shellPoint.right * 2f + m_shellPoint.up * 2f;
            // add player vel
            shellRb.velocity += Player.Instance.m_movement.CalcVelocity();
            // random spin
            shellRb.angularVelocity = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 10f;
        }
    }

    /// <summary>
    /// Looks at where the player cam is looking.
    /// </summary>
    private void FollowCam()
    {
        if (!m_followCam)
        {
            // parent to camera if not already
            if (transform.parent != Player.Instance.m_movement.m_cam.transform)
            {
                transform.SetParent(Player.Instance.m_movement.m_cam.transform);
                // reset position and rotation
                transform.localPosition = m_originalCamPos;
                transform.localRotation = Quaternion.identity;
            }
            return;
        }
        else
        {
            // parent to player if not already
            if (transform.parent != Player.Instance.transform)
            {
                transform.SetParent(Player.Instance.transform);
                // reset position and rotation
                transform.localPosition = m_originalPlayerPos;
                transform.localRotation = Quaternion.identity;
            }
        }

        // null checks
        Player_Movement movement = Player.Instance.m_movement;
        if (movement == null)
        {
            return;
        }

        // look at where camera is looking (raycast to hit point)
        Vector3 hitPoint = Vector3.zero;
        RaycastHit hit;
        LayerMask ignoreMask = LayerMask.GetMask("Player");
        if (Physics.Raycast(movement.m_cam.transform.position, movement.m_cam.transform.forward, out hit, 100f, ~ignoreMask))
        {
            hitPoint = hit.point;
        }
        else
        {
            hitPoint = movement.m_cam.transform.position + movement.m_cam.transform.forward * 100f;
        }

        // move gun to look at hit point
        transform.LookAt(hitPoint);
    }
}
