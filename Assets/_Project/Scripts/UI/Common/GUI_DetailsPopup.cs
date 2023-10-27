using UnityEngine;
using UnityEngine.UI;

public class GUI_DetailsPopup : GUI_HoverPanel 
{
    public Text label;

    public void Setup(string text, RectTransform target, TextAlignment preferredAlignment, bool show = true)
    {
        label.text = text;
        switch(preferredAlignment)
        {
            case TextAlignment.Center:
                label.alignment = TextAnchor.UpperCenter;
                break;
            case TextAlignment.Left:
                label.alignment = TextAnchor.UpperRight;
                break;
            default:
                label.alignment = TextAnchor.UpperLeft;
                break;
        }
        base.Setup(target, preferredAlignment, show);
    }
}
