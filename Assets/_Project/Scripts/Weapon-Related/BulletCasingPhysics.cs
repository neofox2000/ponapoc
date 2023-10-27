using UnityEngine;
using System.Collections;
using GameDB;

public class BulletCasingPhysics : DepthPhysicsMotor
{
    public Vector2 minVelocity = new Vector2(-1, 1);
    public Vector2 maxVelocity = new Vector2(-3, 3);
    [SerializeField] AudioGroupTemplate sound;

    [HideInInspector]
    public int dir = 1;

    IEnumerator init()
    {
        //Init values
        stop();

        //Wait one frame for object to fully spawn and position itself
        yield return null;

        //Start physics simulation
        startUp(
            new Vector3(
                Random.Range(minVelocity.x, maxVelocity.x) * dir,
                Random.Range(minVelocity.y, maxVelocity.y),
                0), 
            true);
    }

    public void doInit(int direction, float groundLevel)
    {
        dir = direction;
        this.groundLevel = groundLevel;
        StartCoroutine(init());
    }
    protected override void OnCollided(CollisionTypes collisionType)
    {
        base.OnCollided(collisionType);
        AudioManager.instance.Play(sound, transform);
    }
}
