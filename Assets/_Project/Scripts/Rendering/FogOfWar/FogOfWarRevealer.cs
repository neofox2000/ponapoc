using UnityEngine;
using System.Collections;

public class FogOfWarRevealer : MonoBehaviour
{
	public float fowRevealRange = 5.5f;
	private float fowCooldown;

	void Update ()
	{
		HandleFOWRevealing();
	}
	
	protected void HandleFOWRevealing()
	{
		fowCooldown -= Time.deltaTime;
		if (fowCooldown <= 0)
		{
            //FogOfWar.instance.RevealAroundPoint(transform.position, fowRevealRange);
            FogOfWar.instance.RevealAroundPoint(
                new Vector2(
                    transform.position.x,
                    transform.position.z),
                fowRevealRange);

            fowCooldown += FogOfWar.instance.updateInterval;
		}
	}
}