using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An electrical connector that that can be broken.
/// </summary>
public class Electrical_Connector : Electrical
{
    [Header("References")]
    public List<GameObject> m_connectionChain = new List<GameObject>();
    public Chain m_chain = null;

    void Update()
    {
        // check if the connection chain is broken
        if (!IsBroken)
        {
            // check if the chain is broken
            if (m_chain != null && m_chain.IsBroken())
            {
                // break the chain
                SetBroken(true);
                return;
            }

            // missing links
            for (int i = 0; i < m_connectionChain.Count; i++)
            {
                if (m_connectionChain[i] == null)
                {
                    // break the chain
                    SetBroken(true);
                    return;
                }
            }
        }
    }
}