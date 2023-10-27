using UnityEngine;

public class CharacterCreationController : ActorController
{
    [Tooltip("Visual Model")]
    public GameObject[] modelChoices;
    [Tooltip("Pre-made Character Sheets")]
    public CharacterSheet[] buildChoices;

    /*
    //Player-specific initialization methods
    override protected void BeforeInit()
    {
        base.BeforeInit();

        //Put a copy of the build template into the player data
        GameDatabase.sPlayerData.characterSheet =
            ScriptableObject.Instantiate<CharacterSheet>(buildChoices[0]);
    }
    */
}