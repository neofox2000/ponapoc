using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

public class CameraController : MonoBehaviour 
{
    //Constants
    public const float maxShakeMagnitude = 20f;
    const float cameraShakeMagnitudeCutOff = 0.15f;    

    //Singleton
    public static CameraController instance;

    #region Public Properties
    //Public properties
    public AnimationCurve 
        smoothCurve, 
        cinematicCurve;
    public Vector3 
        targetOffset, 
        audioListenerOffset;
    public float
        smoothingSpeed = 1,
        baseDistance = 1,
        YHeight = 1.5f,
        XBias = 4f,
        baseSize = 10f,
        cinematicSize = 6f;
    #endregion
    #region Private properties
    Camera myCam;
    Transform myTrans;
    Transform targetTrans;
    Transform listerTransform;
    bool currrentCinameticMode = false;
    int lastDir = 0;

    float
        lastSize, targetSize,
        cinematicHeightOffset = 0,
        smoothingTimer = 0;

    Vector3
        lerpTargetOffset = Vector3.zero,
        lastLerpTargetOffset = Vector3.zero,
        lastTargetOffset = Vector3.zero;

    Actor target;
    EZCameraShake.CameraShaker shaker;
    #endregion

    Vector3 CalculatePosition()
    {
        float locationOffsetY;
        if (target.currentRoom == null)
            locationOffsetY = 0;
        else
            //locationOffsetY = LevelManager.locationSpacing * target.currentRoom.id;
            locationOffsetY = target.currentRoom.verticalOffset;

        return new Vector3(
            targetTrans.position.x,
            YHeight + locationOffsetY + cinematicHeightOffset,
            targetTrans.position.z - baseDistance)

            + targetOffset;
    }

    //Initialization Methods
    void SetupAudioListener()
    {
        listerTransform = AudioManager.instance.AttachListener(myTrans);
        listerTransform.localPosition = audioListenerOffset;
    }
    IEnumerator AwakeDelayed()
    {
#if UNITY_EDITOR
        bool problem = false;
        if (!AudioManager.instance)
        {
            //Flag initial problem for future message if/when resolved
            problem = true;
            Debug.LogWarning("No AudioManager found - waiting for AudioManager to be created...");
        }
#endif
        while (!AudioManager.instance)
            yield return new WaitForEndOfFrame();

        SetupAudioListener();

#if UNITY_EDITOR
        //Display this only if there was a problem initially
        if (problem)
            Debug.Log("Found AudioManager, audio should work normally now.");
#endif
    }

    //Monobehaviour methods
    void Awake()
    {
        instance = this;
        lastSize = baseSize;
        targetSize = baseSize;
        myTrans = transform;
        myCam = GetComponentInChildren<Camera>();
        shaker = GetComponentInChildren<EZCameraShake.CameraShaker>();

        //Setup things that rely on Singletons which many not be created yet
        StartCoroutine(AwakeDelayed());
    }
    void Update()
    {
        if (targetTrans)
        {
            Vector3 curPos = CalculatePosition();

            //Update smoothing timer
            if (smoothingTimer > 0)
            {
                if (currrentCinameticMode || (!Mathf.Approximately(myCam.orthographicSize, baseSize)))
                    smoothingTimer -= Time.unscaledDeltaTime;
                else
                    if (target.motion != Character.Motions.Idle)
                    smoothingTimer -= Time.deltaTime;
            }

            SetLerpTarget(true, currrentCinameticMode);

            //Smooth-update camera
            if (smoothingSpeed > 0)
            {
                //Set smoothing curve to be used
                AnimationCurve smoothingCurveToUse = smoothCurve;
                if (currrentCinameticMode) smoothingCurveToUse = cinematicCurve;

                //Lerp camera position
                myTrans.position = Vector3.Lerp(
                    curPos + lastLerpTargetOffset,
                    curPos + lerpTargetOffset,
                    smoothingCurveToUse.Evaluate((1 - (smoothingTimer / smoothingSpeed))));

                //Lerp camera zoom
                myCam.orthographicSize = lastSize -
                    ((lastSize - targetSize) *
                        smoothingCurveToUse.Evaluate(
                            (1 - (smoothingTimer / smoothingSpeed))));
            }
            else
                Debug.LogError("Camera Smooth Speed cannot be 0 or less!");
        }
    }
    void OnEnable()
    {
        if (shaker) shaker.enabled = true;
    }
    void OnDisable()
    {
        if (shaker) shaker.enabled = false;
    }
    void OnDestroy()
    {
        //Return the lister before it is destroyed along with this object
        if (AudioManager.instance)
            AudioManager.instance.DetachListener();
    }
    
