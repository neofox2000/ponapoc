using UnityEngine;

public class GameobjectEnabler : MonoBehaviour 
{
    public Transform[] gameObjects;
    public RectTransform[] guiObjects;
	
    // Use this for initialization
	void Awake () 
    {
        foreach (Transform T in gameObjects)
            if(T) T.gameObject.SetActive(true);

        foreach (RectTransform RT in guiObjects)
            if (RT) RT.gameObject.SetActive(true);

        //Free up memory
        Destroy(gameObject);
	}
}
