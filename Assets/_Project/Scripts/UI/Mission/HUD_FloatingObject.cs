using UnityEngine;

public class HUD_FloatingObject : MonoBehaviour 
{
    public Vector3 screenOffset, worldOffset;
    public Transform target;

    protected Transform myTrans;
    protected Vector3
        targetPos = Vector3.zero,
        newPos = Vector3.zero;
    protected Camera cam;

    protected virtual void Update()
    {
        if (cam)
        {
            //Use target position if available
            if (target) targetPos = target.position;

            //Calculated the screen position
            newPos = cam.WorldToScreenPoint(targetPos + worldOffset);

            //Compensate for screen coordinate system
            newPos.x = (Screen.width / 2) - newPos.x;
            newPos.y = (-Screen.height / 2) + newPos.y;
            newPos.z = 0;

            //Assign calculated position
            myTrans.localPosition = new Vector3(
                -(newPos.x + screenOffset.x),
                newPos.y + screenOffset.y,
                newPos.z);
        }
    }
    protected virtual void cacheStuff()
    {
        myTrans = transform;
        cam = Camera.main;
    }
    public virtual void fire(Transform target)
    {
        gameObject.SetActive(true);
        this.target = target;
        cacheStuff();
        Update();
    }
    public virtual void fire(Vector3 fixedPosition)
    {
        targetPos = fixedPosition;
        fire(null);
    }
    /// <summary>
    /// Primarily used for mass cleanup purposes
    /// </summary>
    public virtual void hide()
    {
        gameObject.SetActive(false);
    }
}
