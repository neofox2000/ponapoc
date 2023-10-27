using UnityEngine;

public class HUD_LevelPanel : MonoBehaviour
{
    public RectTransform levelUpIndicator;

    void FixedUpdate()
    {
        UpdateDisplay(GameDatabase.lCharacterSheet.availableSkillPoints.valueModded > 0f);
    }
    void UpdateDisplay(bool show)
    {
        if (!levelUpIndicator)
        {
            Debug.LogWarning("No Level Up Indicator assigned to "+name+" in " +transform.parent.name+"!");
            return;
        }

        levelUpIndicator.gameObject.SetActive(show);
    }
}