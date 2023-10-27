using UnityEngine;
using System;
using GameDB;

[RequireComponent(typeof(InteractableObject))]
public class RoomPortal : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("Position that the user will be teleported to")]
    public Transform target;

    InteractableObject interactableObject;

    //Mono Methods
    private void Awake()
    {
        interactableObject = GetComponent<InteractableObject>();
        interactableObject.OnInteractAccessible += Interact;
    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (target)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.25f);
            Gizmos.DrawLine(transform.position, target.position);
            Gizmos.DrawWireSphere(target.position, 0.25f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
#endif

    //Other Methods
    void MoveInteractorToTarget(Actor interactor)
    {
        //Move controller
        interactor.transform.position = target.position;

        //Notify entity that it has been teleported
        interactor.Teleported();
    }
    void Interact(Actor interactor)
    {
        if (target)
        {
            //Move and notify PlayerController
            MoveInteractorToTarget(interactor);
        }
        else
        {
            //Should never get here if design is setup properly
            Debug.LogWarning(name + ": Portal target not set!");
        }
    }
}
