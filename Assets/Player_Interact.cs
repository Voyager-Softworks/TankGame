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
        CheckInteractable();
    }

    void CheckInteractable()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactRayDistance, interactableLayerMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("hit Object" + hit.transform.name);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.Red;
        Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * interactRayDistance);
    }
}