    //Smoothing methods
    public void FollowTarget(Actor newTarget, bool useCinematicMode = false)
    {
        bool snapToTheTarget = false;
        if (target != newTarget)
        {
            //If valid target changed, compensate for lastLerpTarget
            if (target != null)
            {
                target.OnTeleported -= TargetTeleported;

                //lastLerpTargetOffset += target.transform.position - newTarget.transform.position;
                lastTargetOffset = new Vector3(
                    //lastLerpTargetOffset.x + Mathf.Abs(target.transform.position.x - newTarget.transform.position.x),
                    target.transform.position.x - newTarget.transform.position.x,
                    target.transform.position.y - newTarget.transform.position.y,
                    0);
            }
            //Snap to target
            else
            {
                snapToTheTarget = true;
            }
        }

        //Cache new target
        target = newTarget;

        //Cache target's transform (efficiency)
        targetTrans = target.transform;

        //Reset lerp target
        SetLerpTarget(false, useCinematicMode);

        //Subscribe to target's room change events
        target.OnTeleported += TargetTeleported;

        //Instantly move camera to new target
        if (snapToTheTarget) SnapToTarget();
    }
    void TargetTeleported(Actor targetEntity)
    {
        SnapToTarget();
    }
    public void SnapToTarget()
    {
        if (target != null)
        {
            lastLerpTargetOffset = Vector3.zero;
            lerpTargetOffset = Vector3.zero;

            myTrans.position = CalculatePosition();
        }
        else
            Debug.LogWarning("snapToTarget called on null target!");
    }
    void SetLerpTarget(bool checkLastDir = true, bool useCinematicMode = false)
    {
        //Handles smoothing offset
        if (target != null)
        {
            int direction = target.dir;
            if ((checkLastDir) && (lastDir != direction) || (currrentCinameticMode != useCinematicMode) || (lastTargetOffset != Vector3.zero))
            {
                //Set Target Position
                if (useCinematicMode)
                {
                    lastLerpTargetOffset = Vector3.zero;
                    lerpTargetOffset = Vector3.zero;
                }
                else
                {
                    if (direction != 0)
                    {
                        if (direction < 0)
                            lastLerpTargetOffset = new Vector3((myTrans.position.x - targetTrans.position.x) * +1, 0, 0);
                        else
                            lastLerpTargetOffset = new Vector3((targetTrans.position.x - myTrans.position.x) * -1, 0, 0);
                    }
                    else lastLerpTargetOffset = Vector3.zero;

                    lerpTargetOffset = new Vector3(XBias * direction * 1, 0, 0);
                }

                //Check for target change and apply offset
                if(lastTargetOffset != Vector3.zero)
                {
                    lastLerpTargetOffset = lastTargetOffset;
                    lastTargetOffset = Vector3.zero;
                }

                lastDir = direction;

                //Set Target Size
                lastSize = myCam.orthographicSize;
                targetSize = useCinematicMode ? cinematicSize : baseSize;
                //float locationOffsetY = LevelManager.locationSpacing * target.currentRoom.id;
                float locationOffsetY = target.currentRoom.verticalOffset;
                cinematicHeightOffset = useCinematicMode ? (targetTrans.position.y - locationOffsetY) : 0;
                currrentCinameticMode = useCinematicMode;

                //Start timer
                smoothingTimer = smoothingSpeed;
            }
        }
        else
        {
            lerpTargetOffset = Vector3.zero;
            smoothingTimer = 0;
        }
    }

    //Shake methods
    public void Shake(float magnitude, Vector3 emittingPosition)
    {
        float distance = Vector2.Distance(myTrans.position, emittingPosition);
        if (shaker)
        {
            //Dampen with distance and cap to ensure camera doesn't reveal too much beyond the stage borders
            float newMag = Mathf.Min(
                magnitude / (1 + distance),
                maxShakeMagnitude);

            if (newMag > cameraShakeMagnitudeCutOff)
            {
                //Debug.Log("Before " + magnitude + " / After " + newMag);
                shaker.ShakeOnce(newMag, 10f, 0.15f, 0.45f);
            }
        }
        else
            Debug.LogWarning("No Camera Shaker component!");
    }

    //Post-processing Methods
    public void SetPostProcessingProfile(PostProcessProfile profile)
    {
        //Apply profile to camera
        PostProcessVolume postTarget = myCam.GetComponent<PostProcessVolume>();
        if (postTarget)
            postTarget.profile = profile;
        else
            Debug.LogWarning("Camera has no PostProcessingBehaviour - cannot apply PostProcessingProfile!");
    }
}