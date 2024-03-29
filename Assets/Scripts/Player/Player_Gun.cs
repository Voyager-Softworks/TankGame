using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Controls the gun of the player.
/// </summary>
public class Player_Gun : MonoBehaviour
{
	/// <summary>
	/// Data for a shell.
	/// </summary>
	[System.Serializable]
	public class ShellData
	{
		public float m_damage = 100.0f;
		public float m_range = 100.0f;
		public bool m_isDirty = false;
		public bool m_isSpent = false;

		public ShellData()
		{
			// default values
			m_damage = 100.0f;
			m_range = 100.0f;
			m_isDirty = false;
			m_isSpent = false;
		}
	}

	/// <summary>
	/// Data for a clip of shells.
	/// </summary>
	[System.Serializable]
	public class ClipData
	{
		private int m_maxSize = 5;
		/// <summary>
		/// i=0 is the bottom of the clip, i=last is the top of the clip.
		/// </summary>
		public List<ShellData> m_shells = new List<ShellData>();

		/// <summary>
		/// Create a new clip of shells.
		/// </summary>
		/// <param name="_shells">The number of shells to add to the clip. If -1, use max size.</param>
		public ClipData(int _shells = -1)
		{
			// if -1, use max size
			if (_shells == -1)
			{
				_shells = m_maxSize;
			}

			// add shells
			for (int i = 0; i < _shells; i++)
			{
				m_shells.Add(new ShellData());
			}
		}

		public void Add(ShellData _shell)
		{
			if (m_shells.Count < m_maxSize)
			{
				m_shells.Add(_shell);
			}
		}

		public ShellData Top(bool _remove = false)
		{
			if (m_shells.Count == 0)
			{
				return null;
			}
			ShellData shell = m_shells[m_shells.Count - 1];
			if (_remove)
			{
				m_shells.RemoveAt(m_shells.Count - 1);
			}
			return shell;
		}
	}

	[Header("References")]
	public Transform m_shellPoint;
	public Transform m_firePoint;
	public Transform m_aimPoint;
	public Animator m_animator;

	public List<Transform> m_clipShells = new List<Transform>();

	[Header("Ammo")]
	[SerializeField] private int m_startingClips = 20;
	[NonSerialized] private ShellData m_shellInChamber = null;
	public ShellData ShellInChamber { get { return m_shellInChamber; } }
	[NonSerialized] private ClipData m_currentClip = null;
	public ClipData CurrentClip { get { return m_currentClip; } }
	[NonSerialized] private List<ClipData> m_totalClips = new List<ClipData>();
	public List<ClipData> TotalClips { get { return m_totalClips; } }

	[Header("Timers")]
	public float m_aimTime = 0.5f;
	public float m_unaimTime = 0.5f;
	[SerializeField, Utils.ReadOnly] private float m_aimAmount = 0.0f; public float AimAmount { get { return m_aimAmount; } }
	[Tooltip("How far into the aim amount till we are using the sights")] public float m_aimSightThreshold = 0.3f;
	public float m_aimFovMulti = 0.75f;
	public float m_aimFovSpeed = 50.0f;
	public float m_shootTime = 0.5f;
	public float m_dryFireTime = 0.5f;
	public float m_boltTime = 2.5f;
	public float m_shellEjectDelay = 1.0f;
	public float m_reloadTime = 3.0f;

	[Header("State")]
	[SerializeField, Utils.ReadOnly] private bool m_canAim = true; public bool CanAim { get { return m_canAim; } }
	[SerializeField, Utils.ReadOnly] private bool m_isAiming = false; public bool IsAiming { get { return m_isAiming; } }
	[SerializeField, Utils.ReadOnly] private bool m_canUnAim = false; public bool CanUnAim { get { return m_canUnAim; } }
	[SerializeField, Utils.ReadOnly] private bool m_canShoot = true; public bool CanShoot { get { return m_canShoot; } }
	[SerializeField, Utils.ReadOnly] private bool m_isShooting = false; public bool IsShooting { get { return m_isShooting; } }
	[SerializeField, Utils.ReadOnly] private bool m_canBolt = false; public bool CanBolt { get { return m_canBolt; } }
	[SerializeField, Utils.ReadOnly] private bool m_isBolting = false; public bool IsBolting { get { return m_isBolting; } }
	[SerializeField, Utils.ReadOnly] private bool m_canReload = true; public bool CanReload { get { return m_canReload; } }
	[SerializeField, Utils.ReadOnly] private bool m_isReloading = false; public bool IsReloading { get { return m_isReloading; } }

