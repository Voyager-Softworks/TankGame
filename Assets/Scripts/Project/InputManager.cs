using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A custom input manager that uses the new Input System.
/// <br/>- Add actions to the InputMap in Assets/Settings.
/// <br/>- If adding a new action MAP, make sure to add it here too.
/// <br/>- If adding a new action to an existing MAP, it will be added automatically. (might have wait for it to re-generate the C# class?)
/// </summary>
public static class InputManager
{
    private static InputMap m_inputMap;

    public static InputMap.PlayerLookActions PlayerLook { get { return m_inputMap.PlayerLook; } }
    public static InputMap.PlayerMoveActions PlayerMove { get { return m_inputMap.PlayerMove; } }
    public static InputMap.PlayerSpecialActions PlayerSpecial { get { return m_inputMap.PlayerSpecial; } }
    public static InputMap.PlayerGunActions PlayerGun { get { return m_inputMap.PlayerGun; } }
    public static InputMap.TankDriveActions TankDrive { get { return m_inputMap.TankDrive; } }
    public static InputMap.TankSpecialActions TankSpecial { get { return m_inputMap.TankSpecial; } }

    static InputManager()
    {
        Debug.Log("InputManager | static constructor called");

        if (m_inputMap == null)
        {
            m_inputMap = new InputMap();
        }

        m_inputMap.PlayerLook.Enable();
        m_inputMap.PlayerMove.Enable();
        m_inputMap.PlayerSpecial.Enable();
        m_inputMap.PlayerGun.Enable();
        m_inputMap.TankDrive.Enable();
        m_inputMap.TankSpecial.Enable();
    }
}
