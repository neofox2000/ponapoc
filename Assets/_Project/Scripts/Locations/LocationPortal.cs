using UnityEngine;
using GameDB;

[RequireComponent(typeof(InteractableObject))]
public class LocationPortal : MonoBehaviour
{
    public enum PortalTypes { Map, Mission }

    [Header("Portal Stats")]
    public WorldLocationEntrance template;
    public Direction directionToFace = Direction.Right; 
    public PortalTypes portalType;

    [Header("Portal Target")]
    [Tooltip("Link to a portal in another World Location")]
    public WorldLocationEntrance portalTarget;

    InteractableObject interactableObject;

    private void Awake()
    {
        interactableObject = GetComponent<InteractableObject>();
        interactableObject.OnInteractAccessible += interact;
    }
    void interact(Actor interactor)
    {
        if (LoadingManager.awoken)
        {
            //Only allow players to use this
            PlayerController player = interactor.GetComponent<PlayerController>();
            if (player)
            {
                GameManager.instance.ExitMission(this);
                return;
            }
            else
                Debug.LogWarning("Non-Player entity is attempting to interact with me!");
        }
        else
            Debug.LogWarning("Cannot leave level in TEST MODE!");
    }

    public static LocationPortal Find(WorldLocationEntrance template)
    {
        LocationPortal[] mapPortals = Resources.FindObjectsOfTypeAll<LocationPortal>();

        //Bail if no portals are available
        if (mapPortals.Length == 0)
        {
            Debug.LogError("No map portals present in scene");
            return null;
        }

        //Search for a matching template and return it if found
        if (template != null)
            foreach (LocationPortal lp in mapPortals)
                if (lp.template == template)
                    return lp;

        //Template was provided, but no matching portal was found -> warn dev
        if (template != null)
            Debug.LogWarning("No map portals found for " + template.displayName + " --- defaulting to first portal");

        //No matching template was found -> use first map portal available
        return mapPortals[0];
    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (portalType == PortalTypes.Map)
            Gizmos.color = Color.green;
        if (portalType == PortalTypes.Mission)
            Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
#endif
}