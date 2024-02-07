using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the gun of the player.
/// </summary>
public class Player_Gun : MonoBehaviour
{
    public enum ShellType
    {
        Live,
        Dirty,
        Spent,
        SpentDirty,
        None
    }

    [Header("References")]
    public Transform m_shellPoint;
    public Transform m_firePoint;
    public Animator m_animator;

    [Header("Stats")]
    public float m_damage = 100.0f;
    public float m_range = 100.0f;

    [Header("Ammo")]
    [SerializeField] private int m_clipSize = 5;
    [SerializeField] private int m_clipAmmo = 5;
    [SerializeField] private int m_totalAmmo = 20;

    [Header("Timers")]
    public float m_shootTime = 0.5f;
    public float m_dryFireTime = 0.5f;
    public float m_boltTime = 2.5f;
    public float m_shellEjectDelay = 1.0f;
    public float m_reloadTime = 3.0f;

    [Header("State")]
    [SerializeField, Utils.ReadOnly] private ShellType m_shellInChamber = ShellType.Live;
    [SerializeField, Utils.ReadOnly] private bool m_canShoot = true;
    [SerializeField, Utils.ReadOnly] private bool m_isShooting = false;
    [SerializeField, Utils.ReadOnly] private bool m_canBolt = false;
    [SerializeField, Utils.ReadOnly] private bool m_isBolting = false;
    [SerializeField, Utils.ReadOnly] private bool m_canReload = true;
    [SerializeField, Utils.ReadOnly] private bool m_isReloading = false;

    [Header("Prefabs")]
    public GameObject m_shellPrefab;
    public GameObject m_fxGunShot;
    public GameObject m_fxSmokeTrail;

    [Header("Follow Cam")]
    public bool m_followCam = false;
    public Vector3 m_originalCamPos;
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
            StartCoroutine(TryShoot());
        }
        // when player releases the shoot button, eject shell
        if (InputManager.PlayerGun.Shoot.IsPressed() == false)
        {
            StartCoroutine(TryBolt());
        }
        // reload
        if (InputManager.PlayerGun.Reload.triggered)
        {
            StartCoroutine(Reload());
        }
    }

    /// <summary>
    /// Shoot without doing bolt.
    /// </summary>
    /// <returns></returns>
    private IEnumerator TryShoot()
    {
        if (!m_canShoot)
        {
            yield break;
        }

        m_isShooting = true;
        m_canShoot = false;
        m_canReload = false;
        m_canBolt = false;

        // reduce ammo
        bool hasAmmo = m_clipAmmo > 0;

        // Normal shoot
        if (hasAmmo && m_shellInChamber == ShellType.Live)
        {
            m_clipAmmo--;
            m_shellInChamber = ShellType.Spent;

            m_animator.SetTrigger("Shoot");

            // FX
            Instantiate(m_fxGunShot, m_firePoint.position, m_firePoint.rotation);
            Instantiate(m_fxSmokeTrail, m_firePoint.position, m_firePoint.rotation, m_firePoint);

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

            // is shell, need to bolt
            m_canBolt = true;
        }
        // dirty shoot (blank shot, no damage)
        else if (hasAmmo && m_shellInChamber == ShellType.Dirty)
        {
            m_clipAmmo--;
            m_shellInChamber = ShellType.SpentDirty;

            m_animator.SetTrigger("Shoot");

            // FX
            Instantiate(m_fxGunShot, m_firePoint.position, m_firePoint.rotation);
            Instantiate(m_fxSmokeTrail, m_firePoint.position, m_firePoint.rotation, m_firePoint);

            // wait for shoot time
            yield return new WaitForSeconds(m_shootTime);

            // is shell, need to bolt
            m_canBolt = true;
        }
        // dry fire
        else
        {
            AudioManager.SpawnSound<AutoSound_Footstep>(m_firePoint.position); // Temp sound

            // wait for dry fire time
            yield return new WaitForSeconds(m_dryFireTime);

            // no shell, wont bolt
            m_canShoot = true;
            m_canReload = true;
        }

        m_isShooting = false;
    }

    /// <summary>
    /// Bolt the gun.
    /// </summary>
    /// <returns></returns>
    private IEnumerator TryBolt()
    {
        if (!m_canBolt)
        {
            yield break;
        }
        
        m_isBolting = true;
        m_canShoot = false;
        m_canBolt = false;
        m_canReload = false;

        m_animator.SetTrigger("Bolt");

        // audio
        AutoSound reloadSound = AudioManager.SpawnSound<AutoSound_GunReload>(m_shellPoint.position);
        reloadSound.transform.parent = m_shellPoint;
        // spawn shell after a delay
        StartCoroutine(CosmeticEjectShell(m_shellEjectDelay));

        // wait for bolt time
        yield return new WaitForSeconds(m_boltTime);

        bool hasAmmo = m_clipAmmo > 0;
        if (hasAmmo)
        {
            m_shellInChamber = ShellType.Live; //@TODO: implement dirty shells
        }
        else
        {
            m_shellInChamber = ShellType.None;
        }

        m_canShoot = true;
        m_canReload = true;
        m_isBolting = false;
    }

    /// <summary>
    /// Ejects a shell from the gun.
    /// @TODO: Implement shell types
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    private IEnumerator CosmeticEjectShell(float delay)
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
    /// Reload a clip of ammo.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Reload()
    {
        if (!m_canReload)
        {
            yield break;
        }

        m_isReloading = true;
        m_canShoot = false;
        m_canBolt = false;
        m_canReload = false;

        // reload check
        int ammoNeeded = m_clipSize - m_clipAmmo;
        int ammoAvailable = Mathf.Min(ammoNeeded, m_totalAmmo);

        bool shouldReload = ammoAvailable > 0;
        if (shouldReload)
        {
            m_animator.SetTrigger("Reload");

            // audio
            AutoSound reloadSound = AudioManager.SpawnSound<AutoSound_GunReload>(m_shellPoint.position);
            reloadSound.transform.parent = m_shellPoint;

            // wait for reload time
            yield return new WaitForSeconds(m_reloadTime);

            m_clipAmmo += ammoAvailable;
            m_totalAmmo -= ammoAvailable;

            // re-chamber
            bool hasAmmo = m_clipAmmo > 0;
            if (hasAmmo)
            {
                m_shellInChamber = ShellType.Live; //@TODO: implement dirty shells
            }
            else
            {
                m_shellInChamber = ShellType.None;
            }
        }
        // out of ammo or clip is full
        else
        {
            AudioManager.SpawnSound<AutoSound_Footstep>(m_firePoint.position); // Temp sound
        }

        m_canShoot = true;
        m_canReload = true;
        m_isReloading = false;
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
