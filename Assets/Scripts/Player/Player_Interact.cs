using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Interact : MonoBehaviour
{
    [SerializeField]
    Transform cameraTransform;

    public float interactRayDistance = 5.0f;

    public LayerMask interactableLayerMask;

    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = Camera.main.transform;

    }

    // Update is called once per frame
    void Update()
    {
        ProcessInput();
    }

    void ProcessInput()
    {
        if (InputManager.PlayerSpecial.Interact.triggered)
        {
            CheckInteractable();
        }
    }

    void CheckInteractable()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactRayDistance, interactableLayerMask, QueryTriggerInteraction.Collide))
        {
            Interactable selectedInteractableObject = hit.collider.GetComponent<Interactable>();
            if(selectedInteractableObject != null) {
                selectedInteractableObject.Interact();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * interactRayDistance);
    }
}
