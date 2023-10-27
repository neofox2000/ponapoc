using UnityEngine;
using System.Collections;

public class PlayerMarker : MonoBehaviour 
{
    public float groundHeight = 2f;
    public float markerHeight = 4f;
    public LineRenderer mapPathingIndicator;
    public Camera mapCam;

    public Vector3 teleportOffset;
    public bool teleportNow = false;

    Transform myTrans;
    UnityEngine.AI.NavMeshAgent agent;
    Vector3 
        targetPosition, 
        journeyStartPosition;

    [HideInInspector]
    public bool travelling = false;
    public WorldMapArea currentArea = null;

    void Awake()
    {
        myTrans = transform;
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.updateRotation = false;
    }
    void FixedUpdate()
    {
        updatePathIndicator();
    }
    void Update()
    {
        if (travelling)
            stopCheck();

        if(teleportNow)
        {
            teleportNow = false;
            teleport(myTrans.position + teleportOffset);
        }
    }

    void stopCheck()
    {
        //Wait for agent to calculate path
        if (agent.pathPending)
            return;

        //Don't allow travel when path is not found
        if (agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid)
        {
            stopTravelling();
            return;
        }

        //float remainingDistance = Vector3.Distance(myTrans.position, targetPosition);

        if (Vector3.Distance(myTrans.position, agent.pathEndPosition) <= agent.stoppingDistance)
            stopTravelling();
    }
    public void startTravelling()
    {
        //if (targetPosition != playerPosition)
        if (!positionsEqual(targetPosition, myTrans.position))
        {
            if (targetPosition != Vector3.zero)
            {
                //journeyStartTime = Time.time;
                //journeyLength = Vector3.Distance(playerMarkerTrans.position, targetPosition);
                //journeyStartPosition = playerMarkerTrans.position;
                agent.SetDestination(targetPosition);
                //agent.Resume();
                //travelling = true;
                StartCoroutine(waitingForPath());
            }
        }
        else
            //Already at destination
            stopTravelling();
    }
    public void stopTravelling(bool forceStop = false)
    {
        travelling = false;
        targetPosition = Vector3.zero;

        if (forceStop)
            agent.SetDestination(myTrans.position);
            //agent.Stop();
    }
    public void setPlayerTravelTarget(Vector3 target)
    {
        //Set Target with Z Offset
        targetPosition = new Vector3(
            target.x,
            groundHeight,
            target.z);
    }
    public void teleport(Vector3 destination)
    {
        agent.Warp(destination);
        //playerMarkerTrans.position = MetagameManager.instance.worldPosition;
    }
    bool positionsEqual(Vector3 pos1, Vector3 pos2)
    {
        return ((pos1.x == pos2.x) && (pos1.z == pos2.z));
    }
    void updatePathIndicator()
    {
        if (travelling && (agent.path != null) && (agent.pathStatus != UnityEngine.AI.NavMeshPathStatus.PathInvalid))
        {
            int pathLength = agent.path.corners.Length;

            //mapPathingIndicator.SetVertexCount(pathLength);
            mapPathingIndicator.positionCount = pathLength;

            for (int i = 0; i < pathLength; i++)
            {
                mapPathingIndicator.SetPosition(i, new Vector3(
                    agent.path.corners[i].x,
                    markerHeight,
                    agent.path.corners[i].z));                
            }

            mapPathingIndicator.gameObject.SetActive(true);
        }
        else
            mapPathingIndicator.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "MapLocation")
        {
            WorldLocation loc = other.GetComponent<WorldLocation>();
            WorldMap_ProximityTrigger trig = other.GetComponent<WorldMap_ProximityTrigger>();

            if (loc)
            {
                GUIManager.instance.guiWorldMap.guiLocationPanel.OnEnterLocation(loc.template);
                GUIManager.instance.guiWorldMap.show();
            }

            if (trig)
                trig.fire(MapController.instance.gameObject);

            return;
        }

        if(other.tag == "MapArea")
        {
            currentArea = other.GetComponent<WorldMapArea>();
            return;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "MapLocation")
        {
            WorldLocation loc = other.GetComponent<WorldLocation>();
            if (loc.template == GUIManager.instance.guiWorldMap.guiLocationPanel.currentLocation)
            {
                GUIManager.instance.guiWorldMap.guiLocationPanel.OnExitLocation();
                GUIManager.instance.guiWorldMap.hide();
            }
            return;
        }
        if (other.tag == "MapArea")
        {
            if (currentArea == other.GetComponent<WorldMapArea>())
                currentArea = null;

            return;
        }
    }

    IEnumerator waitingForPath()
    {
        while (agent.pathPending)
            yield return new WaitForEndOfFrame();

        travelling = true;
    }
}