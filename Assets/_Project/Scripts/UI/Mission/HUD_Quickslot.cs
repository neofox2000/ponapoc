using UnityEngine;
using UnityEngine.UI;

public class HUD_Quickslot : MonoBehaviour
{
    const string flashAnimationName = "Flash";

    public Image icon;
    public Text readout;

    Animator animator;
    int akhFlash = Animator.StringToHash(flashAnimationName);

    void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void Flash()
    {
        animator.SetTrigger(akhFlash);
    }

    public void SetIcon(Sprite sprite, Color color)
    {
        if (sprite == null)
        {
            icon.gameObject.SetActive(false);
        }
        else
        {
            icon.sprite = sprite;
            icon.color = color;
            icon.gameObject.SetActive(true);
        }
    }
    public void SetProgress(float progress)
    {
        icon.fillAmount = progress;
    }
    public void SetReadout(string newReading)
    {
        readout.text = newReading;
        readout.gameObject.SetActive(newReading != string.Empty);
    }
    public void Clear()
    {
        SetIcon(null, Color.white);
        SetProgress(1f);
        SetReadout(string.Empty);
    }
}