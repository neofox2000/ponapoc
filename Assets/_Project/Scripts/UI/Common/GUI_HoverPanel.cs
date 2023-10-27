using UnityEngine;
using System.Collections;

public class GUI_HoverPanel : GUI_ShowHideObject 
{
    public float xPadding = 30f;
    protected RectTransform myTrans;

    protected override void Awake()
    {
        base.Awake();
        myTrans = GetComponent<RectTransform>();
    }
    virtual public void Setup(RectTransform target, TextAlignment preferredAlignment, bool show = true)
    {
        init();
        //setPosition(target);
        StartCoroutine(DelayedSetPosition(target, preferredAlignment));
        ShowHide(show);
    }
    void SetPosition(RectTransform target, TextAlignment preferredAlignment)
    {
        //Put our position directly on top of the target
        transform.position = target.transform.position;
        Vector3 targetPos = transform.localPosition;

        Vector2 mySize = new Vector2(
            myTrans.rect.width * (1 - myTrans.pivot.x),
            myTrans.rect.height * (1 - myTrans.pivot.y));
        Vector2 targetSize = new Vector2(
            target.rect.width * (1 - target.pivot.x),
            target.rect.height * (1 - target.pivot.y));

        //Calculate X offset
        float offsetX = targetSize.x + mySize.x + xPadding;

        //If the target is past the middle of the screen, then put it on the opposite half instead
        if (targetPos.x > 0) offsetX *= -1;

        /*
        //Set alignment
        switch (preferredAlignment)
        {
            case TextAlignment.Left:
                offsetX *= -1;
                break;
        }
        */

        //Calculate Y offset
        float offsetY = mySize.y - targetSize.y;

        //Stops window from going off the bottom of the screen
        if (((targetPos.y - offsetY) - myTrans.rect.height) < (-Screen.height / 2))
        {
            targetPos.y = myTrans.rect.height - (Screen.height / 2);
            offsetY = 0;
        }

        //Update position
        transform.localPosition = new Vector3(
            targetPos.x + offsetX,
            targetPos.y - offsetY,
            targetPos.z);
    }
    IEnumerator DelayedSetPosition(RectTransform target, TextAlignment preferredAlignment)
    {
        yield return new WaitForEndOfFrame();
        SetPosition(target, preferredAlignment);
    }
}