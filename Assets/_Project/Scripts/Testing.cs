using UnityEngine;
using System;
using Variables;

public class Testing : MonoBehaviour 
{
#if UNITY_EDITOR
    public static string debugStr = "";

    public float newX = 90f;
    public bool questStates = false;
    public bool nameKeyDiscovery = false;
    public float inputDeadZone = 0.1f;
    public GameObject[] testobjects;

    DateTime timeStarted;

    void Update()
    {
        if (!GameDatabase.localPlayer) return;

        //ConsumeSP
        if (Input.GetKeyUp(KeyCode.F3))
        {
            GameDatabase.lCharacterSheet.SP.Consume(10f);
        }
        //Teleport forward
        if (Input.GetKeyUp(KeyCode.F4))
        {
            GameDatabase.localPlayer.transform.position += new Vector3(newX, 0, 0);
        }
        //Delete PlayerPrefs
        if (Input.GetKeyUp(KeyCode.F5))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("PlayerPrefs cleared");
        }
        //Level up
        if (Input.GetKeyUp(KeyCode.F6))
        {
            GameDatabase.localPlayer.myActor.GiveXP(
                GameDatabase.lCharacterSheet.XPRequired);
        }
        //Insta-kill player
        if (Input.GetKeyUp(KeyCode.F8))
        {
            GameDatabase.localPlayer.myActor.Hit(new HitData(
                false,
                ForceMatrix.one, 
                1000f, 
                0f, 
                1f, 
                transform.position, 
                null,
                new DamageResult()));
        }
        //Key name discovery
        if (nameKeyDiscovery)
        {
            foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(code))
                {
                    //your code here
                    Debug.Log("Pressed: " + code);
                }
            }
            foreach (AxisCode code in Enum.GetValues(typeof(AxisCode)))
            {
                float value = InputX.Axis(code, true);
                if (Mathf.Abs(value) >= inputDeadZone)
                {
                    //your code here
                    Debug.Log("Axis Moved: " + code + " by " + value);
                }
            }
        }
    }

    void OnGUI()
    {
        if (debugStr != "")
            GUI.Label(new Rect(0, 0, 1000, 40), debugStr);
    }
#endif
}
