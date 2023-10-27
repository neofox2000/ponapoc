using UnityEngine;
using System.Collections;
using Fungus;

public class BarkTrigger : MonoTrigger
{
    public Flowchart conversation;
    public int bark;

    protected override void fire()
    {
        autoCleanup = false;

        base.fire();
        StartCoroutine(fireBark(triggeringObject));
    }
    IEnumerator fireBark(Transform target)
    {
        //Prevent issues when a bark is fired before managers Awake functions have been called
        yield return new WaitForFixedUpdate();
        Dialog.Bark(conversation, bark, target);
        cleanup();
    }
}
