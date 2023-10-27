using UnityEngine;

public class ColliderEventPassUp : MonoBehaviour
{
    public Collider2D myCollider;
    
    //Trigger Enter 2D delegate
    public delegate void TriggerEntered2D(Collider2D collider);
    public event TriggerEntered2D OnTriggerEntered2D;

    //Tigger Exit2D delegate
    public delegate void TriggerExited2D(Collider2D collider);
    public event TriggerExited2D OnTriggerExited2D;

    void Awake()
    {
        if (!myCollider)
            Debug.LogWarning(gameObject.name + ": Collider not assigned!");
    }

    //Mono methods
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (OnTriggerEntered2D != null)
            OnTriggerEntered2D(collider);
    }
    void OnTriggerExit2D(Collider2D collider)
    {
        if (OnTriggerExited2D != null)
            OnTriggerExited2D(collider);
    }
}
