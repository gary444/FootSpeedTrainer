using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCollisions : MonoBehaviour
{
    public bool collisionDetected = false;

    public bool wasCollisionDetected()
    {
        bool rtnVal = collisionDetected;
        collisionDetected = false;
        return rtnVal;
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collision enter: " + collision.gameObject.name);

        if (collision.gameObject.name.Contains("Controller"))
        {
            collisionDetected = true;
        }

    }
}
