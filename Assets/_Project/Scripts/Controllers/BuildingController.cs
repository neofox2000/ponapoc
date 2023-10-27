using UnityEngine;

/// <summary>
/// TODO: Make blocked entrances open if the player successfully breaks them
/// </summary>
public class BuildingController : MonoBehaviour
{
    [System.Serializable]
    public struct EntranceDef
    {
        public float chanceToBeOpen;
        public Transform[] thingsToEnable;
        public Transform[] thingsToDisable;
    }

    public EntranceDef[] entrances;

    void openEntrance(EntranceDef entrance, bool openit)
    {
        //Toggles things based on open state
        foreach (Transform T in entrance.thingsToEnable)
            T.gameObject.SetActive(openit);
        foreach (Transform T in entrance.thingsToDisable)
            T.gameObject.SetActive(!openit);
    }

    void Start()
    {
        //Sets up initial entrance accessibility
        foreach (EntranceDef ED in entrances)
            openEntrance(ED, (ED.chanceToBeOpen > Random.Range(0, 1f)));
    }
}
