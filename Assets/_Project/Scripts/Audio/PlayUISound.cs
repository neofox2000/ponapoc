using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using GameDB;

public class PlayUISound : MonoBehaviour, 
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler,
    IScrollHandler, ISelectHandler, IDeselectHandler, IUpdateSelectedHandler, ICancelHandler, IMoveHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler, IDropHandler,
    ISubmitHandler
{
    [System.Serializable]
    public class SoundEvent
    {
        public EventTriggerType triggerType = EventTriggerType.PointerClick;
        public AudioGroupTemplate audioGroup;
    }

    [SerializeField] AudioEventGroupTemplate audioEventGroup;
    [Tooltip("NB: Events from this list override the chosen Audio Event Group")]
    [SerializeField] List<SoundEvent> soundEvents;

    void PlaySound(EventTriggerType eventTriggerType)
    {
        SoundEvent soundEvent = soundEvents.Find(x => x.triggerType == eventTriggerType);
        if (soundEvent != null)
        {
            //Play soundEvents list if available
            //AudioManager.instance.Play(soundEvent.sound);
            AudioManager.instance.Play(soundEvent.audioGroup);            
        }
        else
        {
            //Try audio event group if no soundEvent available
            if (audioEventGroup != null)
                audioEventGroup.Play(eventTriggerType);
        }
    }

    public void OnPointerEnter(PointerEventData data)
    {
        PlaySound(EventTriggerType.PointerEnter);
    }
    public void OnPointerExit(PointerEventData data)
    {
        PlaySound(EventTriggerType.PointerExit);
    }
    public void OnPointerUp(PointerEventData data)
    {
        PlaySound(EventTriggerType.PointerUp);
    }
    public void OnPointerDown(PointerEventData data)
    {
        PlaySound(EventTriggerType.PointerDown);
    }
    public void OnPointerClick(PointerEventData data)
    {
        PlaySound(EventTriggerType.PointerClick);
    }
    public void OnScroll(PointerEventData data)
    {
        PlaySound(EventTriggerType.Scroll);
    }
    public void OnBeginDrag(PointerEventData data)
    {
        PlaySound(EventTriggerType.BeginDrag);
    }
    public void OnDrag(PointerEventData data)
    {
        PlaySound(EventTriggerType.Drag);
    }
    public void OnDrop(PointerEventData data)
    {
        PlaySound(EventTriggerType.Drop);
    }
    public void OnEndDrag(PointerEventData data)
    {
        PlaySound(EventTriggerType.EndDrag);
    }
    public void OnCancel(BaseEventData eventData)
    {
        PlaySound(EventTriggerType.Cancel);
    }
    public void OnSelect(BaseEventData eventData)
    {
        PlaySound(EventTriggerType.Select);
    }
    public void OnDeselect(BaseEventData eventData)
    {
        PlaySound(EventTriggerType.Deselect);
    }
    public void OnInitializePotentialDrag(PointerEventData data)
    {
        PlaySound(EventTriggerType.InitializePotentialDrag);
    }
    public void OnMove(AxisEventData data)
    {
        PlaySound(EventTriggerType.Move);
    }
    public void OnSubmit(BaseEventData data)
    {
        PlaySound(EventTriggerType.Submit);
    }
    public void OnUpdateSelected(BaseEventData data)
    {
        PlaySound(EventTriggerType.UpdateSelected);
    }
}