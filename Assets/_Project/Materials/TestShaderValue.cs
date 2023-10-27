using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class TestShaderValue : MonoBehaviour 
{
    public Color glowColor;

    MaterialPropertyBlock matPropertyBlock;
    SpriteRenderer spriteRenderer;
    Color lastColor;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if(lastColor != glowColor)
        {
            lastColor = glowColor;
            matPropertyBlock = new MaterialPropertyBlock();
            spriteRenderer.GetPropertyBlock(matPropertyBlock);
            matPropertyBlock.SetColor("_Glow", glowColor);
            spriteRenderer.SetPropertyBlock(matPropertyBlock);
        }
    }
}
