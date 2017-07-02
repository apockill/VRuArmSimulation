using UnityEngine;
using System.Collections;
using VRTK;

public class RobotController : MonoBehaviour
{
    public GameObject robot;
    private RobotControl robotScript;

    private void Start()
    {
        robotScript = robot.GetComponent<RobotControl>();
        GetComponent<VRTK_ControllerEvents>().TriggerAxisChanged     += new ControllerInteractionEventHandler(DoTriggerAxisChanged);
        GetComponent<VRTK_ControllerEvents>().TouchpadAxisChanged    += new ControllerInteractionEventHandler(DoTouchpadAxisChanged);
        GetComponent<VRTK_ControllerEvents>().TriggerReleased        += new ControllerInteractionEventHandler(DoTriggerReleased);
        GetComponent<VRTK_ControllerEvents>().TouchpadTouchEnd       += new ControllerInteractionEventHandler(DoTouchpadTouchEnd);
        //GetComponent<VRTK_ControllerEvents>().ApplicationMenuPressed += new ControllerInteractionEventHandler(DoApplicationMenuPressed);
    }

    // Pass touchpad stuff to RC car
    private void DoTouchpadAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        robotScript.SetTouchAxis(e.touchpadAxis);
    }

    private void DoTouchpadTouchEnd(object sender, ControllerInteractionEventArgs e)
    {
        robotScript.SetTouchAxis(Vector2.zero);
    }

    // Pass Trigger stuff to RC 
    private void DoTriggerAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        robotScript.SetTriggerAxis(e.buttonPressure);
    }

    private void DoTriggerReleased(object sender, ControllerInteractionEventArgs e)
    {
        robotScript.SetTriggerAxis(0f);
    }

    //private void DoApplicationMenuPressed(object sender, ControllerInteractionEventArgs e)
    //{
    //    robotScript.Reset();
    //}
}

