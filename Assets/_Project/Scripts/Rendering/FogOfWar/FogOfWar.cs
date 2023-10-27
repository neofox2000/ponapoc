using UnityEngine;

public class FogOfWar : MonoBehaviour
{
    //Enums
    public enum FOWState
    {
        Unexplored = 0,
        Forgotten = 1,
        Revealed = 2
    }

    #region Properties
    //Static
    public static FogOfWar instance;

    //Private
    FOWState[,] fowCurrentStateArray;
    FOWState[,] fowNextStateArray;
    Color[] fowColorArray;
    Texture2D fowTexture;
    float cooldown;

    //Public
    public float _updateInterval = .5f;
	public int width = 32;
	public int height = 32;
	public Color unexploredColor;
	public Color forgottenColor;
	public Color revealedColor;

    //Accessors
    public float updateInterval 
    {
        get { return instance._updateInterval; } 
    }
    #endregion

    #region Methods
    //Monobehaviours
    void Awake()
	{
		instance = this;

		fowCurrentStateArray = new FOWState[width, height];
		fowNextStateArray = new FOWState[width, height];

		fowTexture = new Texture2D(width, height);
		fowTexture.wrapMode = TextureWrapMode.Clamp;
		fowTexture.filterMode = FilterMode.Bilinear;

		fowColorArray = new Color[width * height];

		instance.transform.localScale = new Vector3(width, height);
		//instance.transform.position = new Vector3(width / 2 - 0.5f, height / 2 - 0.5f, instance.transform.position.z);
        instance.transform.position = new Vector3(width / 2 - 0.5f, instance.transform.position.y, height / 2 - 0.5f);
		
        instance.GetComponent<Renderer>().material.mainTexture = fowTexture;
		instance.GetComponent<Renderer>().enabled = true;

        gameObject.SetActive(false);
	}
	void Update()
	{
		cooldown -= Time.deltaTime;
        if (cooldown <= 0)
        {
            cooldown += _updateInterval;
            UpdateFogOfWar();
        }
	}
	
    //Data Persistence
    public void setState(FOWState[,] savedFogState)
    {
        //Bail if bad or absent data
        if (savedFogState == null) return;

        /*
        int c = 0;
        for (int x = 0; x < savedFogState.GetLength(0); x++)
            for (int y = 0; y < savedFogState.GetLength(1); y++)
                if (savedFogState[x, y] != FOWState.Unexplored)
                    c++;

        Debug.Log("Explored Count: " + c);
        */

        fowCurrentStateArray = savedFogState;
        fowNextStateArray = savedFogState;
    }
    public FOWState[,] getState()
    {
        int width = fowCurrentStateArray.GetLength(0);
        int height = fowCurrentStateArray.GetLength(1);

        FOWState[,] copyOfState = new FOWState[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                copyOfState[x, y] = fowCurrentStateArray[x, y];

        return copyOfState;
    }

    //Main Functions
    void UpdateFogOfWar()
    {
        int arrayPos;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Update current state to next state
                fowCurrentStateArray[x, y] = fowNextStateArray[x, y];

                //arrayPos = x * height + y;
                arrayPos = x + y * width;

                // Fill the color array with the correct colors
                switch (fowCurrentStateArray[x, y])
                {
                    case FOWState.Unexplored: fowColorArray[arrayPos] = unexploredColor; break;
                    case FOWState.Forgotten: fowColorArray[arrayPos] = forgottenColor; break;
                    case FOWState.Revealed: fowColorArray[arrayPos] = revealedColor; break;
                }

                // Forget recently revealed parts
                if (fowNextStateArray[x, y] == FOWState.Revealed)
                    fowNextStateArray[x, y] = FOWState.Forgotten;
            }
        }

        fowTexture.SetPixels(fowColorArray);
        fowTexture.Apply();
    }
    public void RevealAroundPoint(Vector2 aorigin, float arange)
	{
        //Bail if FogOfWar object not present
        if (fowNextStateArray == null) return;

		int startX = Mathf.RoundToInt(aorigin.x);
		int startY = Mathf.RoundToInt(aorigin.y);

		int ceiledRange = Mathf.CeilToInt(arange);

		int minX = Mathf.Max(0, startX - ceiledRange);
		int minY = Mathf.Max(0, startY - ceiledRange);
		int maxX = Mathf.Min(startX + ceiledRange, fowNextStateArray.GetLength(0));
		int maxY = Mathf.Min(startY + ceiledRange, fowNextStateArray.GetLength(1));

		//if (minX < 0) minX = 0;
		//if (minY < 0) minY = 0;
		//if (maxX > fowNextStateArray.GetLength(0)) maxX = fowNextStateArray.GetLength(0);
		//if (maxY > fowNextStateArray.GetLength(1)) maxY = fowNextStateArray.GetLength(1);

		arange *= arange;

		for (int x = minX; x < maxX; x++)
			for (int y = minY; y < maxY; y++)
				if (((x - startX) * (x - startX)) + ((y - startY) * (y - startY)) <= arange)
					fowNextStateArray[x, y] = FOWState.Revealed;
	}
	public bool VisibilityTest(Vector2 aposition)
	{
		return fowCurrentStateArray[Mathf.RoundToInt(aposition.x), Mathf.RoundToInt(aposition.y)] == FOWState.Revealed;
    }
    #endregion
}