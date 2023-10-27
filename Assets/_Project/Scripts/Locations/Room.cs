using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(DepthHost))]
public class Room : MonoBehaviour
{
    public RoomTemplate roomTemplate;

    //Actors present in this room
    List<Actor> actors = new List<Actor>();

    void Awake()
    {
        //Not sure where else to put this at the moment
        LevelManager.Startup(transform);
    }
    void OnDisable()
    {
        actors.Clear();
    }
    public float verticalOffset 
    {
        get { return transform.localPosition.y; }
    }

    //Music
    public void PlayNormalMusic()
    {
        roomTemplate.PlayNormalMusic();
    }
    public void PlayBattleMusic()
    {
        roomTemplate.PlayBattleMusic();
    }

    //Static Methods
    public static Room Detect(Transform target)
    {
        Collider2D col = Physics2D.OverlapPoint(target.position, GameDatabase.core.locationLayer);
        if (col)
            return col.GetComponent<Room>();
        else
        {
            Debug.LogWarning("Could not detect Room at " + target.position);
            return null;
        }
    }

    //Editor Visual Helpers
    private void OnDrawGizmosSelected()
    {
        //Cache position
        Vector3 origin = transform.position;

        //Set line color
        Gizmos.color = Color.cyan;

        //Draw top camera line
        Gizmos.DrawLine(
            origin + new Vector3(-1000f, +12f),
            origin + new Vector3(+1000f, +12f));
        Gizmos.DrawLine(
            origin + new Vector3(-1000f, +12.2f),
            origin + new Vector3(+1000f, +12.2f));

        //Draw bottom camera line
        Gizmos.DrawLine(
            origin + new Vector3(-1000f, -8f), 
            origin + new Vector3(+1000f, -8f));
        Gizmos.DrawLine(
            origin + new Vector3(-1000f, -8.2f),
            origin + new Vector3(+1000f, -8.2f));
    }

    //Entity Management
    public void EntityEnteredRoom(Actor entity)
    {
        actors.Add(entity);
    }
    public void EntityLeftRoom(Actor entity)
    {
        actors.Remove(entity);
    }
    public List<Actor> GetActorsInRoom()
    {
        return actors;
    }
}