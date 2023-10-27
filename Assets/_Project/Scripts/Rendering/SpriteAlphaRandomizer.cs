using UnityEngine;

public class SpriteAlphaRandomizer : MonoBehaviour
{
    public Vector2 alphaRange;
	void Awake ()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr)
            sr.color = new Color(
                sr.color.r, 
                sr.color.g, 
                sr.color.b, 
                Random.Range(alphaRange.x / 255, alphaRange.y / 255));
	}
}
