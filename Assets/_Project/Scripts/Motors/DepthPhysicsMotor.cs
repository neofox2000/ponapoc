using UnityEngine;

public class DepthPhysicsMotor : MonoBehaviour
{
    protected enum CollisionTypes { Ground, Other };

    #region Public Properties
    public Vector2 spinSpeedLimits = new Vector2(1, 10);
    public Vector3 gravity = new Vector3(0, -0.1f, 0);

    [HideInInspector]
    public float groundLevel = 0;
    #endregion

    #region Private Properties
    private float spinSpeed = 0;
    private bool grounded = false;

    private Vector3 _velocity = Vector3.zero;
    public Vector3 velocity
    {
        get { return _velocity; }
        set { _velocity = value; }
    }

    protected Transform myTrans;
    #endregion

    public void startUp(Vector3 initialVelocity, bool spinning)
    {
        myTrans = transform;

        grounded = false;
        velocity = initialVelocity;

        if (spinning)
            spinSpeed = Random.Range(spinSpeedLimits.x, spinSpeedLimits.y) * 100;
        else
            spinSpeed = 0;
    }
    public void stop()
    {
        //this.grounded = grounded;
        velocity = Vector3.zero;
        spinSpeed = 0;
    }

    protected virtual void Update()
    {
        if ((!grounded) && (myTrans))
        {
            if (myTrans.position.y > groundLevel)
            {
                if (spinSpeed != 0)
                {
                    myTrans.Rotate(Vector3.forward * Time.deltaTime * spinSpeed);
                }

                if (velocity != Vector3.zero)
                {
                    velocity += gravity;
                    myTrans.position = myTrans.position + (velocity * Time.deltaTime);
                }
            }
            else
            {
                myTrans.position = new Vector3(myTrans.position.x, groundLevel, myTrans.position.z);
                spinSpeed = 0;
                velocity = Vector3.zero;

                OnCollided(CollisionTypes.Ground);
            }
        }
    }
    protected virtual void OnCollided(CollisionTypes collisionType)
    {
        switch(collisionType)
        {
            case CollisionTypes.Ground:
                grounded = true;
                break;
            default:
                break;
        }
    }
}
