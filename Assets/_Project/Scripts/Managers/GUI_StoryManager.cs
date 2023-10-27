using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GameDB;

public class GUI_StoryManager : MonoBehaviour 
{
    [SerializeField] Metagame metagame;
    public float fadeTime = 5f;
    [Range(0.1f, 1f)]
    public float speedStep = 0.5f;
    public float bottomPadding = 100f;
    public string animKey = "Storying";
    public RectTransform storyPanel;
    public Text storyTitle, storyBody, timerText;
    public AnimationCurve scrollCurve;

    //Privates Properties
    int akhGo;
    bool finished = true, doneFadeOut = false;
    float speed = 1f, storyTime = 0, storyTimer = 0, scrollDistance = 0;
    Animator animator;
    StoryTemplate currentStory = null;


    //Story playback 
    void UpdateAnimatorSpeed()
    {
        animator.speed = speed;
        //Debug.Log("animator.speed = " + animator.speed);
    }
    public void OnResetPlaybackSpeed()
    {
        speed = 1;
        UpdateAnimatorSpeed();
    }
    public void OnFastForward()
    {
        speed += 0.5f;
        UpdateAnimatorSpeed();
    }
    public void OnSlowDown()
    {
        if ((speed - 0.5f) > 0) speed -= 0.5f;
        UpdateAnimatorSpeed();
    }

    void UpdateStoryTimer()
    {
        storyTimer -= Time.unscaledDeltaTime * speed;
        timerText.text = Mathf.RoundToInt(storyTimer).ToString();
    }
    void HandleTextFadeOut()
    {
        //Handle text fading
        if (!doneFadeOut)
        {
            if (storyTimer <= fadeTime)
            {
                //Trigger fadeout
                doneFadeOut = true;
                animator.SetBool(akhGo, false);
            }
        }
    }
    void HandleTextPositioning()
    {
        //Center text if all fits on screen, otherwise scroll using animation curve
        if ((Screen.height - bottomPadding) < storyBody.rectTransform.rect.height)
        {
            //Calculate if how much scrolling needs to be done
            float animTimeStep = 1 - (storyTimer / storyTime);
            scrollDistance = storyBody.rectTransform.rect.height - (Screen.height - bottomPadding);

            storyPanel.anchoredPosition = new Vector2(
                storyPanel.anchoredPosition.x,
                scrollDistance * scrollCurve.Evaluate(animTimeStep));
        }
        else
        {
            //Center text on screen
            storyPanel.anchoredPosition = new Vector2(
                storyPanel.anchoredPosition.x,
                -1 * ((Screen.height / 2) - 
                    ((storyBody.rectTransform.rect.height + storyTitle.rectTransform.rect.height) / 2)));
        }
    }
    void Update()
    {
        if (!finished)
        {
            if (storyTimer > 0)
            {
                UpdateStoryTimer();
                HandleTextFadeOut();
                HandleTextPositioning();
            }
            else
            {
                finished = true;
                OnSkip();
            }
        }
    }
    void ExecuteStoryAction(StoryTemplate.StoryActions action)
    {
        switch(action)
        {
            case StoryTemplate.StoryActions.Pause:
                //MetagameManager.instance.updatePauseState();
                //MetagameManager.paused = true;
                break;
            case StoryTemplate.StoryActions.UnPause:
                //MetagameManager.instance.updatePauseState();
                //MetagameManager.paused = false;
                break;
            case StoryTemplate.StoryActions.LoadStoryTeller:
                LoadingManager.SwitchScene(LoadingManager.SceneSelection.storyTeller);
                break;
            case StoryTemplate.StoryActions.LoadTitle:
                metagame.currentState = Metagame.MetaGameStates.TitleMenu;
                LoadingManager.SwitchScene(LoadingManager.SceneSelection.title);
                break;
            case StoryTemplate.StoryActions.LoadMission:
                LoadingManager.SwitchScene(LoadingManager.SceneSelection.mission);
                break;

            default:
                return;
        }
    }

    //Scene control
    public void OnSkip()
    {
        deactivate();
        //MetagameManager.instance.startMission();
        //if (OnFinished != null) OnFinished();
    }
    public void PlayStory(StoryTemplate story)
    {
        gameObject.SetActive(true);

        currentStory = story;
        if (currentStory != null)
        {
            storyTitle.text = currentStory.heading;
            storyTitle.color = currentStory.headingTextColor;
            storyBody.text = currentStory.body;
            storyBody.color = currentStory.bodyTextColor;

            //Calculate how long the story should remain on screen
            storyTime = currentStory.body.Length / 15;
            //Start the timer
            storyTimer = storyTime;

            //Initialize components
            if (!animator)
            {
                animator = GetComponent<Animator>();
                akhGo = Animator.StringToHash(animKey);
            }

            //Reset speeds
            OnResetPlaybackSpeed();

            //Go
            StartCoroutine(activation());
        }
        else Debug.LogError("Invalid storyID found while activating StoryManager!");
    }
    public void deactivate()
    {
        GUIManager.instance.menusVisible = false;
        StartCoroutine(deactivation());
    }

    IEnumerator deactivation()
    {
        animator.StopPlayback();

        yield return new WaitForEndOfFrame();

        storyPanel.gameObject.SetActive(false);
        ExecuteStoryAction(currentStory.after);

        yield return new WaitForEndOfFrame();

        gameObject.SetActive(false);
    }
    IEnumerator activation()
    {
        ExecuteStoryAction(currentStory.before);

        yield return new WaitForEndOfFrame();

        storyPanel.gameObject.SetActive(true);

        yield return new WaitForEndOfFrame();

        animator.SetBool(akhGo, true);
        finished = false;
        doneFadeOut = false;

        GUIManager.instance.menusVisible = true;
    }
}
