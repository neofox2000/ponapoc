using UnityEngine;

[RequireComponent(typeof(InteractableObject))]
public class CraftingBench : MonoBehaviour
{
    InteractableObject interactableObject;

    private void Awake()
    {
        interactableObject = GetComponent<InteractableObject>();
        interactableObject.OnInteractAccessible += interact;
    }
    void interact(Actor interactor)
    {
        PlayerController player = interactor.GetComponent<PlayerController>();
        if (player)
        {
            GUI_Common.instance.OnCrafting(true);
            GUI_Common.instance.guiCrafting.Setup(player);
        }
        else
            Debug.LogWarning("Non-Player entity is attempting to interact with me!");
    }
}
