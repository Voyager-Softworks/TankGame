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
	[Header("References")]
	#region References
	public Transform m_gunParent;
	public Transform m_clip;
	public Transform m_shellPointChamber;
	public Transform m_shellPointMag;
	public Transform m_firePoint;
	public Transform m_aimPoint;
	public Animator m_animator;
	public Light m_flashlight;

	public DisplayClip m_displayClip;

	public ShellDefinition m_ammoType;
	public ClipDefinition m_defaultClip;
	#endregion

	[Header("Flashlight")]
	#region Flashlight
	[SerializeField] private bool m_flashlightAllowed = true;
	public bool FlashlightAllowed { get { return m_flashlightAllowed; } }
	[SerializeField] private float m_flashlightIntensity = 1.0f;
	public float FlashlightIntensity { get { return m_flashlightIntensity; } }
	[SerializeField] private float m_flashlightRange = 50.0f;
	public float FlashlightRange { get { return m_flashlightRange; } }
	[SerializeField] private float m_flashlightSpotAngle = 30.0f;
	public float FlashlightSpotAngle { get { return m_flashlightSpotAngle; } }
	#endregion

	[Header("Ammo")]
	#region Ammo
	[SerializeField] private int m_startingClips = 20;
	[NonSerialized] private ShellDefinition m_shellInChamber = null;
	public ShellDefinition ShellInChamber { get { return m_shellInChamber; } }
	[NonSerialized] private ClipDefinition m_internalClip = null;
	public ClipDefinition InternalClip { get { return m_internalClip; } }
	[NonSerialized] private List<ClipDefinition> m_spareClips = null;
	public List<ClipDefinition> SpareClips { get { return m_spareClips; } }
	[NonSerialized] private List<ShellDefinition> m_spareShells = null;
	public List<ShellDefinition> SpareShells { get { return m_spareShells; } }
	#endregion
	
	[Header("Timers")]
	#region Timers
	public float m_activateTime = 0.5f;
	public float m_deactivateTime = 0.5f;
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
	#endregion

	[Header("State")]
	#region State
	public bool m_wantsToBeActive = false; // Controlled externally
	[SerializeField, Utils.ReadOnly] private bool m_canActivate = true; public bool CanActivate { get { return m_canActivate; } }
	[SerializeField, Utils.ReadOnly] private bool m_isActivated = false; public bool IsActivated { get { return m_isActivated; } }
	[SerializeField, Utils.ReadOnly] private bool m_canDeactivate = false; public bool CanDeactivate { get { return m_canDeactivate; } }
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
	[SerializeField, Utils.ReadOnly] private bool m_canAutoBolt = false; public bool CanAutoBolt { get { return m_canAutoBolt; } }
	[SerializeField, Utils.ReadOnly] private bool m_canCheckClips = true; public bool CanCheckClips { get { return m_canCheckClips; } }
	//! @NOTE: Not Done yet vvv
	[SerializeField, Utils.ReadOnly] private bool m_canInspectWeapon = true; public bool CanInspectWeapon { get { return m_canInspectWeapon; } }
	[SerializeField, Utils.ReadOnly] private bool m_isInspectingWeapon = false; public bool IsInspectingWeapon { get { return m_isInspectingWeapon; } }
	#endregion

	[Header("Prefabs")]
	#region Prefabs
	public GameObject m_shellPrefab;
	public GameObject m_fxGunShot;
	public GameObject m_fxSmokeTrail;
	#endregion

	[Header("Spare Clip Display")]
	#region Tracking
	[SerializeField, Utils.ReadOnly] private Vector3 m_displayClipOriginalPos = Vector3.zero;
	[SerializeField] private Vector3 m_displayHideOffset = new Vector3(0.0f, -1.0f, 0.0f);
	[SerializeField] private float m_displayClipShowTime = 0.5f;
	[SerializeField] private float m_displayClipHideTime = 0.25f;
	#endregion

	[Header("Debug")]
	private Vector3 m_aimDir = Vector3.zero;

	#region Unity Methods
	private void Awake()
	{
		// create lists
		m_spareClips = new List<ClipDefinition>();
		m_spareShells = new List<ShellDefinition>();
		m_internalClip = m_defaultClip.GetRandomInstance(0); //empty clip

		// add starting clips
		for (int i = 0; i < m_startingClips; i++)
		{
			m_spareClips.Add(m_defaultClip.GetRandomInstance());
		}

		// save original clip position
		m_displayClipOriginalPos = Player.Instance.m_clipDisplayParent.transform.localPosition;
		// hide it
		Player.Instance.m_clipDisplayParent.SetActive(false);
		Player.Instance.m_clipDisplayParent.transform.localPosition = m_displayHideOffset;
		// hide template
		Player.Instance.m_clipDisplayTemplate.SetActive(false);

		UpdateFlashlightSettings();
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

		// aim (continuous)
		UpdateAim();
		// // check chamber (continuous)
		// UpdateCheckChamber();

		// activate
		if (m_wantsToBeActive)
		{
			StartCoroutine(Activate());
		}
		// deactivate
		else if (!m_wantsToBeActive && !(m_isShooting || m_isBolting || m_isReloading || m_isCheckingChamber))
		{
			StartCoroutine(Deactivate());
		}
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
		// check clips
		if (InputManager.PlayerGun.CheckClips.IsPressed())
		{
			// check clips
			StartCoroutine(TryCheckClips());
		}
	}
	#endregion

	#region Flashlight
	/// <summary>
	/// Updates the flashlight with the current settings.
	/// </summary>
	private void UpdateFlashlightSettings()
	{
		m_flashlight.gameObject.SetActive(m_flashlightAllowed); // just disable the gameobject, as the component is controlled by other parts
		m_flashlight.intensity = m_flashlightIntensity;
		m_flashlight.range = m_flashlightRange;
		m_flashlight.spotAngle = m_flashlightSpotAngle;
	}
	#endregion

	#region Active
	private IEnumerator Activate()
	{
		if (!m_canActivate)
		{
			yield break;
		}

		m_canActivate = false;
		m_canDeactivate = true;

		// begin activate animation
		m_animator.SetBool("Active", true);

		// enable flashlight
		m_flashlight.enabled = true;

		// wait for activate time
		yield return new WaitForSeconds(m_activateTime);

		m_isActivated = true;

		// reset state
		m_canAim = true;
		m_canShoot = true;
		m_canBolt = true;
		m_canReload = true;
		m_canCheckChamber = true;
		m_canAutoBolt = false;
		m_canCheckClips = true;
		m_canInspectWeapon = true;
	}

	private IEnumerator Deactivate()
	{
		if (!m_canDeactivate)
		{
			yield break;
		}

		// cant do anything while deactivating, except activate
		m_canActivate = true;
		m_canDeactivate = false;
		m_canAim = false;
		m_canUnAim = false;
		m_canShoot = false;
		m_canBolt = false;
		m_canReload = false;
		m_canCheckChamber = false;
		m_canAutoBolt = false;
		m_canCheckClips = false;
		m_canInspectWeapon = false;

		// begin deactivate animation
		m_animator.SetBool("Active", false);

		// wait for deactivate time
		yield return new WaitForSeconds(m_deactivateTime);

		// disable flashlight
		m_flashlight.enabled = false;
	}
	
	#endregion

	#region Aim
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
		else if ((!wantsToAim && m_canUnAim) || m_isBolting || m_isReloading || m_isCheckingChamber || !m_wantsToBeActive)
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
	#endregion

	#region Shoot
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
	#endregion

	#region Bolt
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
		if (m_internalClip?.TopShell() != null)
		{
			shell = ShellDefinition.InstantiateCosmeticShell(m_shellPrefab, m_internalClip.TopShell(), m_shellPointMag, _parent: true);
		}

		// wait for bolt time
		yield return new WaitForSeconds(m_boltTime);

		// re-chamber using top shell in clip
		m_shellInChamber = m_internalClip?.TopShell(_remove: true) ?? null;

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
	#endregion

	#region Eject
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
		ShellDefinition copyShell = m_shellInChamber.GetCopy();

		yield return new WaitForSeconds(delay);

		// spawn shell
		GameObject shell = ShellDefinition.InstantiateCosmeticShell(m_shellPrefab, copyShell, m_shellPointChamber);
		// add force
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
	#endregion

	#region Reload
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
		if (m_spareClips.Count > 0)
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

			// get next clip & make temp clip to move needed into
			ClipDefinition nextClip = m_spareClips[0];
			ClipDefinition tempClip = nextClip.GetRandomInstance(0);
			// calc needed shells
			int remainingShells = m_internalClip?.GetShellCount() ?? 0;
			int neededShells = m_internalClip.MaxSize - remainingShells;
			// transfer shells from next clip to temp clip (in same order)
			for (int i = 0; i < neededShells; i++)
			{
				ShellDefinition shell = nextClip.BottomShell(_remove: true); // remove from bottom of clip to keep order
				if (shell != null)
				{
					tempClip.AddShell(shell);
				}
			}
			BalanceAmmo(); // ensure all next clips are full

			// spawn in cosmetic shells instead of animating
			m_displayClip.SetClip(tempClip);

			// wait for reload time
			yield return new WaitForSeconds(m_reloadTime);

			// destroy temp shells
			m_displayClip.SetClip(null);

			// transfer shells from temp clip to internal clip (in same order)
			for (int i = tempClip.GetShellCount() - 1; i >= 0; i--)
			{
				ShellDefinition shell = tempClip.BottomShell(_remove: true); // remove from clip to keep order
				if (shell != null)
				{
					bool didAdd = m_internalClip.AddShell(shell);
					if (!didAdd)
					{
						// didn't add, keep in spare shells
						m_spareShells.Add(shell);
					}
				}
			}

			// re-chamber using top shell in clip
			m_shellInChamber = m_internalClip?.TopShell(_remove: true) ?? null;
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
	#endregion

	#region Check Chamber
	/// <summary>
	/// Check the chamber for a shell.
	/// </summary>
	/// <returns></returns>
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
			shell = ShellDefinition.InstantiateCosmeticShell(m_shellPrefab, m_shellInChamber, m_shellPointChamber, _parent: true);
		}

		// animation
		m_animator.SetBool("Check_Chamber", true);

		// audio
		AutoSound openBoltSound = AudioManager.SpawnSound<AutoSound_GunBoltOpen>(m_shellPointChamber.position); // Temp sound
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
		AutoSound closeBoltSound = AudioManager.SpawnSound<AutoSound_GunBoltClose>(m_shellPointChamber.position); // Temp sound
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
	#endregion

	#region Check Clips
	/// <summary>
	/// Pulls the clips up on screen, like a diegetic UI.
	/// </summary>
	/// <returns></returns>
	private IEnumerator TryCheckClips()
	{
		if (!m_canCheckClips)
		{
			yield break;
		}

		m_canCheckClips = false;

		// show clip parent
		Player.Instance.m_clipDisplayParent.SetActive(true);

		// spawn in cosmetic clips for each spare clip
		Vector3 clipOffset = Player.Instance.m_clipDisplayTemplate.transform.localPosition;
		List<GameObject> clipObjects = new List<GameObject>();
		for (int i = 0; i < m_spareClips.Count; i++)
		{
			// make empty gameobject at spawn point
			GameObject clipObject = new GameObject("Clip " + i);
			clipObject.transform.parent = Player.Instance.m_clipDisplayParent.transform;
			clipObject.transform.localPosition = clipOffset * (i + 1);
			clipObject.transform.rotation = Player.Instance.m_clipDisplayTemplate.transform.rotation;
			clipObjects.Add(clipObject);

			ClipDefinition clip = m_spareClips[i];
			GameObject displayClip = clip.InstantiateCosmeticClip(clipObject.transform, _parent: true);
		}

		// move the clip parent to the player's view slowly
		float time = 0.0f;
		float duration = m_displayClipShowTime;
		Vector3 startPos = Player.Instance.m_clipDisplayParent.transform.localPosition;
		Vector3 endPos = m_displayClipOriginalPos;
		while (time < duration && InputManager.PlayerGun.CheckClips.IsPressed())
		{
			time += Time.deltaTime;
			Player.Instance.m_clipDisplayParent.transform.localPosition = Vector3.Lerp(startPos, endPos, time / duration);
			yield return null;
		}

		// wait for user to release the check clips button
		while (InputManager.PlayerGun.CheckClips.IsPressed())
		{
			yield return null;
		}

		// move the clip parent back to the hide position quickly
		time = 0.0f;
		duration = m_displayClipHideTime;
		startPos = Player.Instance.m_clipDisplayParent.transform.localPosition;
		endPos = m_displayClipOriginalPos + m_displayHideOffset;
		while (time < duration && !InputManager.PlayerGun.CheckClips.IsPressed())
		{
			time += Time.deltaTime;
			Player.Instance.m_clipDisplayParent.transform.localPosition = Vector3.Lerp(startPos, endPos, time / duration);
			yield return null;
		}

		// destroy clip objects
		for (int i = clipObjects.Count - 1; i >= 0; i--)
		{
			Destroy(clipObjects[i]);
		}

		// hide clip parent
		Player.Instance.m_clipDisplayParent.SetActive(false);

		m_canCheckClips = true;
	}
	#endregion

	#region Ammo
	/// <summary>
	/// Balances available clips so that the next usable clip is always the most full.
	/// </summary>
	private void BalanceAmmo()
	{
		// sort clips by size (biggest first)
		//m_totalClips.Sort((a, b) => b.m_shells.Count.CompareTo(a.m_shells.Count));

		// empty all shells
		List<ShellDefinition> shells = new List<ShellDefinition>();
		foreach (ClipDefinition clip in m_spareClips)
		{
			while (clip.GetShellCount() > 0)
			{
				shells.Add(clip.TopShell(_remove: true));
			}
		}

		// add spare shells
		foreach (ShellDefinition shell in m_spareShells)
		{
			shells.Add(shell);
		}
		m_spareShells.Clear();

		// fill all clips with shells
		foreach (ClipDefinition clip in m_spareClips)
		{
			for (int i = 0; i < clip.MaxSize; i++)
			{
				if (shells.Count > 0)
				{
					clip.AddShell(shells[0]);
					shells.RemoveAt(0);
				}
			}
		}

		// add remaining shells to new clips
		while (shells.Count > 0)
		{
			ClipDefinition clip = m_defaultClip.GetRandomInstance(0);
			for (int i = 0; i < clip.MaxSize; i++)
			{
				if (shells.Count > 0)
				{
					clip.AddShell(shells[0]);
					shells.RemoveAt(0);
				}
			}
			m_spareClips.Add(clip);
		}

		// sort clips by size (biggest first)
		m_spareClips.Sort((a, b) => b.GetShellCount().CompareTo(a.GetShellCount()));

		// remove any empty clips
		for (int i = m_spareClips.Count - 1; i >= 0; i--)
		{
			if (m_spareClips[i].GetShellCount() == 0)
			{
				m_spareClips.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// Add a spare shell and balance ammo.
	/// </summary>
	/// <param name="_shell"></param>
	public void AddSpareShell(ShellDefinition _shell)
	{
		m_spareShells.Add(_shell);

		// balance ammo
		BalanceAmmo();
	}

	/// <summary>
	/// Add a spare clip and balance ammo.
	/// </summary>
	/// <param name="_clip"></param>
	public void AddSpareClip(ClipDefinition _clip)
	{
		m_spareClips.Add(_clip);

		// balance ammo
		BalanceAmmo();
	}
	#endregion

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
			EditorGUILayout.LabelField("Shell in Chamber", ShellDefinition.GetShellString(script.m_shellInChamber));
			// current clip
			EditorGUILayout.LabelField("Current Clip", ClipDefinition.GetClipString(script.m_internalClip));
			// spare clips
			EditorGUILayout.LabelField("Spare Clips", script.m_spareClips?.Count.ToString());
			if (script.m_spareClips != null)
			{
				foreach (ClipDefinition clip in script.m_spareClips)
				{
					EditorGUILayout.LabelField("Clip", ClipDefinition.GetClipString(clip));
				}
			}
			// spare shells
			EditorGUILayout.LabelField("Spare Shells", script.m_spareShells?.Count.ToString());
			if (script.m_spareShells != null)
			{
				string shells = string.Join("|", script.m_spareShells.ConvertAll(shell => ShellDefinition.GetShellString(shell)).ToArray());
				EditorGUILayout.LabelField("Shells", shells);
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