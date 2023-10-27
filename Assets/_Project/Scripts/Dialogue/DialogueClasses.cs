using Fungus;
using UnityEngine;

public enum QuestState
{
    Inactive = 0,
    Active = 1,
    Succeeded = 2,
    Failed = 3
}
public enum Condition { Is, IsNot }
public enum BarterResult { Cancelled, Accepted, Failed };
public enum HaggleResult { Success, Fail, Exhausted };

[System.Serializable]
public struct QuestStatePair
{
    public QuestTemplate quest;
    public Condition condition;
    public QuestState state;

    public QuestStatePair(QuestTemplate quest, Condition condition, QuestState state)
    {
        this.quest = quest;
        this.condition = condition;
        this.state = state;
    }
}

public static class DialogueQuery
{
    public static bool isQuestRequirementMet(QuestStatePair questReq)
    {
        QuestState questState = GameDatabase.sPlayerData.GetQuestState(questReq.quest);
        return questReq.condition == Condition.Is ? 
            questState == questReq.state:
            questState != questReq.state;
    }
    public static bool areQuestRequirementsMet(QuestStatePair[] requirements)
    {
        foreach (QuestStatePair req in requirements)
            if (!isQuestRequirementMet(req))
                return false;

        return true;
    }
}

public static class DialogueMessages
{
    public static string
        //Bark Messages
        StartConversation = "Start",
        BarkSpecific = "Bark",
        BarkIdle = "BarkIdle",

        //Barter Messages
        BarterAccept = "BarterAccept",
        BarterFail = "BarterFail",
        BarterCancel = "BarterCancel",
        HaggleSuccess = "HaggleSuccess",
        HaggleFail = "HaggleFail",
        HaggleExhausted = "HaggleExhausted";
}
public static class DialogueVariables
{
    public static string
        //Core variables
        BarkBookmark = "BarkBookmark",
        OutputText = "OutputText";
}
public static class Dialog
{
    public static void Bark(Flowchart conversation, int barkID, Transform target)
    {
        //Set variables needed to run operation
        conversation.SetIntegerVariable(DialogueVariables.BarkBookmark, barkID);

        //Start flowchart
        conversation.SendFungusMessage(DialogueMessages.BarkSpecific);
    }
    public static void BarkIdle(Flowchart conversation, Transform target)
    {
        //Start flowchart
        conversation.SendFungusMessage(DialogueMessages.BarkIdle);
    }
    public static void SendBarterResult(Flowchart conversation, BarterResult result)
    {
        switch (result)
        {
            case BarterResult.Accepted:
                conversation.SendFungusMessage(DialogueMessages.BarterAccept);
                break;
            case BarterResult.Cancelled:
                conversation.SendFungusMessage(DialogueMessages.BarterCancel);
                break;
            case BarterResult.Failed:
                conversation.SendFungusMessage(DialogueMessages.BarterFail);
                break;
        }
    }
    /// <summary>
    /// Sends haggle results to the active flowchart
    /// </summary>
    /// <param name="result"></param>
    public static void SendHaggleResult(Flowchart conversation, HaggleResult result)
    {
        switch (result)
        {
            case HaggleResult.Success:
                conversation.SendFungusMessage(DialogueMessages.HaggleSuccess);
                break;
            case HaggleResult.Fail:
                conversation.SendFungusMessage(DialogueMessages.HaggleFail);
                break;
            case HaggleResult.Exhausted:
                conversation.SendFungusMessage(DialogueMessages.HaggleExhausted);
                break;
        }
    }
}