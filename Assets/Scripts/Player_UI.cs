using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerUI is a class that manages the UI of the player.
/// </summary>
public class PlayerUI : MonoBehaviour
{
    public RectTransform m_helpSection = null;

    [Header("Focus")]
    public RectTransform m_focusTL = null;
    public RectTransform m_focusTR = null;
    public RectTransform m_focusBL = null;
    public RectTransform m_focusBR = null;
    public RectTransform m_focusC = null;
    public RectTransform m_focusTextBG = null;
    public RectTransform m_interactText = null;
    public RectTransform m_grabText = null;
    public RectTransform m_dropText = null;

    private void Start()
    {
        // disable help section
        ToggleHelp(false);
    }

    private void Update()
    {
        if (InputManager.PlayerSpecial.Help.WasPerformedThisFrame())
        {
            ToggleHelp(!m_helpSection.gameObject.activeSelf);
        }
    }

    /// <summary> Toggle the help section </summary>
    /// <param name="_show">Show or hide the help section</param>
    public void ToggleHelp(bool _show)
    {
        m_helpSection.gameObject.SetActive(_show);
    }
}
