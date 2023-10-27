using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

/// <summary>
/// Handles sprite sorting and scaling to simulate depth (z-axis)
/// </summary>
[ExecuteInEditMode]
public class DepthController : MonoBehaviour 
{
    public const int sortModifier = -1000;
    public const float depthDivider = 2f;

    public float verticalOffset;
    public bool fireOnce = true;
    public bool useLocationScaling = false;

    public Transform hostTrans { get; protected set; }
    Transform _myTrans = null;
    Transform myTrans
    {
        get
        {
            if (_myTrans == null)
                _myTrans = transform;

            return _myTrans;
        }
    }
    Vector3 lastPosition;

    //Used to prevent constant detections if no depth host is found
    bool findAttempted = false;
    public int sortingOrder { get; protected set; }
    DepthHost _depthHost = null;
    DepthHost depthHost
    {
        get
        {
            if(!findAttempted)
            {
                //Don't try to detect again
                findAttempted = true;

                //Reset values just in case
                _depthHost = null;
                hostTrans = null;

                //Try to find a host
                _depthHost = DepthHost.Find(myTrans);

                //Found a new host
                if (_depthHost)
                {

                    hostTrans = _depthHost.transform;

                    //Reset Z position (for... some reason?)
                    myTrans.position = new Vector3(myTrans.position.x, myTrans.position.y, 0f);
                }
            }

            return _depthHost;
        }
    }
    public float hostScaleOffset
    {
        get
        {
            return depthHost == null ? 1f : depthHost.scaleOffset;
        }
    }
    SortingGroup _sortingGroup = null;
    public SortingGroup sortingGroup
    {
        get
        {
            if (!_sortingGroup)
                _sortingGroup = GetComponentInChildren<SortingGroup>();

            return _sortingGroup;
        }
    }

    void ResetMe()
    {
        findAttempted = false;

        //Reset scale (in case new host does not use scaling)
        myTrans.localScale = Vector3.one;
    }
    void SetDepth()
    {
        if (!myTrans) return;

        //Try to detect teleportation (may need room re-detection)
        if (Vector2.Distance(lastPosition, myTrans.position) > 1f)
            ResetMe();

        lastPosition = myTrans.position;

        //Get host's Z position
        float hostY =
            (depthHost ? hostTrans.position.y : 0f) +
            verticalOffset;

        //Set Z position
        float zMod = (myTrans.position.y - hostY) / depthDivider;
        myTrans.position = new Vector3(myTrans.position.x, myTrans.position.y, zMod);

        //Set scaling
        if (useLocationScaling && depthHost != null)
        {
            if (depthHost.scaleFator != 0)
            {
                //depthHost.scaleFator * ((zMod - (hostTrans.position.y / depthDivider)) + depthHost.scaleOffset);
                float scaleMod = depthHost.scaleOffset +
                    (depthHost.scaleFator * zMod);

                myTrans.localScale = new Vector3(
                    1f - scaleMod,
                    1f - scaleMod,
                    1f - scaleMod);
            }
            else
            {
                myTrans.localScale = Vector3.one + (Vector3.one * depthHost.scaleOffset);
            }
        }

        //Calculation location offset to avoid integer overflow in sortingOrder property
        float locationSortingOffset = hostTrans ?
            locationSortingOffset = hostTrans.position.z :
            0f;

        //Set sorting order
        sortingOrder =
            Mathf.RoundToInt(
                (myTrans.position.z - locationSortingOffset) *
                sortModifier);

        //Apply sorting order to rendering group
        if (sortingGroup)
            sortingGroup.sortingOrder = sortingOrder;
    }
    void FireAndDisable()
    {
        if (fireOnce && Application.isPlaying)
        {
            if (sortingGroup)
            {
                SetDepth();
                enabled = false;
            }
        }
    }

    void Start()
    {
        FireAndDisable();
    }
    void Update() 
    {
        if(!fireOnce || !Application.isPlaying)
            SetDepth();
    }

    void OnDrawGizmosSelected()
    {
        //Show horizontal guide (the point at which a sprite will show behind/in front of)
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            transform.position + new Vector3(-1f, -verticalOffset),
            transform.position + new Vector3(+1f, -verticalOffset));
        Gizmos.DrawLine(
            transform.position + new Vector3(0f, -verticalOffset),
            transform.position + new Vector3(0f, -verticalOffset + 1f));
    }
}