	[Header("Prefabs")]
	public GameObject m_shellPrefab;
	public GameObject m_fxGunShot;
	public GameObject m_fxSmokeTrail;

	[Header("Follow Cam")]
	public bool m_followCam = false;
	public Vector3 m_originalCamPos;
	private Vector3 m_originalPlayerPos;

	[Header("Debug")]
	private Vector3 m_aimDir = Vector3.zero;

	private void Awake()
	{
		// add starting clips
		for (int i = 0; i < m_startingClips; i++)
		{
			m_totalClips.Add(new ClipData(5));
		}
	}

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

		// aim (continuous)
		UpdateAim();
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
			StartCoroutine(TryReload());
		}
	}

	/// <summary>
	/// Aims the gun.
	/// <br/>Since aiming is continuous and can be interrupted, this method is called every frame, and doesnt use coroutines.
	/// </summary>
	private void UpdateAim()
	{
		bool wantsToAim = InputManager.PlayerGun.Aim.IsPressed();

		// aim (+ can only start if can shoot)
		if (wantsToAim && m_canAim && m_canShoot)
		{
			m_canUnAim = true;
			m_isAiming = true;

			// begin aim animation
			m_animator.SetBool("Aiming", true);

			// increment aim amount
			m_aimAmount = Mathf.Clamp01(m_aimAmount + Time.deltaTime / m_aimTime);

			// if fully aimed, set state
			if (m_aimAmount >= 1.0f)
			{
				m_canAim = false;
			}
		}
		// unaim
		else if (!wantsToAim && m_canUnAim)
		{
			m_canAim = true;
			m_isAiming = false;

			// begin unaim animation
			m_animator.SetBool("Aiming", false);

			// decrement aim amount
			m_aimAmount = Mathf.Clamp01(m_aimAmount - Time.deltaTime / m_unaimTime);

			// if fully unaimed, set state
			if (m_aimAmount <= 0.0f)
			{
				m_canUnAim = false;
			}
		}

		Player_Movement movement = Player.Instance.m_movement;
		Camera cam = movement.m_cam;
		bool useAimPoint = m_aimAmount > m_aimSightThreshold;
		m_aimDir = useAimPoint ? m_aimPoint.position - cam.transform.position : cam.transform.forward;
		// draw debug line
		Debug.DrawRay(cam.transform.position, m_aimDir * (m_shellInChamber != null ? m_shellInChamber.m_range : 100f), useAimPoint ? Color.green : Color.red);

		/* Nicer, but not working during partial aims
		// use the aim amount to update the FOV
		float fov = movement.DefaultFOV;
		if (IsAiming)
		{
			fov = Mathf.Lerp(movement.DefaultFOV, movement.DefaultFOV * m_aimFovMulti, m_aimAmount / m_aimSightThreshold);
		}
		else
		{
			fov = Mathf.Lerp(movement.DefaultFOV, movement.DefaultFOV * m_aimFovMulti, 1f - (1f - m_aimAmount) / m_aimSightThreshold);
		}
		cam.fieldOfView = fov; */

		// just use delta time for now
		float currentFov = cam.fieldOfView;
		float newFov = currentFov;
		if (IsAiming)
		{
			newFov = Mathf.Clamp(currentFov - Time.deltaTime * m_aimFovSpeed, movement.DefaultFOV * m_aimFovMulti, movement.DefaultFOV);
		}
		else
		{
			newFov = Mathf.Clamp(currentFov + Time.deltaTime * m_aimFovSpeed, movement.DefaultFOV * m_aimFovMulti, movement.DefaultFOV);
		}
		cam.fieldOfView = newFov;

		// update sensitivity (current fov / default fov)
		movement.m_mouseSensitivityMulti = newFov / movement.DefaultFOV;
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

		// Normal shoot
		// No shell, dry fire
		if (m_shellInChamber == null)
		{
			// audio
			AudioManager.SpawnSound<AutoSound_GunEmpty>(m_firePoint.position); // Temp sound

			// wait for dry fire time
			yield return new WaitForSeconds(m_dryFireTime);

			// no shell, wont bolt
			m_canShoot = true;
			m_canReload = true;
		}
		// Dirty or spent shell, dry fire
		else if (m_shellInChamber.m_isDirty || m_shellInChamber.m_isSpent)
		{
			m_shellInChamber.m_isSpent = true;

			// audio
			AudioManager.SpawnSound<AutoSound_GunEmpty>(m_firePoint.position); // Temp sound

			// wait for dry fire time
			yield return new WaitForSeconds(m_dryFireTime);

			// is shell, need to bolt
			m_canBolt = true;
		}
		// normal shoot
		else if (m_shellInChamber != null && !m_shellInChamber.m_isSpent && !m_shellInChamber.m_isDirty)
		{
			m_shellInChamber.m_isSpent = true;

			if (m_isAiming)
			{
				m_animator.SetTrigger("Shoot_Aim");
				m_animator.ResetTrigger("Shoot_Hip");
			}
			else
			{
				m_animator.SetTrigger("Shoot_Hip");
				m_animator.ResetTrigger("Shoot_Aim");
			}

			// FX
			Instantiate(m_fxGunShot, m_firePoint.position, m_firePoint.rotation, m_firePoint);
			Instantiate(m_fxSmokeTrail, m_firePoint.position, m_firePoint.rotation, m_firePoint);

			// if aiming, use aim point, else use cam forward
			Player_Movement movement = Player.Instance.m_movement;
			Camera cam = movement.m_cam;

			// raycastall to check for hits
			LayerMask ignoreMask = LayerMask.GetMask("Player");
			RaycastHit[] hits = Physics.RaycastAll(cam.transform.position, m_aimDir, m_shellInChamber.m_range, ~ignoreMask);
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
					Health.DamageInfo damageInfo = new Health.DamageInfo(m_shellInChamber.m_damage, gameObject, m_firePoint.position, hit.point, hit.normal);

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
		AutoSound reloadSound = AudioManager.SpawnSound<AutoSound_GunBolt>(m_shellPoint.position);
		reloadSound.transform.parent = m_shellPoint;
		// spawn shell after a delay
		StartCoroutine(CosmeticEjectShell(m_shellEjectDelay));

		// wait for bolt time
		yield return new WaitForSeconds(m_boltTime);

		// re-chamber using top shell in clip
		m_shellInChamber = m_currentClip?.Top(_remove: true) ?? null;

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
		AutoSound shellSound = AudioManager.SpawnSound<AutoSound_EjectShell>(m_shellPoint.position);
		Rigidbody shellRb = shell.GetComponent<Rigidbody>();
		if (shellRb != null)
		{
			// right and up
			shellRb.velocity = m_shellPoint.right * 2f + m_shellPoint.up * 2f;
			// add player vel
			shellRb.velocity += Player.Instance.m_movement.CalcVelocity();
			// random spin
			shellRb.angularVelocity = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)) * 10f;
		}
	}

	/// <summary>
	/// Reload a clip of ammo.
	/// </summary>
	/// <returns></returns>
	private IEnumerator TryReload()
	{
		if (!m_canReload)
		{
			yield break;
		}

		m_isReloading = true;
		m_canShoot = false;
		m_canBolt = false;
		m_canReload = false;

		// do we have any clips
		if (m_totalClips.Count > 0)
		{
			m_animator.SetTrigger("Reload");

			// audio
			AutoSound reloadSound = AudioManager.SpawnSound<AutoSound_GunReload>(m_shellPoint.position);
			reloadSound.transform.parent = m_shellPoint;

			// wait for reload time
			yield return new WaitForSeconds(m_reloadTime);

			// use first clip
			m_currentClip = m_totalClips[0];
			m_totalClips.RemoveAt(0);

			// re-chamber using top shell in clip
			m_shellInChamber = m_currentClip?.Top(_remove: true) ?? null;
		}
		// out of ammo or clip is full
		else
		{
			AudioManager.SpawnSound<AutoSound_GunEmpty>(m_firePoint.position); // Temp sound
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

	#region Custom Editor
#if UNITY_EDITOR
	[CustomEditor(typeof(Player_Gun))]
	public class Player_GunEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			Player_Gun script = (Player_Gun)target;

			// Begin Ammo vertical
			GUILayout.BeginVertical("Ammo", "window");
			// debug ammo
			EditorGUILayout.LabelField("Shell in Chamber", script.m_shellInChamber != null ?
				(script.m_shellInChamber.m_isSpent ? "Spent" : "Live") + "," +
				(script.m_shellInChamber.m_isDirty ? "Dirty" : "Clean")
				: "None"
			);
			EditorGUILayout.LabelField("Current Clip Shells", script.m_currentClip?.m_shells.Count.ToString() ?? "None");
			EditorGUILayout.LabelField("Total Clips", script.m_totalClips.Count.ToString());
			// end Ammo vertical
			GUILayout.EndVertical();

			// space
			EditorGUILayout.Space();

			DrawDefaultInspector();
		}
	}
#endif
	#endregion
}