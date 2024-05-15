using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WheelPhysics : MonoBehaviour
{   
    Rigidbody rb;
    Transform tire;

    [SerializeField] public bool frontLeft;
    [SerializeField] public bool frontRight;
    [SerializeField] public bool rearLeft;
    [SerializeField] public bool rearRight;

    [Header("Suspension")]
    [SerializeField] float springRestLength;
    [SerializeField] float maxSpringTravel;
    [SerializeField] float springStrength;
    [SerializeField] float damperStrength;

    private float minSpringLength;
    private float maxSpringLength;
    private float lastSpringLength;
    private float springLength;
    private float springOffset;
    private float springVelocity;
    private float springForce;
    private float damperForce;

    private Vector3 suspensionForce;
    private Vector3 steerForce;
    private Vector3 torqueForce;

    [Header("Wheel")]
    [SerializeField] float wheelRadius;
    [SerializeField] float wheelAngle;
    [SerializeField] float wheelMass = 7f;
    [SerializeField] public float steerAngle;
    [SerializeField] float steerTime = 8f;
    [SerializeField] float gripFactor;
    public float wheelAccelScale = 0.0f;
    //[HideInInspector] 

    [Header("Forces")]
    private Vector3 tractiveForce;
    private Vector3 dragForce;
    private Vector3 rollingResistanceForce;

    [SerializeField] float engineForce = 8000f;
    private float dragConstant = 0.4257f;
    private float rrConstant = 12.771f;


    [Header("Debug")]
    [SerializeField] float debugForce = 1000f;

    // Start is called before the first frame update
    void Start()
    {
        rb = transform.root.GetComponent<Rigidbody>();
        tire = transform.GetChild(transform.childCount - 1);
        minSpringLength = springRestLength - maxSpringTravel;
        maxSpringLength = springStrength + maxSpringTravel;
    }

    private void Update()
    {
        wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, steerTime);
        transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
        tire.localRotation = transform.localRotation;
        Debug.DrawRay(transform.position, -transform.up * (springLength + wheelRadius), Color.green);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        bool didCastHit = Physics.Raycast(transform.position, -transform.up, out RaycastHit hitInfo, maxSpringLength + wheelRadius);
        if (didCastHit)
        {
            Vector3 tireVel = rb.GetPointVelocity(transform.position);
            
            //Suspension
            lastSpringLength = springLength;
            springLength = hitInfo.distance - wheelRadius;
            //Clamp Spring
            springLength = Mathf.Clamp(springLength, minSpringLength, maxSpringLength);
            springVelocity = (lastSpringLength - springLength) / Time.fixedDeltaTime;
            float tireSpeed = Vector3.Dot(transform.up, tireVel);
            springOffset = springRestLength - springLength;
            springForce = springStrength * springOffset;
            damperForce = damperStrength * springVelocity;
            //damperForce = damperStrength * tireSpeed;

            //SteeringPhysics
            Vector3 steerDir = transform.right;
            float steerVelMagnitude = Vector3.Dot(steerDir, tireVel);
            float desiredChangeInVel = -steerVelMagnitude * gripFactor;
            float desiredTireAccel = desiredChangeInVel / Time.fixedDeltaTime;

            //Acceleration
            Vector3 accelDir = transform.forward;
            float carSpeed = Vector3.Dot(accelDir, rb.velocity);
            float torque = engineForce * wheelAccelScale;

            //Calculate Forces
            suspensionForce = (springForce + damperForce) * transform.up;
            steerForce = steerDir * desiredTireAccel * wheelMass;
            torqueForce = torque * accelDir;
            rb.AddForceAtPosition(suspensionForce, transform.position);
        }

    }
}
