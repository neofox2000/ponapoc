using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(menuName = "Templates/Audio Event Group")]
public class AudioEventGroupTemplate : ScriptableObject
{
    public AudioGroupTemplate pointerEnter;
    public AudioGroupTemplate pointerUp;
    public AudioGroupTemplate pointerDown;
    public AudioGroupTemplate pointerClick;
    public AudioGroupTemplate scroll;
    public AudioGroupTemplate initializePotentialDrag;
    public AudioGroupTemplate beginDrag;
    public AudioGroupTemplate drag;
    public AudioGroupTemplate drop;
    public AudioGroupTemplate endDrag;
    public AudioGroupTemplate submit;
    public AudioGroupTemplate cancel;
    public AudioGroupTemplate select;
    public AudioGroupTemplate deselect;
    public AudioGroupTemplate updateSelected;
    public AudioGroupTemplate move;

    void Play(AudioGroupTemplate audioGroup)
    {
        AudioManager.instance.Play(audioGroup);
    }
    public void Play(EventTriggerType eventTriggerType)
    {
        switch (eventTriggerType)
        {
            case EventTriggerType.PointerEnter: Play(pointerEnter); break;
            case EventTriggerType.PointerDown: Play(pointerDown); break;
            case EventTriggerType.PointerUp: Play(pointerUp); break;
            case EventTriggerType.PointerClick: Play(pointerClick); break;
            case EventTriggerType.Drag: Play(drag); break;
            case EventTriggerType.Drop: Play(drop); break;
            case EventTriggerType.Scroll: Play(scroll); break;
            case EventTriggerType.UpdateSelected: Play(updateSelected); break;
            case EventTriggerType.Select: Play(select); break;
            case EventTriggerType.Deselect: Play(deselect); break;
            case EventTriggerType.Move: Play(move); break;
            case EventTriggerType.InitializePotentialDrag: Play(initializePotentialDrag); break;
            case EventTriggerType.BeginDrag: Play(beginDrag); break;
            case EventTriggerType.EndDrag: Play(endDrag); break;
            case EventTriggerType.Submit: Play(submit); break;
            case EventTriggerType.Cancel: Play(cancel); break;
        }
    }
}