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
		public static Color DIRTY_COLOR = Color.red;
		public static Color BRASS_COLOR = new Color(0.8f, 0.6f, 0.2f); // brass
		public static Color COPPER_COLOR = new Color(0.72f, 0.45f, 0.2f); // copper
		public static float s_dirtyChance = 0.5f;
		public static float s_dirtyDryFireChance = 0.5f;
		public static float s_maxSpendTries = 5;

		private float m_damage = 100.0f;
		public float Damage { get { return m_damage; } }
		private float m_range = 500.0f;
		public float Range { get { return m_range; } }
		private float m_hitForce = 1.0f;
		public float HitForce { get { return m_hitForce; } }
		private bool m_isDirty = false;
		public bool IsDirty { get { return m_isDirty; } }
		private bool m_isSpent = false;
		public bool IsSpent { get { return m_isSpent; } }
		
		private int m_spendTries = 0;

		public System.Action OnDirty;
		public System.Action OnSpend;

		public ShellData()
		{
			// all values are initialized to default

			// randomize dirty
			m_isDirty = UnityEngine.Random.value < s_dirtyChance;
		}

		/// <summary>
		/// Makes the shell dirty.
		/// </summary>
		/// <param name="_dirty"></param>
		public void SetDirty(bool _dirty)
		{
			// no change
			if (m_isDirty == _dirty)
			{
				return;
			}

			m_isDirty = _dirty;

			// if made dirty, invoke event
			if (m_isDirty)
			{
				OnDirty?.Invoke();
			}
		}

		/// <summary>
		/// Tries to spend the shell.
		/// </summary>
		/// <returns>True if the shell was spent, false if dry fired.</returns>
		public bool TrySpend()
		{
			// no change
			if (m_isSpent)
			{
				return false;
			}

			// dirty
			if (m_isDirty)
			{
				bool canDryFire = m_spendTries < s_maxSpendTries;
				bool shouldDryFire = UnityEngine.Random.value < s_dirtyDryFireChance;
				// dry fire
				if (canDryFire && shouldDryFire)
				{
					m_spendTries++;
					return false;
				}
			}


			m_isSpent = true;

			// invoke event
			OnSpend?.Invoke();

			return true;
		}

		/// <summary>
		/// <inheritdoc cref="GetShellString(ShellData)"/>
		/// </summary>
		/// <returns></returns>
		public string GetShellString()
		{
			return GetShellString(this);
		}

		/// <summary>
		/// Get a string representation of the shell.<br/>
		/// Spent/Live = S/L, Clean/Dirty = C/D, null = _
		/// </summary>
		/// <returns></returns>
		public static string GetShellString(ShellData _shell)
		{
			if (_shell == null)
			{
				return "_";
			}
			return (_shell.m_isSpent ? "S" : "L") + "" + (_shell.m_isDirty ? "D" : "C");
		}
	}

	/// <summary>
	/// Data for a clip of shells.
	/// </summary>
	[System.Serializable]
	public class ClipData
	{
		private int m_maxSize = 5;
		public int MaxSize { get { return m_maxSize; } }
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

		/// <summary>
		/// Get a string representation of the clip.<br/>
		/// [
		/// </summary>
		/// <param name="_clip"></param>
		/// <returns></returns>
		public static string GetClipString(ClipData _clip)
		{
			if (_clip == null)
			{
				return "_";
			}

			string clipString = "";
			for (int i = 0; i < _clip.MaxSize; i++)
			{
				clipString += ShellData.GetShellString(i < _clip.m_shells.Count ? _clip.m_shells[i] : null);
				clipString += i < _clip.MaxSize - 1 ? "|" : "";
			}
			return clipString;
		}
	}

	[Header("References")]
	public Transform m_gunParent;
	public Transform m_clip;
	public Transform m_shellPointChamber;
	public Transform m_shellPointMag;
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
	public float m_aimTime = 0.7916667f;
	public float m_unaimTime = 0.4583333f;
	[SerializeField, Utils.ReadOnly] private float m_aimAmount = 0.0f; public float AimAmount { get { return m_aimAmount; } }
	[Tooltip("How far into the aim amount till we are using the sights")] public float m_aimSightThreshold = 0.3f;
	public float m_aimFovMulti = 0.75f;
	public float m_aimFovSpeed = 50.0f;
	public float m_shootTime = 0.48f;
	public float m_dryFireTime = 0.25f;
	public float m_boltTime = 1.375f;
	public float m_shellEjectDelay = 0.4583333f;
	public float m_reloadTime = 2.583333f;
	public float m_checkChamberTime = 0.625f;
	public float m_uncheckChamberTime = 0.6666667f;

	[Header("State")]
	[SerializeField, Utils.ReadOnly] private bool m_canAim = true; public bool CanAim { get { return m_canAim; } }
	[SerializeField, Utils.ReadOnly] private bool m_isAiming = false; public bool IsAiming { get { return m_isAiming; } }
	[SerializeField, Utils.ReadOnly] private bool m_canUnAim = false; public bool CanUnAim { get { return m_canUnAim; } }
	[SerializeField, Utils.ReadOnly] private bool m_canShoot = true; public bool CanShoot { get { return m_canShoot; } }
	[SerializeField, Utils.ReadOnly] private bool m_isShooting = false; public bool IsShooting { get { return m_isShooting; } }
	[SerializeField, Utils.ReadOnly] private bool m_canBolt = true; public bool CanBolt { get { return m_canBolt; } }
	[SerializeField, Utils.ReadOnly] private bool m_isBolting = false; public bool IsBolting { get { return m_isBolting; } }
	[SerializeField, Utils.ReadOnly] private bool m_canReload = true; public bool CanReload { get { return m_canReload; } }
	[SerializeField, Utils.ReadOnly] private bool m_isReloading = false; public bool IsReloading { get { return m_isReloading; } }
	[SerializeField, Utils.ReadOnly] private bool m_canCheckChamber = true; public bool CanCheckChamber { get { return m_canCheckChamber; } }
	[SerializeField, Utils.ReadOnly] private bool m_isCheckingChamber = false; public bool IsCheckingChamber { get { return m_isCheckingChamber; } }
	//! @NOTE: Not Done yet vvv
	[SerializeField, Utils.ReadOnly] private bool m_canInspectWeapon = true; public bool CanInspectWeapon { get { return m_canInspectWeapon; } }
	[SerializeField, Utils.ReadOnly] private bool m_isInspectingWeapon = false; public bool IsInspectingWeapon { get { return m_isInspectingWeapon; } }

	[SerializeField, Utils.ReadOnly] private bool m_canAutoBolt = false; public bool CanAutoBolt { get { return m_canAutoBolt; } }

	[Header("Prefabs")]
	public GameObject m_shellPrefab;
	public GameObject m_fxGunShot;
	public GameObject m_fxSmokeTrail;

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
		// nothing
	}

	// Update is called once per frame
	void Update()
	{
		// null checks
		if (Player.Instance == null)
		{
			return;
		}

		ClipModel clipModel = FindObjectOfType<ClipModel>();
		if (clipModel != null)
		{
			clipModel.SetClip(m_currentClip);
		}

		// aim (continuous)
		UpdateAim();
		// // check chamber (continuous)
		// UpdateCheckChamber();

		// shoot
		if (InputManager.PlayerGun.Shoot.WasPerformedThisFrame())
		{
			StartCoroutine(TryShoot());
		}
		// auto bolt after shoot
		if (InputManager.PlayerGun.Shoot.IsPressed() == false && m_canAutoBolt)
		{
			StartCoroutine(TryBolt());
		}
		// reload
		if (InputManager.PlayerGun.Reload.WasPerformedThisFrame())
		{
			StartCoroutine(TryReload());
		}
		// force bolt
		if (InputManager.PlayerGun.Bolt.WasPerformedThisFrame())
		{
			StartCoroutine(TryBolt());
		}
		// check chamber
		if (InputManager.PlayerGun.CheckChamber.IsPressed())
		{
			StartCoroutine(TryCheckChamber());
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
		else if ((!wantsToAim && m_canUnAim) || m_isBolting || m_isReloading || m_isCheckingChamber)
		{
			m_canAim = true;
			m_isAiming = false;

			// begin unaim animation
			m_animator.SetBool("Aiming", false);

			// decrement aim amount
			m_aimAmount = Mathf.Clamp01(m_aimAmount - Time.deltaTime / (m_unaimTime * 0.5f));

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
		Debug.DrawRay(cam.transform.position, m_aimDir * (m_shellInChamber != null ? m_shellInChamber.Range : 100f), useAimPoint ? Color.green : Color.red);

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

	// private void UpdateCheckChamber()
	// {

	// }

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
		m_canBolt = false;
		m_canReload = false;
		m_canCheckChamber = false;

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
			m_canCheckChamber = true;
		}
		// spent shell, dry fire
		else if (m_shellInChamber.IsSpent)
		{
			// audio
			AudioManager.SpawnSound<AutoSound_GunEmpty>(m_firePoint.position); // Temp sound

			// wait for dry fire time
			yield return new WaitForSeconds(m_dryFireTime);
		}
		// fireable shell
		else if (m_shellInChamber != null)
		{
			// Try to spend the shell
			if (!m_shellInChamber.TrySpend())
			{
				// it was dirty, so dry fire

				// audio
				AudioManager.SpawnSound<AutoSound_GunEmpty>(m_firePoint.position); // Temp sound

				// wait for dry fire time
				yield return new WaitForSeconds(m_dryFireTime);

				// can shoot again
				m_canShoot = true;
				m_canCheckChamber = true;
			}
			// it was spent successfully
			else
			{
				// are we aiming enough to use sights
				if (m_isAiming && m_aimAmount > m_aimSightThreshold)
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
				RaycastHit[] hits = Physics.RaycastAll(cam.transform.position, m_aimDir, m_shellInChamber.Range, ~ignoreMask);
				// sort hits by distance
				System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

				// loop through hits
				foreach (RaycastHit hit in hits)
				{
					// generate damage info
					Health.DamageInfo damageInfo = new Health.DamageInfo(m_shellInChamber.Damage, gameObject, m_firePoint.position, hit.point, hit.normal, m_shellInChamber.HitForce);

					// get health component in child&/parent
					Health health = hit.collider.GetComponentInParent<Health>() ?? hit.collider.GetComponentInChildren<Health>();
					if (health != null)
					{
						// damage health
						health.Damage(damageInfo);
					}

					// get rigidbody
					Rigidbody rb = hit.collider.attachedRigidbody;
					if (rb != null)
					{
						// add force
						Vector3 force = damageInfo.Direction * damageInfo.m_hitForce;
						rb.AddForceAtPosition(force, damageInfo.m_hitPoint, ForceMode.Impulse);
					}

					// break after first hit (for now)
					//@TODO: implement piercing? e.g. StoppingPower stat
					break;
				}

				// wait for shoot time
				yield return new WaitForSeconds(m_shootTime);
			}
		}

		m_isShooting = false;
		m_canReload = true;
		m_canBolt = true;

		// auto bolt if spent shell
		if (m_shellInChamber != null && m_shellInChamber.IsSpent)
		{
			m_canAutoBolt = true;
		}
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
		m_canCheckChamber = false;
		m_canAutoBolt = false;

		m_aimAmount = 0.0f;

		m_animator.SetTrigger("Bolt");

		// audio
		AutoSound reloadSound = AudioManager.SpawnSound<AutoSound_GunBolt>(m_shellPointChamber.position);
		reloadSound.transform.parent = m_shellPointChamber;
		// spawn ejected shell after a delay
		StartCoroutine(CosmeticEjectShell(m_shellEjectDelay));

		// spawn new mag shell
		GameObject shell = null;
		if (m_currentClip?.Top() != null)
		{
			shell = InstantiateCosmeticShell(m_currentClip.Top(), m_shellPointMag, _parent: true);
		}

		// wait for bolt time
		yield return new WaitForSeconds(m_boltTime);

		// re-chamber using top shell in clip
		m_shellInChamber = m_currentClip?.Top(_remove: true) ?? null;

		// destroy chambered shell
		if (shell != null)
		{
			Destroy(shell);
		}

		m_canShoot = true;
		m_canBolt = true;
		m_canReload = true;
		m_isBolting = false;
		m_canCheckChamber = true;
	}

	/// <summary>
	/// Ejects a shell from the gun. <br/>
	/// !Call before changing the current shell in chamber! <br/>
	/// @TODO: Implement shell types
	/// </summary>
	/// <param name="delay"></param>
	/// <returns></returns>
	private IEnumerator CosmeticEjectShell(float delay)
	{
		// if no shell, return
		if (m_shellInChamber == null)
		{
			yield break;
		}

		yield return new WaitForSeconds(delay);

		GameObject shell = InstantiateCosmeticShell(m_shellInChamber, m_shellPointChamber);
		//AutoSound shellSound = AudioManager.SpawnSound<AutoSound_EjectShell>(m_shellPoint.position);
		Rigidbody shellRb = shell.GetComponent<Rigidbody>();
		if (shellRb != null)
		{
			// right and up
			shellRb.velocity = m_shellPointChamber.right * 2f + m_shellPointChamber.up * 2f;
			// add player vel
			shellRb.velocity += Player.Instance.m_movement.CalcVelocity();
			// random spin
			shellRb.angularVelocity = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)) * 10f;
		}
	}

	private GameObject InstantiateCosmeticShell(ShellData _toCopy, Transform _pos, bool _parent = false)
	{
		GameObject shell = Instantiate(m_shellPrefab, _pos.position, _pos.rotation, _parent ? _pos : null);

		// no physics if parented
		Rigidbody shellRb = shell.GetComponent<Rigidbody>();
		if (shellRb != null && _parent)
		{
			Destroy(shellRb);
			// destroy all colliders
			Collider[] colliders = shell.GetComponentsInChildren<Collider>();
			for (int i = colliders.Length - 1; i >= 0; i--)
			{
				Destroy(colliders[i]);
			}
		}

		// update visuals
		MosinShell mosinShell = shell.GetComponent<MosinShell>();
		mosinShell.SetShellData(_toCopy);

		return shell;
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
		m_canCheckChamber = false;
		m_canAutoBolt = false;

		// do we have any clips
		if (m_totalClips.Count > 0)
		{
			m_animator.SetTrigger("Reload");

			// audio
			AutoSound reloadSound = AudioManager.SpawnSound<AutoSound_GunReload>(m_shellPointChamber.position);
			reloadSound.transform.parent = m_shellPointChamber;

			// if there is a shell in the chamber, it gets ejected
			if (m_shellInChamber != null)
			{
				// spawn shell after a delay
				StartCoroutine(CosmeticEjectShell(m_shellEjectDelay));
				m_shellInChamber = null;
			}

			int remainingShells = m_currentClip?.m_shells.Count ?? 0;
			int moved = 0;
			// if no clip, use first clip 
			if (remainingShells == 0)
			{
				m_currentClip = m_totalClips[0];
				moved = m_currentClip.m_shells.Count;
				m_totalClips.RemoveAt(0);
			}
			else
			{
				ClipData nextClip = m_totalClips[0];
				int neededShells = m_currentClip.MaxSize - remainingShells;
				// transfer shells from next clip to current clip
				for (int i = 0; i < neededShells; i++)
				{
					ShellData shell = nextClip.Top(_remove: true);
					if (shell != null)
					{
						m_currentClip.Add(shell);
						moved++;
					}
				}

				BalanceClips();
			}

			// spawn in cosmetic shells instead of animating
			List<GameObject> tempShells = new List<GameObject>();
			for (int i = 0; i < m_clipShells.Count; i++)
			{
				Transform shell = m_clipShells[i];
				shell.localScale = Vector3.zero;

				if (i >= m_clipShells.Count - moved)
				{
					ShellData shellData = m_currentClip.m_shells[i];

					GameObject tempShell = InstantiateCosmeticShell(shellData, shell.parent, _parent: true);
					tempShells.Add(tempShell);

					tempShell.transform.position = shell.position;
					// subtract 90 degrees to x rotation
					tempShell.transform.rotation = shell.rotation * Quaternion.Euler(-90f, 0f, 0f);
					tempShell.transform.localScale = Vector3.one;

					StartCoroutine(CosmeticTrackShells(tempShell, shell, m_reloadTime));
				}
			}

			// wait for reload time
			yield return new WaitForSeconds(m_reloadTime);

			// destroy temp shells
			foreach (GameObject tempShell in tempShells)
			{
				Destroy(tempShell);
			}
			tempShells.Clear();

			// re-chamber using top shell in clip
			m_shellInChamber = m_currentClip?.Top(_remove: true) ?? null;
		}
		// out of ammo or clip is full
		else
		{
			AudioManager.SpawnSound<AutoSound_GunEmpty>(m_firePoint.position); // Temp sound
		}

		m_canShoot = true;
		m_canBolt = true;
		m_canReload = true;
		m_isReloading = false;
		m_canCheckChamber = true;
	}

	/// <summary>
	/// Coroutine for temp shells to track the animation.
	/// </summary>
	/// <returns></returns>
	private IEnumerator CosmeticTrackShells(GameObject _shell, Transform _pos, float _time)
	{
		// during the time, track the shell to the position
		float timeStart = Time.time;
		while (Time.time - timeStart < _time)
		{
			// null check
			if (_shell == null)
			{
				yield break;
			}

			// a little forward of the position
			_shell.transform.position = _pos.position /* + _shell.forward * 0.1f */;
			yield return null;
		}
	}

	private IEnumerator TryCheckChamber()
	{
		if (!m_canCheckChamber)
		{
			yield break;
		}

		m_isCheckingChamber = true;
		m_canShoot = false;
		m_canBolt = false;
		m_canReload = false;
		m_canCheckChamber = false;

		// spawn shell 
		GameObject shell = null;
		if (m_shellInChamber != null)
		{
			shell = InstantiateCosmeticShell(m_shellInChamber, m_shellPointChamber, _parent: true);
		}

		// animation
		m_animator.SetBool("Check_Chamber", true);

		// audio
		AutoSound openBoltSound = AudioManager.SpawnSound<AutoSound_GunBolt>(m_shellPointChamber.position); // Temp sound
		openBoltSound.transform.parent = m_shellPointChamber;

		// wait for check chamber time
		yield return new WaitForSeconds(m_checkChamberTime);

		// wait for user to release the check chamber button
		while (InputManager.PlayerGun.CheckChamber.IsPressed())
		{
			yield return null;
		}

		// animation
		m_animator.SetBool("Check_Chamber", false);

		// audio
		AutoSound closeBoltSound = AudioManager.SpawnSound<AutoSound_GunBolt>(m_shellPointChamber.position); // Temp sound
		closeBoltSound.transform.parent = m_shellPointChamber;

		// wait for uncheck chamber time
		yield return new WaitForSeconds(m_uncheckChamberTime);

		// destroy shell
		if (shell != null)
		{
			Destroy(shell);
		}

		m_canShoot = true;
		m_canBolt = true;
		m_canReload = true;
		m_isCheckingChamber = false;
		m_canCheckChamber = true;
	}

	/// <summary>
	/// Balances available clips so that all clips are full.
	/// </summary>
	private void BalanceClips()
	{
		// sort clips by size (biggest first)
		//m_totalClips.Sort((a, b) => b.m_shells.Count.CompareTo(a.m_shells.Count));

		// empty all shells
		List<ShellData> shells = new List<ShellData>();
		foreach (ClipData clip in m_totalClips)
		{
			shells.AddRange(clip.m_shells);
			clip.m_shells.Clear();
		}

		// fill all clips with shells
		foreach (ClipData clip in m_totalClips)
		{
			for (int i = 0; i < clip.MaxSize; i++)
			{
				if (shells.Count > 0)
				{
					clip.Add(shells[0]);
					shells.RemoveAt(0);
				}
			}
		}

		// add remaining shells to new clips
		while (shells.Count > 0)
		{
			ClipData clip = new ClipData(0);
			for (int i = 0; i < clip.MaxSize; i++)
			{
				if (shells.Count > 0)
				{
					clip.Add(shells[0]);
					shells.RemoveAt(0);
				}
			}
			m_totalClips.Add(clip);
		}

		// sort clips by size (biggest first)
		m_totalClips.Sort((a, b) => b.m_shells.Count.CompareTo(a.m_shells.Count));

		// remove any empty clips
		for (int i = m_totalClips.Count - 1; i >= 0; i--)
		{
			if (m_totalClips[i].m_shells.Count == 0)
			{
				m_totalClips.RemoveAt(i);
			}
		}
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
			// debug ammo, Spent/Live = S/L, Clean/Dirty = C/D, Empty = _
			// chamber
			EditorGUILayout.LabelField("Shell in Chamber", ShellData.GetShellString(script.m_shellInChamber));
			// current clip
			EditorGUILayout.LabelField("Current Clip", ClipData.GetClipString(script.m_currentClip));
			// total clips
			EditorGUILayout.LabelField("Spares", script.m_totalClips.Count.ToString());
			foreach (ClipData clip in script.m_totalClips)
			{
				EditorGUILayout.LabelField("Clip", ClipData.GetClipString(clip));
			}

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