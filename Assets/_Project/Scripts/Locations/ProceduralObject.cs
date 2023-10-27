using UnityEngine;

public class ProceduralObject : MonoBehaviour
{
    [Range(0f, 1f)]
    public float spawnChance = 0.5f;
	
	void Start()
    {
        gameObject.SetActive(Random.Range(0f, 1f) < spawnChance);
    }
}
