using UnityEngine;

/// <summary>
/// Allows showing/hiding of panels using fading tweens
/// Uses a CanvasGroup for fading, interaction and raycast blocking
/// </summary>
public class PanelFader : MonoBehaviour
{
    enum States { Hidden, Showing, Shown, Hiding }

    [SerializeField] CanvasGroup canvasGroup;
    [Range(0.1f, 10f)]
    [SerializeField] float fadeInSpeed = 3f;
    [Range(0.1f, 10f)]
    [SerializeField] float fadeOutSpeed = 1f;
    [SerializeField] bool useUnscaledTime = true;

    bool autoHide = false;
    float hideTimer = 0f;

    States _state = States.Hidden;
    States state
    {
        get { return _state; }
        set
        {
            _state = value;

            bool setting = value == States.Shown;
            canvasGroup.blocksRaycasts = setting;
            canvasGroup.interactable = setting;
        }
    }

    public bool isShowing
    {
        get { return state != States.Hidden; }
    }

    void Awake()
    {
        //Add canvasGroup if needed
        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        //Setup initial values for canvas group
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
    void Update()
    {
        if (autoHide)
        {
            if (hideTimer > 0)
            {
                hideTimer -= useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            }
            else
            {
                Hide();
            }
        }

        switch (state)
        {
            case States.Showing:
                if (canvasGroup.alpha < 1f)
                {
                    canvasGroup.alpha += Time.unscaledDeltaTime * fadeInSpeed;
                }
                else
                {
                    state = States.Shown;
                }
                break;
            case States.Hiding:
                if (canvasGroup.alpha > 0f)
                {
                    canvasGroup.alpha -= Time.unscaledDeltaTime * fadeOutSpeed;
                }
                else
                {
                    state = States.Hidden;
                }
                break;
        }
    }
    
    /// <summary>
    /// Fade in the panel and automatically fade it back out after <paramref name="autoHideTime"/> seconds
    /// </summary>
    /// <param name="autoHideTime">0 = No auto hide</param>
    public void Show(float autoHideTime)
    {
        autoHide = autoHideTime > 0;
        if (autoHide) hideTimer = autoHideTime;
        state = States.Showing;
    }
    /// <summary>
    /// Fade out panel immediately
    /// </summary>
    public void Hide()
    {
        autoHide = false;
        state = States.Hiding;
    }
}