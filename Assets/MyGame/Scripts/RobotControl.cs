using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VRTK;

public class RobotControl : MonoBehaviour
{
    // Script Inputs
    public float maxSpring = 100f;

    // Recordings
    private bool isRecording = false;
    private bool isPlaying = false;
    private int currentState = 0;  // Keep track of the currently playing recording transform
    private struct RecordSlot { public float rotAngle, strAngle, hgtAngle, endAngle; public bool pumpStat;}
    private List<RecordSlot> recording = new List<RecordSlot>();

    // Robot Parts and State
    private HingeJoint rotHinge;
    private HingeJoint strHinge;
    private HingeJoint hgtHinge;
    private HingeJoint endHinge;
    private GameObject sucker;
    private RadialMenu radMenu;

    // Controller/Grabbing Related Variables 
    private Renderer grabSphere;
    private VRTK_InteractableObject grabScript;

    // Pick and Place Variables
    private bool pumpStatus;
    private GameObject touchedObject = null;
    private FixedJoint pumpJoint = null;



    // Keep controller info variables up to date
    public void SetTouchAxis(Vector2 data)
    {
    }

    public void SetTriggerAxis(float data)
    {
    }



    // Monobehaviour Functions
    private void Awake()
    {
        // Get any GameObjects that are needed (or whose components are needed)
        GameObject baseTop        = transform.Find("Base Top").gameObject;
        GameObject linkageBottom  = transform.Find("Linkage Bottom").gameObject;
        GameObject linkageBottom1 = transform.Find("Linkage Bottom 1").gameObject;
        GameObject menuPanel      = transform.Find("Base Bottom/RadialMenu/RadialMenuUI/Panel").gameObject;
        GameObject grabbable      = transform.Find("Grabbable Sphere").gameObject;
        GameObject effector       = transform.Find("Effector End").gameObject;
        sucker                    = transform.Find("Effector End/Effector Sucker").gameObject;

        // Find the "servo" joints
        rotHinge = baseTop.GetComponent<HingeJoint>();
        strHinge = linkageBottom.GetComponent<HingeJoint>();
        hgtHinge = linkageBottom1.GetComponent<HingeJoint>();
        endHinge = effector.GetComponent<HingeJoint>();

        // Get Misc Components from Children
        radMenu    = menuPanel.GetComponent<RadialMenu>();  // Find the RadialMenu
        grabSphere = grabbable.GetComponent<Renderer>();    // Find the Grabbable Sphere Mesh Renderer

        // Testing
        grabScript = grabbable.GetComponent<VRTK_InteractableObject>();


        // Example code- how to make a button
        //RadialMenuButton btn = new RadialMenuButton();
        //btn.ButtonIcon       = Resources.Load<Sprite>("Sprites/record_end");
        //btn.OnClick          = new UnityEngine.Events.UnityEvent();
        //btn.OnHold           = new UnityEngine.Events.UnityEvent();
        //btn.OnHoverEnter     = new UnityEngine.Events.UnityEvent();
        //btn.OnHoverExit      = new UnityEngine.Events.UnityEvent();
        //btn.OnClick.AddListener(deleteThis);
        //Debug.Log(btn.ButtonIcon);
        //radMenu.AddButton(btn);
    }
    
    public void deleteThis()
    {
        Debug.Log("It worked!");
    }

    private void FixedUpdate()
    {
        // Handle modes
        if (isRecording) { record();}
        if (isPlaying)   { play(); }


        // Grab objects if the pump is currently on. Let go of objects if it's not
        grabUpdate();


        // If the user is currently grabbing the robots end-effector
        if (grabScript.GetGrabbingObject() != null)   
        {
            GameObject activeController = grabScript.GetGrabbingObject().gameObject;
            VRTK_ControllerEvents controllerState = activeController.GetComponent<VRTK_ControllerEvents>();

            // If controller is clicked, turn on pump. If controller is not clicked, turn off pump.
            setPump(controllerState.triggerClicked);
        }


    }


    // Grabbing Functions
    public void setPump(bool status)
    {
        pumpStatus = status;         
    }
    
    public void grabUpdate()
    {
        // If the pump is on and touching an object, but no Joint has been attached yet
        if(pumpStatus && touchedObject != null && pumpJoint == null){
            // Attach the object to the pump
            Debug.Log("Attaching " + touchedObject);
            pumpJoint                     = touchedObject.AddComponent<FixedJoint>();
            pumpJoint.connectedBody       = sucker.GetComponent<Rigidbody>();
            pumpJoint.breakForce          = 400;
            pumpJoint.breakTorque         = 100;
            pumpJoint.enableCollision     = true;
            pumpJoint.enablePreprocessing = false;
        }


        // If the pump is off and there is still an object attached
        if (!pumpStatus && touchedObject != null && pumpJoint != null)
        {
            // Detach the object from the pump
            Debug.Log("Detaching " + touchedObject);

            Destroy(pumpJoint);
            pumpJoint = null;
        }

            //// If the pump is on and a there is an object currently being touched
            //if(pumpStatus && touchedObject != null)
            //{
            //    Vector3 direction = (touchedPoint.point - sucker.transform.position).normalized;
            //    touchedObject.GetComponent<Rigidbody>().AddForceAtPosition(direction * 10, sucker.transform.position);

            //    Debug.Log("normalized" + direction + "    touched" + touchedPoint.point + "    sucker" + sucker.transform.position);
            //    // Vector3 offset = new Vector3(0, 0, 1);
            //}
        }

    
    
