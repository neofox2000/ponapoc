using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Selector : MonoBehaviour
{
    public Selectable[] selectionOrder;

    IEnumerator selectFirstEnabled()
    {
        bool found = false;

        while (!found)
        {
            yield return new WaitForEndOfFrame();
            foreach (Selectable selectable in selectionOrder)
                if (selectable.IsActive() && selectable.IsInteractable())
                {
                    selectable.Select();
                    found = true;
                    break;
                }
        }
    }
    /*
    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (EventSystem.current.currentSelectedGameObject == null)
                StartCoroutine(selectFirstEnabled());
        }
    }
    */
    private void OnEnable()
    {
        StartCoroutine(selectFirstEnabled());
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }
}