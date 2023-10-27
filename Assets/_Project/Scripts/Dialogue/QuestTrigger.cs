using UnityEngine;
using System.Collections;

public class QuestTrigger : MonoTrigger
{
    [Tooltip("What quest conditions must be met for this object to be activated")]
    public QuestStatePair[] questConditions;
    [Tooltip("What quest states will be set when this object is activated")]
    public QuestStatePair[] questStatesToSet;

    void Start()
    {
        //Disable if conditions are not met
        foreach (QuestStatePair foo in questConditions)
            if (GameDatabase.sPlayerData.GetQuestState(foo.quest) != foo.state)
            {
                disable();
                break;
            }
    }
    void disable()
    {
        gameObject.SetActive(false);
    }
    protected override void fire()
    {
        base.fire();

        //Update quest states
        foreach (QuestStatePair foo in questStatesToSet)
            GameDatabase.sPlayerData.SetQuestState(
                foo.quest, 
                foo.state, 
                0f);

        //Disable to make sure it doesn't fire again
        disable();
    }
}