using UnityEngine;
using UnityEngine.Rendering;

public class TracerEffect : MonoBehaviour 
{
    public float fadeTime, delay;
    public string fireAnimationKey = "Fire";
    public Color startColor, endColor;
    public float alpha = 1f;
    public string sortingLayer;

    int akFire;

    Animator animator;
    LineRenderer line;
    ParticleSystem particles;

    void Awake()
    {
        TimedPop TP = gameObject.AddComponent<TimedPop>();
        line = GetComponent<LineRenderer>();
        line.sortingLayerName = sortingLayer;
        animator = GetComponent<Animator>();
        particles = GetComponentInChildren<ParticleSystem>();
        
        akFire = Animator.StringToHash(fireAnimationKey);
        animator.speed = 1 / Mathf.Clamp(fadeTime, 0.01f, fadeTime);
        
        TP.timeToLive = fadeTime + 0.01f;
        TP.Start();
    }
    public void fire(Vector3 start, Vector3 end, SortingGroup parentSortingGroup)
    {
        if (line)
        {
            if (parentSortingGroup)
            {
                line.sortingLayerID = parentSortingGroup.sortingLayerID;
                line.sortingOrder = parentSortingGroup.sortingOrder - 1;
            }
            else
            {
                line.sortingLayerID = 0;
                line.sortingOrder = -1;
                Debug.LogWarning("Tracer.Fire got null for parentSortingGroup!");
            }

            line.SetPosition(0, start);
            line.SetPosition(1, end);
        }

        if (particles)
        {
            particles.transform.position = (start + end) / 2;
            particles.transform.localScale = new Vector3((Vector3.Distance(start, end)), 1, 1);
            particles.Play();
        }

        //Start fade animation
        animator.SetBool(akFire, true);
    }
    public void fireAnimStart()
    {
        //Prevent fade animation from looping
        animator.SetBool(akFire, false);
    }
    public void fireAnimEnd()
    {

    }
    void Update()
    {
        if ((line) && (line.enabled))
        {
            startColor.a = alpha;
            endColor.a = alpha;

            //line.SetColors(startColor, endColor);
            line.startColor = startColor;
            line.endColor = endColor;
        }
    }
}
