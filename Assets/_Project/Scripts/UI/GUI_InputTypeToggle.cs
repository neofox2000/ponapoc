using UnityEngine;

/// <summary>
/// Enables/Disables specified objects based on detected input type
/// </summary>
public class GUI_InputTypeToggle : MonoBehaviour
{
    [Tooltip("These objects will be enabled when keyboard/mouse input is detected")]
    public RectTransform[] keyboardMouseToggles;
    [Tooltip("These objects will be enabled when controller input is detected")]
    public RectTransform[] controllerToggles;

    LoadingManager.InputMethod lastInputMethod;

    private void Awake()
    {
        //Cache input method
        lastInputMethod = LoadingManager.InputMethod.KeyMouse;

        //Do an initial update
        UpdateObjects();
    }
    private void Update()
    {
        //Check for input changes
        if (LoadingManager.currentInputMethod != lastInputMethod)
            UpdateObjects();
    }
    private void UpdateObjects()
    {
        //Cache input method
        lastInputMethod = LoadingManager.currentInputMethod;

        //Convert to simple booleans for quick use
        bool showController = lastInputMethod == LoadingManager.InputMethod.Controller;
        bool showKeyboardMouse = lastInputMethod == LoadingManager.InputMethod.KeyMouse;

        //Set all object states
        foreach (RectTransform obj in keyboardMouseToggles)
            obj.gameObject.SetActive(showKeyboardMouse);
        foreach (RectTransform obj in controllerToggles)
            obj.gameObject.SetActive(showController);
    }
}