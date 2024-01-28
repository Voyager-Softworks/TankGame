using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Hatch : Interactable
{
    public override void Interact()
    {
        base.Interact();

        Tank.Instance.OnPlayerEnter();
        Player.Instance.DisablePlayer();
    }
}
