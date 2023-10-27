using UnityEngine;
using System.Collections;

public class CharacterCreationManager : MonoBehaviour
{
    public CharacterSheet characterSheet;
    public CharacterCreationTemplate[] characterCreationTemplates;
    public Transform spawnPoint;
    public Camera cam;

    Actor actor = null;

    IEnumerator InitGUI()
    {
        while (!GUIManager.instance) yield return new WaitForEndOfFrame();

        GUIManager.instance.toggleGUI(GUIManager.GUISelection.CharacterCreation, true);
    }

    void Start()
    {
        //Connect to the GUI's changes event
        GUI_CharacterCreation.OnCharacterSubmit += SubmitChoices;

        SetupChoice(0);

        StartCoroutine(InitGUI());
    }

    void ChangeActor(int templateIndex)
    {
        if(actor != null)
        {
            Destroy(actor.gameObject);
            actor = null;
        }

        //Create a new character creation player model
        var GO = Instantiate(characterCreationTemplates[templateIndex].prefab, spawnPoint, false);
        actor = GO.GetComponent<Actor>();
    }

    public void Deactivate()
    {
        GUI_CharacterCreation.OnCharacterSubmit -= SubmitChoices;
    }
    bool ValidateChoices()
    {
        //Validate Stats
        if (characterSheet.availableAttributePoints.valueBaseTemp > 0)
        {
            Alerter.ShowMessage("You must assign all your free stat points!");
            return false;
        }

        return true;
    }
    public void SetupChoice(int templateIndex)
    {
        //bool needStatReset = characterSheet.raceID != race;
        //Update character sheet data
        characterSheet.CopyFrom(
            characterCreationTemplates[templateIndex].characterSheet,
            false);

        //Update visuals
        ChangeActor(templateIndex);

        //Update gui?
    }
    public void SubmitChoices()
    {
        if (ValidateChoices())
        {
            GUI_YesNoBox.instance.Show("Play Tutorial / Intro?", true,
                NewGameTutorial,
                NewGameNoTutorial);
        }
    }
    void NewGameTutorial()
    {
        StartNewGame(true);
    }
    void NewGameNoTutorial()
    {
        StartNewGame(false);
    }
    void StartNewGame(bool doTutorial)
    {
        GUI_Common.instance.OnCharacterCreation(false);
        Deactivate();
        GameDatabase.core.NewGame(doTutorial, false);
    }
}