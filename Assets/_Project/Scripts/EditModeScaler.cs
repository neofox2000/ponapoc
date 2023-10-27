using UnityEngine;

[ExecuteInEditMode]
public class EditModeScaler : MonoBehaviour
{
    [Range(0.25f, 3f)]
    public float scaleSetting = 1f;

	void Update ()
    {
        if (transform.localScale.x != scaleSetting)
            transform.localScale = Vector3.one * scaleSetting;
	}
}
