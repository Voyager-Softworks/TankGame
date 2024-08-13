using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Electrical class is used to manage the electrical components in the game. <br>
/// Its fairly simple, can be a source, can receive, can transfer.
/// </summary>
public class Electrical : MonoBehaviour
{
    #region Variables
    private static List<Electrical> s_allElectricals = new List<Electrical>();
    public static List<Electrical> AllElectricals { get { return s_allElectricals; } }

    [Header("Electrical Properties")]
    [SerializeField] protected bool m_isSource = false;
    public bool IsSource { get { return m_isSource; } }
    [SerializeField] protected bool m_canReceive = true;
    public bool CanReceive { get { return m_canReceive; } }
    [SerializeField] protected bool m_canTransfer = true;
    public bool CanTransfer { get { return m_canTransfer; } }

    [Header("Electrical Links")]
    [SerializeField] protected List<Electrical> m_receivedFrom = new List<Electrical>();
    public List<Electrical> ReceivedFrom { get { return m_receivedFrom; } }
    [SerializeField] protected List<Electrical> m_transfersTo = new List<Electrical>();
    public List<Electrical> TransfersTo { get { return m_transfersTo; } }

    [Header("Power State")]
    [Tooltip("Is this electrical in an On state?\nSet this to true for things that start in a powered state! e.g. Lights")]
    [SerializeField] protected bool m_isOn = false;
    public bool IsOn { get { return m_isOn; } }
    /// <summary> Is this electrical receiving power from any source? </summary>
    public bool IsReceivingPower { get { return m_receivedFrom.Any(e => e.HasPower); } }
    /// <summary> Does this electrical have power right now? </summary>
    public bool HasPower { get { return m_isSource || IsReceivingPower; } }

    protected bool m_isBeingDestroyed = false;
    #endregion

