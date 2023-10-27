using UnityEngine;

[CreateAssetMenu(menuName = "Templates/Character")]
public class CharacterTemplate : ScriptableObject
{
    [Tooltip("Unique ID used to save the player's choice of character")]
    public int saveID;
    [Tooltip("Is this character a possible chocie during character creation?")]
    public bool playable = false;
    public CharacterAnimationProfile animationProfile;
    public CharacterSoundsTemplate soundsTemplate;
    public WeaponAnimationProfile weaponAnimationProfile;
    [Tooltip("The conversation flowchart (stored as a prefab)")]
    public GameObject conversationPrefab;
    public GameObject hitEffect;
    public GameObject criticalHitEffect;

    [Header("Behaviour")]
    [Tooltip("Equip armor in starting inventory when spawning?")]
    public bool equipArmor = true;
    [Tooltip("Equip a weapon from starting inventory when spawning?")]
    public bool equipWeapons = false;


    #region Quick-Access Methods
    public AudioGroupTemplate GetSound(int soundIndex)
    {
        if ((soundsTemplate.sounds.Length > 0) && (soundsTemplate.sounds.Length > soundIndex))
            return soundsTemplate.sounds[soundIndex].sound;

        return null;
    }
    public AudioGroupTemplate GetSound(CharacterSoundsTemplate.SoundKeys key)
    {
        foreach (CharacterSoundsTemplate.CharacterSound characterSound in soundsTemplate.sounds)
            if (characterSound.key == key)
                return characterSound.sound;

        return null;
    }
    #endregion
}