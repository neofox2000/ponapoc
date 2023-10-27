using UnityEngine;

public class WorldLocation : MonoBehaviour
{
    public Transform
        artContainer,       //The location's art (buildings, etc) - appears when the player is in visual range
        labelContainer,     //The location's name - appears when the player has arrived
        markerContainer;    //The location's in-fog marker - disappears when the player is in visual range                    

    public WorldLocationTemplate template;

    UnityEngine.AI.NavMeshObstacle nmObstacle;

    void Awake()
    {
        gameObject.tag = "MapLocation";
        nmObstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();
    }

    public void showArt(bool showIt)
    {
        if (artContainer)
            artContainer.gameObject.SetActive(showIt);
    }
    public void showLabel(bool showIt)
    {
        if (labelContainer)
            labelContainer.gameObject.SetActive(showIt);
    }
    public void showMarker(bool showIt)
    {
        if (markerContainer)
            markerContainer.gameObject.SetActive(showIt);
    }
    
    public void hide()
    {
        gameObject.SetActive(false);
    }
    public void show(bool newMission = false)
    {
        gameObject.SetActive(true);
    }
    public void toggleBlocker(bool on)
    {
        if (nmObstacle)
            nmObstacle.enabled = on;
    }
}