    // Robot or Controller Events
    public void OnEffectorCollisionEnter(Collision col)
    {
         
        GameObject colObj = col.gameObject;

        if(colObj.GetComponent<Rigidbody>() != null) {
            touchedObject = col.gameObject;
        }
    }

    public void OnEffectorCollisionExit(Collision col)
    {
        touchedObject = null;
        if(pumpJoint != null)
        {
            Debug.Log("Detaching " + touchedObject + " because collision exit");
            Destroy(pumpJoint);
        }

    }


    // Recording Functions
    public void toggleRecording()
    {
        isRecording = !isRecording;
        if (isRecording){
            startRecording();
        } else {
            resetState();
        }
    }

    public void startRecording()
    {
        resetState();

        isRecording = true;
        recording.Clear();

        // Set the Recording Button to the correct state
        RadialMenuButton recordBtn = radMenu.GetButton(0);
        recordBtn.ButtonIcon = Resources.Load<Sprite>("Sprites/button_record_end");
        radMenu.RegenerateButtons();

        // Show a sphere around the end effector, indicating it can be grabbed
        grabSphere.enabled = true;
    }

    public void record()
    {

        RecordSlot rec;
        rec.rotAngle = rotHinge.angle;
        rec.strAngle = strHinge.angle;
        rec.hgtAngle = hgtHinge.angle;
        rec.endAngle = endHinge.angle;
        rec.pumpStat = pumpStatus;
        recording.Add(rec);
    }



    // Playing Functions
    public void togglePlaying()
    {
        isPlaying = !isPlaying;
        if (isPlaying)
        {
            startPlaying();
        }
        else
        {
            resetState();
        }
    }

    public void startPlaying()
    {
        resetState();
        isPlaying = true;
        attachServos();

        // Set the Play icon to the "Stop" icon
        RadialMenuButton playBtn = radMenu.GetButton(1);
        playBtn.ButtonIcon = Resources.Load<Sprite>("Sprites/button_replay_stop");
        radMenu.RegenerateButtons();
    }

    public void play()
    {
        // If the recording is over, stop playing
        if (currentState >= recording.Count)
        {
            currentState = 0;
            return;
        }

        RecordSlot recordSlot = recording[currentState];

        setServoAngle(rotHinge, recordSlot.rotAngle);
        setServoAngle(strHinge, recordSlot.strAngle);
        setServoAngle(hgtHinge, recordSlot.hgtAngle);
        setServoAngle(endHinge, recordSlot.endAngle);

        setPump(recordSlot.pumpStat);

        currentState += 1;

        Debug.Log(recordSlot.pumpStat);
    }



    // Control Robot
    public void resetState()
    {
        // Go to the default state, with no playing, recording, and where servos are free

        // Reset Recording
        isRecording = false;
        currentState = 0;

        // Reset Playing
        isPlaying = false;
        detachServos();

        // Reset all Buttons
        RadialMenuButton recordBtn = radMenu.GetButton(0);
        recordBtn.ButtonIcon = Resources.Load<Sprite>("Sprites/button_record_start");

        RadialMenuButton playBtn = radMenu.GetButton(1);
        playBtn.ButtonIcon = Resources.Load<Sprite>("Sprites/button_replay_play");
        radMenu.RegenerateButtons();

        // Reset Mesh Renderer for the end effector grabbing object
        grabSphere.enabled = false;
    }

    public void destroy()
    {
        // Destroy the uArm
        Destroy(this.gameObject);
    }

    public void attachServos()
    {
        rotHinge.useSpring = true;
        strHinge.useSpring = true;
        hgtHinge.useSpring = true;
    }

    public void detachServos()
    {
        rotHinge.useSpring = false;
        strHinge.useSpring = false;
        hgtHinge.useSpring = false;
    }

    private void setServoAngle(HingeJoint servo, float angle)
    {
        JointSpring hingeSpring = servo.spring;
        hingeSpring.spring = maxSpring;
        hingeSpring.damper = 3;
        hingeSpring.targetPosition = angle;
        servo.spring = hingeSpring;
        servo.useSpring = true;
    }
}
