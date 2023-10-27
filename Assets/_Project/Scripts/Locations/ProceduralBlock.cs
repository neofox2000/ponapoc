using UnityEngine;

public class ProceduralBlock : MonoBehaviour 
{
    [System.Serializable]
    public struct ProceduralPart
    {
        [Tooltip("The transform for the part that will be shown.")]
        public Transform part;

        [Tooltip("The chance that the part will be shown (1 = 100%)")]
        [Range(0f,1f)]
        public float chance;
    }

    public Vector2 blockSize;

    public ProceduralPart[] proceduralParts;

    public void Awake()
    {
        foreach (ProceduralPart PP in proceduralParts)
            PP.part.gameObject.SetActive(
                Random.Range(0f, 1f) < PP.chance);
    }
}
