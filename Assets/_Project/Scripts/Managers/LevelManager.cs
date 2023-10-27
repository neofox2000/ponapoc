using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameDB;

public class LevelManager : MonoBehaviour
{
    //Constants
    const float questWindowWidth = 280f;
    const float questWindowHeight = 10f;
    const float topScreen = 50f;
    const float topMargin = 20f;
    const float leftMargin = 8f;
    const float textHeight = 20f;
    const float spacing = 4f;

    //Static properties
    static bool started = false;
    static bool testing = false;

    //Inspector Properties
    public bool showQuests = false;

    //Private Properties
    List<SaveQuest> playerQuests;

    //Mono Methods
    private void Awake()
    {
        LoadingManager.LoadManagerScenes();
        StartCoroutine(Init());
    }
    private void OnDestroy()
    {
        //Reset flag so that level manager can start next time a mission is loaded
        started = false;
    }
    IEnumerator Init()
    {
        if (testing)
        {
            //Create basic testing data
            GameDatabase.core.NewGame(false, true);

            //Wait for GUI scene to load
            while (!GUIManager.instance)
                yield return new WaitForEndOfFrame();

            //Init GUI stuff
            GUIManager.instance.toggleGUI(GUIManager.GUISelection.Mission, true);
            GUI_Common.instance.OnCloseAll();
        }

        //Spawn camera
        Instantiate(GameDatabase.core.missionCamPrefab, transform);
        //Spawn GameManager
        Instantiate(GameDatabase.core.gameManagerPrefab, transform);
    }

    //Static Methods
    public static void Startup(Transform caller)
    {
        //Begin testing mode
        if (!started)
        {
            //Determine if we're in testing mode
            testing = !LoadingManager.awoken;

            //Mark as started to avoid multiple calls from scenes with multiple rooms
            started = true;

            //Create container
            GameObject testObject = new GameObject("Level Manager");

            //Move container into room scene to avoid putting transient objects in permanent scenes
            testObject.transform.SetParent(caller);

            //Unparent container in scene
            testObject.transform.SetParent(null);

            //Add LevelManager
            testObject.AddComponent<LevelManager>();

            //Move to top of list for sanity's sake
            testObject.transform.SetAsFirstSibling();
        }
    }

#if UNITY_EDITOR
    //GUI Methods
    private void DrawQuestWindow(int windowID)
    {
        //Draw close button
        if(GUI.Button(new Rect(questWindowWidth - 20f, 0, 20f, 20f), "X"))
            showQuests = false;

        //Draw Quest List
        if (playerQuests == null) playerQuests = GameDatabase.sPlayerData.quests;
        else
        {
            int c = 0;
            QuestTemplate quest;
            Vector2 questLabelSize = new Vector2(150f, textHeight);
            Vector2 questButtonSize = new Vector2(80f, textHeight);

            //Iterate through all quests in the database
            for (int i = 0; i < GameDatabase.core.quests.Length; i++)
            {
                //Fetch quest template
                quest = GameDatabase.core.quests[i];
                //See if the player has an entry for this quest
                int index = playerQuests.FindIndex(x => x.id == quest.saveID);

                //Cache or create saveQuest struct based on the findings
                SaveQuest saveQuest;
                if (index > -1) saveQuest = playerQuests[index];
                else saveQuest = new SaveQuest(quest.saveID, QuestState.Inactive, 0);

                //Draw quest name
                GUI.Label(
                    new Rect(
                        new Vector2(
                            leftMargin,
                            topMargin + (c * textHeight) + spacing),
                        questLabelSize),
                    quest.name);

                //Draw quest state button
                if (GUI.Button(
                    new Rect(
                        new Vector2(
                            leftMargin + questLabelSize.x + spacing,
                            topMargin + (c * textHeight) + spacing),
                        questButtonSize),
                    saveQuest.state.ToString()))
                {
                    //Set new state
                    saveQuest.state = saveQuest.state.Next();

                    //Add/Replace the list entry with the updated one
                    if (index > -1) playerQuests[index] = saveQuest;
                    else playerQuests.Add(saveQuest);
                }

                //Increment count
                c++;
            }
        }
    }
    private void OnGUI()
    {
        //Only do this in testing mode
        if (!testing) return;

        //Draw Developer Quest View
        if (showQuests)
        {
            //Draw container window
            GUI.Window(0,
                //Container Rect
                new Rect(
                    //Position
                    leftMargin, topScreen,
                    //Size
                    questWindowWidth, questWindowHeight + GameDatabase.core.quests.Length * (textHeight + spacing)),
                //Function
                DrawQuestWindow,
                //Label
                "Quests");
        }
        else
        {
            if (GUI.Button(new Rect(leftMargin, topScreen, 60f, textHeight), "Quests"))
                showQuests = true;
        }
    }
#endif
}
