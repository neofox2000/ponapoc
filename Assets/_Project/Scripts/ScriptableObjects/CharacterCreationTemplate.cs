using UnityEngine;

[CreateAssetMenu(menuName = "Data/Templates/Character Creation Template")]
public class CharacterCreationTemplate : ScriptableObject
{
    public string firstName;
    public string lastName;
    public string nickname;
    public GameObject prefab;
    public CharacterSheet characterSheet;
}