using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuadText : MonoBehaviour 
{
    [System.Serializable]
    public class QuadFontCharacter
    {
        public string name;
        public Texture2D art;
        public float width = 0.5f;
        public int[] characterIds;
    }

    public string text = "";
    public TextAlignment textAlignment;
    public float spacing = 0;
    public float wordSpacing = 0.5f;
    public Mesh mesh;
    public Material[] materials;
    public QuadFontCharacter[] fontCharacters;

    bool initialized = false;
    float currentSpacing = 0;
    string currentText = "";
    Transform myTrans;
    List<MeshRenderer> quads = new List<MeshRenderer>();
    MaterialPropertyBlock matPB;

    MeshRenderer createQuad()
    {
        GameObject newQuad = new GameObject();
        MeshFilter meshFilter = newQuad.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = newQuad.AddComponent<MeshRenderer>();

        //for(int i=0; i < materials.Length; i++)
            meshRenderer.materials = materials;

        meshFilter.mesh = mesh;
        newQuad.transform.SetParent(myTrans);
        return meshRenderer;
    }
    void setupQuads()
    {
        //Create extra quads if needed
        if(quads.Count < currentText.Length)
        {
            for (int i = quads.Count; i < currentText.Length; i++)
                quads.Add(createQuad());
        }

        //Switch off all quads
        for (int i = 0; i < quads.Count; i++)
            quads[i].gameObject.SetActive(false);
    }
    QuadFontCharacter getLetterFromFont(char letter)
    {
        for (int i = 0; i < fontCharacters.Length; i++)
            for (int j = 0; j < fontCharacters[i].characterIds.Length; j++)
                if (fontCharacters[i].characterIds[j] == (int)letter)
                    return fontCharacters[i];

        return null;
    }
    void setupLetter(MeshRenderer quad, Texture2D art)
    {
        matPB.Clear();
        quad.GetPropertyBlock(matPB);
        if (art != null)
            matPB.SetTexture("_MainTex", art);

        quad.SetPropertyBlock(matPB);
    }
    void setupLetters()
    {
        float pos = 0;
        QuadFontCharacter letter;
        for (int i = 0; i < currentText.Length; i++)
        {
            if (currentText[i] != ' ')
            {
                letter = getLetterFromFont(currentText[i]);
                setupLetter(quads[i], letter.art);
                quads[i].transform.localPosition = new Vector3(pos, 0, i * 0.0001f);
                quads[i].transform.localRotation = Quaternion.Euler(Vector3.zero);
                quads[i].gameObject.SetActive(true);
                //quad.transform.localScale = MapController.instance.mapLocationVisuals[artID].mainArtScale;

                pos += letter.width + currentSpacing;
            }
            else
                pos += wordSpacing;
        }
    }
    void setup()
    {
        currentText = text;
        currentSpacing = spacing;

        setupQuads();
        setupLetters();
    }

    void Awake()
    {
        myTrans = transform;
        matPB = new MaterialPropertyBlock();
        initialized = true;
    }
    void Update()
    {
        if ((currentText != text) || (currentSpacing != spacing))
        {
            setup();
        }
    }
    void OnDisable()
    {
        quads.Clear();
    }
    void OnEnable()
    {
        if (initialized)
            setup();
    }
}
