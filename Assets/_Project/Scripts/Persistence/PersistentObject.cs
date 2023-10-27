using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    [Tooltip("Identifies this object in the Player's save file")]
    public int id;

    public int GetState()
    {
        //Already looted?
        return GameDatabase.sPlayerData.GetCurrentLocationState()
            .getObjectState(id);
    }

    public void SetState(int state)
    {
        //Update state to looted
        GameDatabase.sPlayerData.GetCurrentLocationState()
            .setObjectState(id, state);
    }
}
