using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    const float poolReturnDelay = 0.01f;
    const int musicPlayerCount = 3;
    const string poolObjectName = "Pooled Sound";
    const string musicAudioSourceName = "Music Player";

    [System.Serializable]
    public struct TrackRequest
    {
        public MusicTrackTemplate track;
        public float transitionTime;

        public TrackRequest(MusicTrackTemplate track, float transitionTime = 5f)
        {
            this.track = track;
            this.transitionTime = transitionTime;
        }
    }
    class MusicPlayer
    {
        public enum State { Stopped, FadingIn, Playing, FadingOut }
        public AudioSource _as = null;
        public State state = State.Stopped;

        float fadeTime = 0;
        float fadeTimer = 0;
        float targetVolume = 0;

        public MusicPlayer(AudioSource audioSource)
        {
            _as = audioSource;
        }
        public void Play(TrackRequest trackRequest)
        {
            _as.Stop();
            _as.volume = 0;
            _as.clip = trackRequest.track.clip;
            _as.pitch = trackRequest.track.pitch;
            targetVolume = trackRequest.track.volume;
            _as.Play();

            fadeTime = trackRequest.transitionTime;
            fadeTimer = fadeTime;

            state = State.FadingIn;
        }
        public void Stop(float fadeTime)
        {
            if ((state == State.Playing) || (state == State.FadingIn))
            {
                //Cache the fadeTime
                this.fadeTime = fadeTime;

                //Set the timer based on how far the FadeIn got
                fadeTimer = (_as.volume / targetVolume) * fadeTime;

                //Update the state
                state = State.FadingOut;
            }
        }
        public void Update(float deltaTime)
        {
            //Update timer
            if(fadeTimer > 0) fadeTimer -= deltaTime;

            //Handle fading states
            switch (state)
            {
                case State.FadingIn:
                    _as.volume = Mathf.Min(targetVolume, targetVolume - (targetVolume * (fadeTimer / fadeTime)));
                    if (_as.volume >= targetVolume) state = State.Playing;
                    break;
                case State.FadingOut:
                    _as.volume = Mathf.Max(0, targetVolume * (fadeTimer / fadeTime));
                    if (_as.volume <= 0) state = State.Stopped;
                    break;
            }
        }
    }

    public static AudioManager instance = null;

    [Tooltip("Number of sound sources to pre-make")]
    [SerializeField] int startingPoolSize = 32;
    [Tooltip("Maximum number of sound sources that can be made before play requests are refused")]
    [SerializeField] int maxPoolSize = 256;
    [Tooltip("Distance at which sounds will no longer be played")]
    [SerializeField] float ignoreDistance = 50f;
    [SerializeField] AudioMixerGroup musicMixer;

    //Used for efficient audiosource pool selection
    int poolPointer = 0;

    //Cache
    AudioSource template = null;

    //Stores a cache of audiosources to be used and put back...
    //... instead of constantly instantiating and destorying objects
    List<AudioSource> audioSourcePool = null;
    Stack<TrackRequest> musicQueue = null;

    //Music players
    MusicPlayer[] musicPlayers = null;
    MusicPlayer lastMusicPlayer = null;

    //Transform caching for further efficiency gains
    Transform poolTransform = null;
    Transform musicTransform = null;
    Transform _transform = null;
    Transform _listenerTransform = null;
    Transform listener = null;

    //Accessor to ensure that only active audio listeners are used (when possible)
    Transform listenerTransform
    {
        get
        {
            if ((_listenerTransform == null) || (!_listenerTransform.gameObject.activeInHierarchy))
            {
                //Clear old reference
                _listenerTransform = null;
                
                //Find first active listener 
                AudioListener listener = FindObjectOfType<AudioListener>();

                //If a listener was found, cache the reference to its transform
                if (listener)
                   _listenerTransform = listener.transform;

                //If no listener was found post a log and leave the reference null for external checks
                if (_listenerTransform == null)
                    Debug.LogWarning("No active audio listener in scene!");
            }

            return _listenerTransform;
        }
    }

    //Testing
    public AudioGroupTemplate testSound;
    public MusicTrackTemplate testMusic;

    //Initialization Methods
    AudioSource CreateMusicAudioSource(Transform host)
    {
        AudioSource AS;

        //Add the audiosource component
        AS = host.gameObject.AddComponent<AudioSource>();

        //Set default properties
        AS.playOnAwake = false;

        //Don't pause music when game is paused
        AS.ignoreListenerPause = true;

        //Music will be looping in most cases
        AS.loop = true;

        //Make sure music never gets stopped by external optimizations
        AS.priority = 0;

        //Assign mixer
        AS.outputAudioMixerGroup = musicMixer;

        return AS;
    }
    AudioSource CreatePooledAudioSource()
    {
        GameObject GO;
        AudioSource AS;

        //Make new gameobject
        GO = new GameObject(poolObjectName);

        //Parent it
        GO.transform.SetParent(poolTransform);

        //Add the audiosource component
        AS = GO.AddComponent<AudioSource>(template);

        //Put into the pool
        audioSourcePool.Add(AS);

        //Turn off until needed
        GO.SetActive(false);

        return AS;
    }
    void InitPool()
    {
        //Initialize list
        audioSourcePool = new List<AudioSource>(startingPoolSize);

        //Create a Pool Parent object for neatness' sake
        poolTransform = (new GameObject("Pool")).transform;
        poolTransform.SetParent(_transform);

        //Create the new objects
        for (int i = 0; i < startingPoolSize; i++)
            CreatePooledAudioSource();
    }
    void InitMusic()
    {
        //Create Music queue
        musicQueue = new Stack<TrackRequest>();

        //Create a Parent object for neatness' sake
        musicTransform = (new GameObject("Music")).transform;
        musicTransform.SetParent(_transform);

        //Create the new objects
        musicPlayers = new MusicPlayer[musicPlayerCount];
        for (int i =0; i < musicPlayerCount; i++)
            musicPlayers[i] = new MusicPlayer(CreateMusicAudioSource(musicTransform));
    }

    //Testing Methods
    void HandleTesting()
    {
        //Sounds
        if (testSound)
        {
            Play(testSound);
            testSound = null;
        }

        //Music
        if (testMusic)
        {
            PlayMusic(new TrackRequest(
                testMusic,
                5f),
                2.5f);
            testMusic = null;
        }
    }

    //Mono Methods
    void Awake()
    {
        //Set me
        instance = this;

        template = GetComponent<AudioSource>();

        //Cache my transform
        _transform = transform;

        //Initialize Audio Listener
        GameObject GO = new GameObject("Listener", typeof(AudioListener));
        listener = GO.transform;
        listener.SetParent(_transform);

        //Initialize the Audio Source Pool
        InitPool();

        //Initialize Music Pool and cached properties
        InitMusic();
    }
    void Update()
    {
        //Testing
        HandleTesting();

        //If both music players are busy we have to wait for them to finish
        if (musicQueue.Count > 0)
        {
            //Get rid of any extra requests
            while (musicQueue.Count > 1)
                musicQueue.Pop();

            //Play Request if possible (otherwise wait until next Update tick)
            MusicPlayer mp = GetFreeMusicPlayer();
            if (mp != null)
                mp.Play(musicQueue.Pop());
        }

        //Tick music players
        for (int i = 0; i < musicPlayerCount; i++)
            musicPlayers[i].Update(Time.unscaledDeltaTime);
    }

    //Pooling Methods
    void IncrementPoolPointer()
    {
        poolPointer++;
        if (poolPointer >= audioSourcePool.Count)
            poolPointer = 0;
    }
    AudioSource GetAudioSource()
    {
        AudioSource ret = null;

        //Find a free sound object
        for (int i = 0; i < startingPoolSize; i++)
        {
            //Check if audiosource was destroy
            //This may happen if the source was playing on an object that was destroyed
            if(audioSourcePool[poolPointer] == null)
                audioSourcePool[poolPointer] = CreatePooledAudioSource();

            //Check if the audiosource is not in use
            if (!audioSourcePool[poolPointer].gameObject.activeSelf)
            {
                //Set the return reference
                ret = audioSourcePool[poolPointer];
                //Increment the pointer
                IncrementPoolPointer();
                //Discontinue the loop, we're done here
                break;
            }

            IncrementPoolPointer();
        }

        //If no sound objects were free, make a new one
        if ((ret == null) && (audioSourcePool.Count < maxPoolSize))
        {
            //Create new one
            ret = CreatePooledAudioSource();
            //Reset pointer
            poolPointer = 0;
        }

        if (ret == null) Debug.LogWarning("Audio Pool maxxed out!");

        return ret;
    }
    AudioSource GetAudioSource(Vector3 position)
    {
        AudioSource ret = GetAudioSource();

        if(ret != null)
            ret.transform.position = position;

        return ret;
    }
    AudioSource GetAudioSource(Transform trans)
    {
        AudioSource ret = GetAudioSource();

        if (ret != null)
        {
            ret.transform.SetParent(trans);
            ret.transform.localPosition = Vector3.zero;
        }

        return ret;
    }

    //Sound Play Methods
    void SetupAudioSourceAndPlay(AudioGroupTemplate audioGroup, AudioSource audioSource)
    {
        if (audioGroup != null)
        {
            audioSource.outputAudioMixerGroup = audioGroup.mixer;
            audioSource.clip = audioGroup.playClip;
            audioSource.volume = audioGroup.volume;
            audioSource.pitch = audioGroup.playPitch;
            audioSource.ignoreListenerPause = audioGroup.ignorePause;

            //Don't bother if there is no listener
            if (listenerTransform != null)
            {
                audioSource.gameObject.SetActive(true);
                audioSource.Play();

                //Set source to return to pool after it is finished playing
                StartCoroutine(ReturnToPoolWhenFinished(
                    //(audioSource.clip.length * -audioSource.pitch) + poolReturnDelay, 
                    audioSource));
            }
        }
    }

    public void Play(AudioGroupTemplate audioGroup)
    {
        //Fetch audiosource and position on listener (no 3d effect)
        AudioSource audioSource = GetAudioSource(listenerTransform);

        //Set audiosource properties and play sound
        SetupAudioSourceAndPlay(audioGroup, audioSource);
    }
    public void Play(AudioGroupTemplate audioGroup, Vector3 position)
    {
        if (Vector3.Distance(position, listenerTransform.position) <= ignoreDistance)
        {
            //Fetch audiosource
            AudioSource audioSource = GetAudioSource(position);

            if (audioSource != null)
            {
                //Set audiosource properties and play sound
                SetupAudioSourceAndPlay(audioGroup, audioSource);
            }
        }
    }
    public void Play(AudioGroupTemplate audioGroup, Transform trans)
    {
        if (Vector3.Distance(trans.position, listenerTransform.position) <= ignoreDistance)
        {
            //Fetch audiosource
            AudioSource audioSource = GetAudioSource(trans);

            if (audioSource != null)
            {
                //Set audiosource properties and play sound
                SetupAudioSourceAndPlay(audioGroup, audioSource);
            }
        }
    }
    /// <summary>
    /// Stops all playing sound effects
    /// </summary>
    public void StopAll()
    {
        //Stop all "return to pool" coroutines as they are no longer neccessary
        StopAllCoroutines();

        //Stop and return all sounds to the pool
        for (int i = 0; i < audioSourcePool.Count; i++)
        {
            //Make sure the object has not been destroyed
            if(audioSourcePool[i] != null)
                audioSourcePool[i].Stop();

            ReturnToPool(audioSourcePool[i]);
        }
    }
    void ReturnToPool(AudioSource audioSource)
    {
        //If audio source was not destroyed while playing, don't bother
        if (audioSource != null)
        {
            //Disable (mark for pool re-use)
            audioSource.gameObject.SetActive(false);

            //Reparent back to pool (prevents excessive destruction of pooled objects)
            audioSource.transform.SetParent(poolTransform);
        }
    }
    IEnumerator ReturnToPoolWhenFinished(AudioSource audioSource)
    {
        //Wait for sound to finish playing
        while (audioSource && audioSource.isPlaying)
            yield return new WaitForEndOfFrame();

        //Return sound object to pool
        if(audioSource) ReturnToPool(audioSource);
    }

    //Music Play Methods
    public void PlayMusic(TrackRequest track, float fadeOutOtherMusicTime = 2.5f)
    {
        //Don't play duplicates
        if ((lastMusicPlayer == null) || (lastMusicPlayer._as.clip != track.track.clip))
        {
            if(fadeOutOtherMusicTime > 0) StopMusic(fadeOutOtherMusicTime);
            musicQueue.Push(track);
        }
    }
    public void StopMusic(float fadeTime)
    {
        for (int i = 0; i < musicPlayerCount; i++)
            musicPlayers[i].Stop(fadeTime);
    }
    MusicPlayer GetFreeMusicPlayer()
    {
        for (int i = 0; i < musicPlayerCount; i++)
            if (musicPlayers[i].state == MusicPlayer.State.Stopped)
            {
                lastMusicPlayer = musicPlayers[i];
                return lastMusicPlayer;
            }

        return null;
    }
    
    //Pausing Methods
    public void Pause()
    {
        if (listenerTransform != null)
        {
            AudioListener.pause = true;
        }
        else
            Debug.LogWarning("Could not pause sound - no audio listener!");
    }
    public void Unpause()
    {
        if (listenerTransform != null)
        {
            AudioListener.pause = false;
        }
        else
            Debug.LogWarning("Could not pause sound - no audio listener!");
    }

    //Listener Methods
    public Transform AttachListener(Transform attachmentPlace)
    {
        listener.SetParent(attachmentPlace);
        listener.localPosition = Vector3.zero;

        return listenerTransform;
    }
    public void DetachListener()
    {
        listener.SetParent(_transform);
        listener.localPosition = Vector3.zero;
    }
}