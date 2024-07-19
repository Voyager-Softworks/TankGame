using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scriptable object to define a shell.
/// </summary>
[CreateAssetMenu(fileName = "ShellDefinition", menuName = "Shell Definition", order = 1)]
public class ShellDefinition : ScriptableObject
{
	[SerializeField] protected Color DIRTY_COLOR = Color.red;
	[SerializeField] protected Color BRASS_COLOR = new Color(0.8f, 0.6f, 0.2f); // brass
	[SerializeField] protected Color COPPER_COLOR = new Color(0.72f, 0.45f, 0.2f); // copper
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

	public System.Action OnDirty;
	public System.Action OnSpend;

	public ShellDefinition()
	{
		// all values are initialized to default

		// randomize dirty
		//m_isDirty = UnityEngine.Random.value < s_dirtyChance;
	}

	/// <summary>
	/// Copy constructor.
	/// </summary>
	/// <param name="_toCopy"></param>
	public ShellDefinition(ShellDefinition _toCopy)
	{
		m_damage = _toCopy.m_damage;
		m_range = _toCopy.m_range;
		m_hitForce = _toCopy.m_hitForce;
		m_isDirty = _toCopy.m_isDirty;
		m_isSpent = _toCopy.m_isSpent;
		m_spendTries = _toCopy.m_spendTries;
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
}