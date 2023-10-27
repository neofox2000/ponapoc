using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUD_Sneaking : MonoBehaviour
{
    public enum SneakStates { None, Sneak, Detected };

    public RectTransform 
        sneakingIndicator, 
        detectedIndicator;

    public void OnSneakChange(SneakStates sneakState)
    {
        sneakingIndicator.gameObject.SetActive(false);
        detectedIndicator.gameObject.SetActive(false);

        switch (sneakState)
        {
            case SneakStates.Sneak:
                sneakingIndicator.gameObject.SetActive(true);
                break;

            case SneakStates.Detected:
                detectedIndicator.gameObject.SetActive(true);
                break;
        }
    }
}
