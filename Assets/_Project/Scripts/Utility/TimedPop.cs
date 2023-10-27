using UnityEngine;

public class TimedPop : MonoBehaviour
{
    const string poolName = "Pool";

    public float timeToLive = 5f;
    public bool usePoolManager = true;
    public bool useDisable = false;
    public bool useAudioTime = false;
    public bool useParticleSystemLifeTime = false;

    float audioTime = 0f;
    float timeToLiveTimer = 0;

    public void resetTimer()
    {
        timeToLiveTimer = 0;
    }
    public void Start()
    {
        resetTimer();
        if ((useAudioTime) && (GetComponent<AudioSource>()))
        {
            timeToLive = GetComponent<AudioSource>().clip.length * GetComponent<AudioSource>().pitch;
            audioTime = timeToLive;
        }

        if (useParticleSystemLifeTime)
        {
            ParticleSystem PS = (ParticleSystem)GetComponent(typeof(ParticleSystem));
            if (PS)
            {
                if (!PS.main.loop)
                {
                    if ((PS.main.duration + PS.main.startDelay.constant) > audioTime)
                        timeToLive = PS.main.duration + PS.main.startDelay.constant;
                }
            }
        }
    }
    void OnEnable()
    {
        if (useDisable)
            resetTimer();
    }
    void Update()
    {
        if (timeToLiveTimer < timeToLive)
            timeToLiveTimer += Time.deltaTime;
        else
            Pop();
    }
    void Pop()
    {
        if (usePoolManager)
        {
            timeToLiveTimer = 0;
            if (gameObject.activeSelf)
                Common.poolDespawn(transform);
                //PathologicalGames.PoolManager.Pools[poolName].Despawn(transform);
        }
        else if (useDisable)
            gameObject.SetActive(false);
        else
            Destroy(gameObject);
    }
}
