using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEBUG_AirdropLever : MonoBehaviour
{
    [SerializeField] private Electrical m_lever;

    private void Awake()
    {
        m_lever = GetComponent<Electrical>();

        m_lever.OnPowerStateChanged += OnPowerStateChanged;
    }

    private void OnPowerStateChanged(bool _isOn)
    {
        if (_isOn)
        {
            Plane_Movement plane = FindObjectOfType<Plane_Movement>();
            if (plane != null)
            {
                plane.DebugAirdrop();
            }
        }
        else
        {
            Plane_Movement plane = FindObjectOfType<Plane_Movement>();
            if (plane != null)
            {
                plane.StopAirdrop();
            }
        }
    }
}
