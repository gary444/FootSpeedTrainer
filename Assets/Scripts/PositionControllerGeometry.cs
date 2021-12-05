using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionControllerGeometry : MonoBehaviour
{
    public GameObject leftController;
    public GameObject rightController;

    // Start is called before the first frame update
    void Start()
    {
        leftController = gameObject.transform.Find("LeftController").gameObject;
        rightController = gameObject.transform.Find("RightController").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        leftController.transform.position = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand);
        rightController.transform.position = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RHand);
    }
}
