using UnityEngine;
using Variables;

public class HUD_FloatingBarDataPlugger : MonoBehaviour
{
    public HUD_Bar bar;
    public HUD_FloatingObject floater;
    public FloatingBarData data;

    void Awake()
    {
        if (data)
        {
            data.OnInUseChanged.AddListener(InUseChanged);
            InUseChanged();
        }
    }
    void OnDestroy()
    {
        if (data)
            data.OnInUseChanged.RemoveListener(InUseChanged);
    }
    void Update()
    {
        if (!data) return;

        bar.SetValue(data.currentProgress, data.maxProgress);
        bar.SetLabel(data.displayText);
        bar.SetColor(data.fillColor);
    }

    void InUseChanged()
    {
        gameObject.SetActive(data.GetInUse());

        if (data.GetInUse())
        {
            floater.worldOffset = data.offset;
            if(data.target) floater.fire(data.target);
            else floater.fire(data.fixedPosition);
        }
    }
}