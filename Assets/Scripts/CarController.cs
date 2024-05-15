using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{   
    enum DriveTrain
    {
        FWD,
        RWD,
        AWD
    }

    [Header("Wheels")]
    [SerializeField] Suspension[] wheels;

    [Header("Properties")]
    [SerializeField] public float wheelBase;
    [SerializeField] public float rearTrack;
    [SerializeField] public float turnRadius;
    [SerializeField] DriveTrain driveTrain;
    
    //InputVariables
    float steerInput;
    float accelInput;

    [SerializeField] private float ackermannAngleLeft;
    [SerializeField] private float ackermannAngleRight;

    private Vector3 tractiveForce;
    private Vector3 dragForce;
    private Vector3 rollingResistanceForce;

    private float engineForce = 8000f;
    private float dragConstant = 0.4257f;
    private float rrConstant;

    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rrConstant = 30 * dragConstant;
    }

    // Update is called once per frame
    void Update()
    {
        steerInput = Input.GetAxis("Horizontal");
        accelInput = Input.GetAxis("Vertical");

        if (accelInput < 0.0f)
        {
            foreach(Suspension wheel in wheels) 
            {
                wheel.wheelAccelScale = accelInput;
            }
        }
        else
        {
            PowerDistribution();
        }
        if (steerInput > 0.0f ) 
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
        }
        else if (steerInput < 0.0f )
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
        }
        else
        {
            ackermannAngleLeft = 0.0f;
            ackermannAngleRight = 0.0f;
        }

        foreach(Suspension wheel in wheels) 
        {
            if (wheel.frontLeft)
            {
                wheel.steerAngle = ackermannAngleLeft;
            }
            if (wheel.frontRight)
            {
                wheel.steerAngle = ackermannAngleRight;
            }
        }
    }

    void PowerDistribution()
    {
        switch(driveTrain)
        {
            case DriveTrain.FWD: 
                {
                    foreach(Suspension wheel in wheels) 
                    {
                        if(wheel.frontLeft)
                        {
                            wheel.wheelAccelScale = accelInput;
                        }
                        if(wheel.frontRight)
                        {

                            wheel.wheelAccelScale = accelInput;
                            //GiveAccelInput
                        }
                    }
                    break;
                }
            case DriveTrain.RWD: 
                {
                    foreach (Suspension wheel in wheels)
                    {
                        if(wheel.rearLeft)
                        {

                            //GiveAccelInput
                            wheel.wheelAccelScale = accelInput;
                        }
                        if(wheel.rearRight)
                        {

                            //GiveAccelInput
                            wheel.wheelAccelScale = accelInput;
                        }
                    }
                    break;
                }
            case DriveTrain.AWD: 
                {
                    foreach (Suspension wheel in wheels)
                    {

                        //GiveAccelInput
                        wheel.wheelAccelScale = accelInput;
                    }
                    break;
                }
            default: { break; }
        }
    }

}
