using UnityEngine;
using UnityEngine.UI;

public class GUI_StatusEffectBarIcon : MonoBehaviour 
{
    GUI_StatusEffectsBar buffBarRef;
    Image _sprite;
    public Image sprite
    {
        get { return _sprite; }
        set { _sprite = value; }
    }
    StatusEffect _statusEffectRef;
    public StatusEffect statusEffectRef
    {
        get { return _statusEffectRef; }
        set { _statusEffectRef = value; }
    }

	void Awake()
    {
        sprite = GetComponent<Image>();
        buffBarRef = transform.parent.GetComponent<GUI_StatusEffectsBar>();
	}
    public void OnHover()
    {
        buffBarRef.OnHover(this);
    }
    public void OnHoverOut()
    {
        buffBarRef.OnHoverOut();
    }
}
