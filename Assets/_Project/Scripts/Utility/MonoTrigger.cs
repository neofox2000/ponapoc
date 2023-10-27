using UnityEngine;

public class MonoTrigger : MonoBehaviour
{
    //Enumerations
    public enum TriggerType { Awake, Start, Spawned, Enabled, Disabled, TriggerEnter, TriggerExit, Interact }
    public enum CleanupAction { None, Disable, Destroy, DisableCollider }

    //Inspector Properties
    [SerializeField] protected TriggerType trigger = TriggerType.TriggerEnter;
    [Tooltip("After the trigger as been hit, this number of seconds will be waited before action is taken")]
    [SerializeField] protected float triggerDelay;
    [Tooltip("What to do after trigger finishes")]
    [SerializeField] protected CleanupAction cleanupAction = CleanupAction.Disable;

    //Protected Properties
    protected bool autoCleanup = true;
    protected Transform triggeringObject = null;

    //Private Properties
    private float spawnDelayTimer = 0;
    private bool working = false;
    private InteractableObject interactableObject = null;

    //Methods
    void PrepareSpawn()
    {
        spawnDelayTimer = triggerDelay;
        working = true;
    }

    //Override Methods
    protected virtual void fire() { }
    void interact(Actor target)
    {
        fire();
    }
    protected virtual void cleanup()
    {
        switch(cleanupAction)
        {
            case CleanupAction.Disable:
                gameObject.SetActive(false);
                break;
            case CleanupAction.Destroy:
                Destroy(gameObject);
                break;
            case CleanupAction.DisableCollider:
                //Check for Normal Collider
                Collider col = GetComponent<Collider>();
                if (col) col.enabled = false;
                
                //Check for 2D Collider
                Collider2D col2d = GetComponent<Collider2D>();
                if (col2d) col2d.enabled = false;

                break;
        }
    }

    //Monobehaviour Methods
    void OnEnable()
    {
        if (trigger == TriggerType.Enabled)
        {
            triggeringObject = transform;
            PrepareSpawn();
        }
    }
    void OnDisable()
    {
        if (trigger == TriggerType.Disabled)
        {
            triggeringObject = transform;
            PrepareSpawn();
        }
    }
    void OnSpawn()
    {
        if (trigger == TriggerType.Spawned)
        {
            triggeringObject = transform;
            PrepareSpawn();
        }
    }
    void Awake()
    {
        interactableObject = GetComponent<InteractableObject>();
        if (interactableObject)
        {
            interactableObject.OnInteractAccessible += interact;
            if (trigger != TriggerType.Interact)
            {
                trigger = TriggerType.Interact;
                Debug.LogWarning(name + "> " + 
                    "Found Interactable Object on Trigger but not set to 'Interact' trigger type!\n" +
                    "Trigger type has been changed to 'Interact'");
            }
        }
        else
        {
            if (trigger == TriggerType.Interact)
                Debug.LogWarning(name + "> Trigger Type set to 'Interact' but no Interactable Object script found!");
        }

        if (trigger == TriggerType.Awake)
        {
            triggeringObject = transform;
            PrepareSpawn();
        }
    }
    void Start()
    {
        if (trigger == TriggerType.Start)
        {
            triggeringObject = transform;
            PrepareSpawn();
        }
    }
    void OnTriggerEnter(Collider collider)
    {
        if (trigger == TriggerType.TriggerEnter)
        {
            triggeringObject = collider.transform;
            PrepareSpawn();
        }
    }
    void OnTriggerExit(Collider collider)
    {
        if (trigger == TriggerType.TriggerExit)
        {
            triggeringObject = collider.transform;
            PrepareSpawn();
        }
    }
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (trigger == TriggerType.TriggerEnter)
        {
            triggeringObject = collider.transform;
            PrepareSpawn();
        }
    }
    void OnTriggerExit2D(Collider2D collider)
    {
        if (trigger == TriggerType.TriggerExit)
        {
            triggeringObject = collider.transform;
            PrepareSpawn();
        }
    }

    void Update()
    {
        if (working)
        {
            if (spawnDelayTimer <= 0)
            {
                working = false;
                fire();
                if(autoCleanup) cleanup();
            }
            else
                spawnDelayTimer -= Time.deltaTime;
        }
    }
}