    #region Unity Functions
    private void OnDrawGizmos()
    {
        Gizmos.color = HasPower ? Color.green : Color.red;
        Gizmos.DrawSphere(transform.position, 0.1f);

        foreach (Electrical source in m_receivedFrom)
        {
            Gizmos.color = source.HasPower ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, source.transform.position);
        }
        foreach (Electrical receiver in m_transfersTo)
        {
            Gizmos.color = receiver.HasPower ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, receiver.transform.position);
        }
    }

    protected virtual void Awake()
    {
        // Add this to the list of all electricals.
        if (!s_allElectricals.Contains(this))
        {
            s_allElectricals.Add(this);
        }
    }

    protected virtual void Start()
    {
        // bind all electricals
        foreach (Electrical source in m_receivedFrom)
        {
            source.TransferTo(this);
        }
        foreach (Electrical receiver in m_transfersTo)
        {
            receiver.ReceiveFrom(this);
        }
    }

    protected virtual void OnDestroy()
    {
        m_isBeingDestroyed = true;

        // Remove this from the list of all electricals.
        if (s_allElectricals.Contains(this))
        {
            s_allElectricals.Remove(this);
        }

        // Remove this from all the sources and receivers. (reverse loop)
        for (int i = m_receivedFrom.Count - 1; i >= 0; i--)
        {
            m_receivedFrom[i].RemoveTransfer(this);
        }
        for (int i = m_transfersTo.Count - 1; i >= 0; i--)
        {
            m_transfersTo[i].RemoveSource(this);
        }
    }
    #endregion

    #region Power Control
    /// <summary>
    /// Updates the state of the power, and its children.
    /// </summary>
    public void UpdatePowerState()
    {
        // if source or receiving power, turn on
        if (m_isSource || IsReceivingPower)
        {
            PowerOn();
        }
        // else turn off
        else
        {
            PowerOff();
        }

        // Update all the children
        foreach (Electrical receiver in m_transfersTo)
        {
            receiver.UpdatePowerState();
        }
    }

    /// <summary>
    /// Makes this electrical a power source.
    /// </summary>
    /// <returns></returns>
    public bool SetPowerSource(bool _isSource)
    {
        if (m_isSource == _isSource)
        {
            return false;
        }

        m_isSource = _isSource;

        UpdatePowerState();

        return true;
    }

    /// <summary>
    /// Powers on this electrical if not already, and does power on logic.
    /// </summary>
    /// <returns></returns>
    private bool PowerOn()
    {
        if (m_isOn)
        {
            return false;
        }

        m_isOn = true;

        OnPowerOn();

        return true;
    }

    /// <summary>
    /// Powers off this electrical if not already, and does power off logic.
    /// </summary>
    private bool PowerOff()
    {
        if (!m_isOn)
        {
            return false;
        }

        m_isOn = false;

        // if not being destroyed, do power off logic
        if (!m_isBeingDestroyed && gameObject.activeInHierarchy)
        {
            OnPowerOff();
        }

        return true;
    }

    /// <summary>
    /// Does the individual power on logic (Does not turn on/off children!).
    /// </summary>
    protected virtual void OnPowerOn()
    {
        Debug.Log("Electrical.OnPowerOn | " + gameObject.name);
    }

    /// <summary>
    /// Does the individual power off logic (Does not turn on/off children!).
    /// </summary>
    protected virtual void OnPowerOff()
    {
        Debug.Log("Electrical.OnPowerOff | " + gameObject.name);
    }
    #endregion

    #region Flow Control
    /// <summary>
    /// Source gives power to the this receiver.
    /// </summary>
    /// <param name="_source"></param>
    /// <returns>Was the source added?</returns>
    public bool ReceiveFrom(Electrical _source)
    {
        // null check
        if (_source == null)
        {
            Debug.LogWarning("Electrical.ReceiveFrom | _source is null");
            return false;
        }

        // already exists (prevent loops)
        if (m_receivedFrom.Contains(_source))
        {
            return false;
        }

        // Receive from the source
        m_receivedFrom.Add(_source);

        // Transfer to this
        _source.TransferTo(this);

        UpdatePowerState();

        return true;
    }

    /// <summary>
    /// This source gives power to the receiver.
    /// </summary>
    /// <param name="_receiver"></param>
    /// <returns>Was the _receiver added?</returns>
    public bool TransferTo(Electrical _receiver)
    {
        // null check
        if (_receiver == null)
        {
            Debug.LogWarning("Electrical.TransferTo | receiver is null");
            return false;
        }

        // already exists (prevent loops)
        if (m_transfersTo.Contains(_receiver))
        {
            return false;
        }

        // Transfer to the receiver
        m_transfersTo.Add(_receiver);

        // Receive from this
        _receiver.ReceiveFrom(this);

        UpdatePowerState();

        return true;
    }

    /// <summary>
    /// Remove the source from the list.
    /// </summary>
    /// <param name="_source"></param>
    /// <returns>Was the source removed?</returns>
    public bool RemoveSource(Electrical _source)
    {
        // null check
        if (_source == null)
        {
            Debug.LogWarning("Electrical.RemoveSource | _source is null");
            return false;
        }

        // existing check (prevent loops)
        if (!m_receivedFrom.Contains(_source))
        {
            return false;
        }

        // Stop receiving from the source
        bool removed = m_receivedFrom.Remove(_source);

        // Stop transferring to this
        _source.RemoveTransfer(this);

        UpdatePowerState();

        return removed;
    }

    /// <summary>
    /// Remove the receiver from the list.
    /// </summary>
    /// <param name="_receiver"></param>
    /// <returns>Was the receiver removed?</returns>
    public bool RemoveTransfer(Electrical _receiver)
    {
        // null check
        if (_receiver == null)
        {
            Debug.LogWarning("Electrical.RemoveTransfer | _receiver is null");
            return false;
        }

        // existing check (prevent loops)
        if (!m_transfersTo.Contains(_receiver))
        {
            return false;
        }

        // Stop transferring to the receiver
        bool removed = m_transfersTo.Remove(_receiver);

        // Stop receiving from this
        _receiver.RemoveSource(this);

        UpdatePowerState();

        return removed;
    }

    /// <summary>
    /// Unlinks the given electrical from this completely.
    /// </summary>
    /// <param name="_electrical"></param>
    /// <returns>Was the electrical removed?</returns>
    public bool RemoveElectrical(Electrical _electrical)
    {
        // null check
        if (_electrical == null)
        {
            Debug.LogWarning("Electrical.RemoveElectrical | _electrical is null");
            return false;
        }

        // Remove from receivedFrom
        bool removed = RemoveSource(_electrical);
        removed |= RemoveTransfer(_electrical);

        return removed;
    }
    #endregion
}