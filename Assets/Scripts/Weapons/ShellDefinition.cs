using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Scriptable object to define a shell.
/// </summary>
[CreateAssetMenu(fileName = "ShellDefinition", menuName = "Shell Definition", order = 1)]
public class ShellDefinition : ScriptableObject
{
	[Header("Data")]
	[SerializeField] protected string m_name = "Shell";

	[SerializeField] protected float m_dirtyChance = 0.5f;
	[SerializeField] protected float m_dirtyDryFireChance = 0.5f;
	[SerializeField] protected float m_maxSpendTries = 5;

	[SerializeField] protected float m_damage = 100.0f;
	public float Damage { get { return m_damage; } }
	[SerializeField] protected float m_range = 500.0f;
	public float Range { get { return m_range; } }
	[SerializeField] protected float m_hitForce = 1.0f;
	public float HitForce { get { return m_hitForce; } }
	[SerializeField] protected bool m_isDirty = false;
	public bool IsDirty { get { return m_isDirty; } }
	[SerializeField] protected bool m_isSpent = false;
	public bool IsSpent { get { return m_isSpent; } }

	[SerializeField] protected int m_spendTries = 0;
	
	[Header("Colors")]
	[SerializeField] protected Color m_dirtyColor = Color.red;
	public Color DirtyColor { get { return m_dirtyColor; } }
	[SerializeField] protected Color m_brassColor = new Color(0.8f, 0.6f, 0.2f); // brass
	public Color BrassColor { get { return m_brassColor; } }
	[SerializeField] protected Color m_copperColor = new Color(0.72f, 0.45f, 0.2f); // copper
	public Color CopperColor { get { return m_copperColor; } }

	[Header("References")]
	[SerializeField] protected GameObject m_shellPrefab;

	public System.Action OnDirty;
	public System.Action OnSpend;

	/// <summary>
	/// Protected constructor to prevent instantiation.
	/// </summary>
	protected ShellDefinition()
	{
	}

	protected void Copy(ShellDefinition _toCopy)
	{
		m_name = _toCopy.m_name;
		m_dirtyChance = _toCopy.m_dirtyChance;
		m_dirtyDryFireChance = _toCopy.m_dirtyDryFireChance;
		m_maxSpendTries = _toCopy.m_maxSpendTries;
		m_damage = _toCopy.m_damage;
		m_range = _toCopy.m_range;
		m_hitForce = _toCopy.m_hitForce;
		m_isDirty = _toCopy.m_isDirty;
		m_isSpent = _toCopy.m_isSpent;
		m_spendTries = _toCopy.m_spendTries;
		m_dirtyColor = _toCopy.m_dirtyColor;
		m_brassColor = _toCopy.m_brassColor;
		m_copperColor = _toCopy.m_copperColor;
		m_shellPrefab = _toCopy.m_shellPrefab;
	}

	/// <summary>
	/// Create a new shell with the same data as this shell.
	/// </summary>
	/// <returns></returns>
	public ShellDefinition GetCopy()
	{
		ShellDefinition copy = ScriptableObject.CreateInstance<ShellDefinition>();
		copy.Copy(this);
		return copy;
	}

	/// <summary>
	/// Create a new shell with random data.
	/// </summary>
	/// <returns></returns>
	public ShellDefinition GetRandomInstance()
	{
        ShellDefinition copy = this.GetCopy();
        // randomize dirty
        copy.m_isDirty = UnityEngine.Random.value < m_dirtyChance;

        return copy;
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
			bool canDryFire = m_spendTries < m_maxSpendTries;
			bool shouldDryFire = UnityEngine.Random.value < m_dirtyDryFireChance;
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
	/// <inheritdoc cref="GetShellString(ShellDefinition)"/>
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
	public static string GetShellString(ShellDefinition _shell)
	{
		if (_shell == null)
		{
			return "_";
		}
		return (_shell.m_isSpent ? "S" : "L") + "" + (_shell.m_isDirty ? "D" : "C");
	}

	/// <inheritdoc cref="InstantiateCosmeticShell(GameObject, ShellDefinition, Transform, bool)"/>
	public GameObject InstantiateCosmeticShell(Transform _pos, bool _parent = false)
	{
		return InstantiateCosmeticShell(m_shellPrefab, this, _pos, _parent);
	}

	/// <summary>
	/// Instantiates a cosmetic shell at the given position.
	/// </summary>
	/// <param name="_prefab">What prefab to instantiate.</param>
	/// <param name="_toCopy">The data to copy from.</param>
	/// <param name="_pos">The position to instantiate at.</param>
	/// <param name="_parent">Should the shell be parented to the given transform?</param>
	/// <returns></returns>
	public static GameObject InstantiateCosmeticShell(GameObject _prefab, ShellDefinition _toCopy, Transform _pos, bool _parent = false)
	{
		GameObject shellObject = Instantiate(_prefab, _pos.position, _pos.rotation, _parent ? _pos : null);

		// no physics if parented
		Rigidbody shellRb = shellObject.GetComponent<Rigidbody>();
		if (shellRb != null && _parent)
		{
			shellRb.isKinematic = true; // kinematic first to prevent physics
			Destroy(shellRb);
			// // destroy all colliders
			// Collider[] colliders = shell.GetComponentsInChildren<Collider>();
			// for (int i = colliders.Length - 1; i >= 0; i--)
			// {
			// 	Destroy(colliders[i]);
			// }
		}

		// dont generate new shell if copying
		if (_toCopy != null && shellObject.TryGetComponent(out Interactable_AmmoShell interactShell))
		{
			interactShell.GenerateRandomShell = false;
		}
		// update visuals
		if (shellObject.TryGetComponent(out DisplayShell displayShell))
		{
			displayShell.SetShell(_toCopy);
		}

		return shellObject;
	}
}