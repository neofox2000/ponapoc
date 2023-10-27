using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameDB;

public class PlayGameSound : MonoBehaviour
{
    public enum SoundEvents {
        Awake, Start, Spawn, Enable,
        TriggerEnter, TriggerEnter2D, TriggerExit, TriggerExit2D }

    [System.Serializable]
    public class SoundEvent
    {
        public SoundEvents triggerType = SoundEvents.Awake;
        public AudioGroupTemplate audioGroup;
    }

    [SerializeField] List<SoundEvent> soundEvents;

    void PlaySound(SoundEvents trigger)
    {
        SoundEvent sound = soundEvents.Find(x => x.triggerType == trigger);
        if (sound != null)
            //Delay for correct sound position if repositioning occurs after spawn
            StartCoroutine(DelayedPlaySound(sound.audioGroup));
    }
    IEnumerator DelayedPlaySound(AudioGroupTemplate audioGroup)
    {
        yield return new WaitForEndOfFrame();

        //AudioManager.instance.Play(soundID, transform.position);
        AudioManager.instance.Play(audioGroup, transform.position);
    }

    private void Awake()
    {
        PlaySound(SoundEvents.Awake);
    }
    private void Start()
    {
        PlaySound(SoundEvents.Start);
    }
    private void OnSpawn()
    {
        PlaySound(SoundEvents.Spawn);
    }
    private void OnEnable()
    {
        PlaySound(SoundEvents.Enable);
    }
    private void OnTriggerEnter(Collider other)
    {
        PlaySound(SoundEvents.TriggerEnter);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlaySound(SoundEvents.TriggerEnter2D);
    }
    private void OnTriggerExit(Collider other)
    {
        PlaySound(SoundEvents.TriggerExit);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        PlaySound(SoundEvents.TriggerExit2D);
    }
}