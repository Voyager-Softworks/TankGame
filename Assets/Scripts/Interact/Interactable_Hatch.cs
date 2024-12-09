using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Hatch : Interactable
{
    public override void OnInteract(Focuser _interacter)
    {
        base.OnInteract(_interacter);

        if (!IsInteractable)
        {
            return;
        }

        Tank.Instance.OnPlayerEnter();
        Player.Instance.DisablePlayer();
    }
}
