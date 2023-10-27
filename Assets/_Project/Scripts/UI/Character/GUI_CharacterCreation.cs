using System;
using UnityEngine;

public class GUI_CharacterCreation : MonoBehaviour
{
    public static Action OnCharacterSubmit;

    //UI Events
    public void OnSubmit()
    {
        OnCharacterSubmit?.Invoke();
    }
}