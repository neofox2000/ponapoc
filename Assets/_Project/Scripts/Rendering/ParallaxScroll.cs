using UnityEngine;
using System.Collections;

public class ParallaxScroll : MonoBehaviour
{
    public float speed, width;
    Transform camTrans;

    void Start()
    {
        StartCoroutine(StartDelayed());
    }
    IEnumerator StartDelayed()
    {
        while (!Camera.main)
            yield return new WaitForEndOfFrame();

        camTrans = Camera.main.transform;
    }
    void Update () 
    {
        if (camTrans)
        {
            float camX = camTrans.position.x;
            //float offSetX = (camX % (width / speed)) * speed;
            float offSetX = (camX * speed) % width;

            //Debug.Log((camX % width) + ": " + camX + " - " + offSetX + " = " + (camX - offSetX));

            Vector3 pos = transform.position;
            transform.position = new Vector3(camX - offSetX, pos.y, pos.z);
        }
	}
}
